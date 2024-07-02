using Database.Entities;
using Database.Enums.Verification;
using DataWorkerService.Processors.Resolvers.Interfaces;

namespace DataWorkerService.Processors.Tournaments;

/// <summary>
/// Processor tasked with fetching data from outside sources for a <see cref="Tournament"/>
/// </summary>
public class TournamentDataProcessor(
    ILogger<TournamentDataProcessor> logger,
    IMatchProcessorResolver matchProcessorResolver
) : ProcessorBase<Tournament>(logger)
{
    public override async Task OnProcessingAsync(Tournament entity, CancellationToken cancellationToken)
    {
        if (entity.ProcessingStatus is not TournamentProcessingStatus.NeedsData)
        {
            logger.LogDebug(
                "Tournament does not require processing [Id: {Id} | Processing Status: {Status}]",
                entity.Id,
                entity.ProcessingStatus
            );

            return;
        }

        IProcessor<Match> matchDataProcessor = matchProcessorResolver.GetDataProcessor();
        foreach (Match match in entity.Matches)
        {
            await matchDataProcessor.ProcessAsync(match, cancellationToken);
        }

        logger.LogInformation(
            "Tournament data processing summary " +
            "[Matches: {MCnt} | Games: {GCnt} | Beatmaps: {BCnt} | Scores: {SCnt} | Players: {PCnt}]",
            entity.Matches.Count,
            entity.Matches.SelectMany(m => m.Games).Count(),
            entity.Matches.SelectMany(m => m.Games.Select(g => g.Beatmap)).Distinct().Count(),
            entity.Matches.SelectMany(m => m.Games.SelectMany(g => g.Scores)).Count(),
            entity.Matches.SelectMany(m => m.Games.SelectMany(g => g.Scores).Select(s => s.Player)).Distinct().Count()
        );

        // If all matches completed data processing, advance processing status
        if (entity.Matches.All(m => m.ProcessingStatus > MatchProcessingStatus.NeedsData))
        {
            entity.ProcessingStatus += 1;
        }
    }
}
