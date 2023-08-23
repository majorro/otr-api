using API.Entities;
using API.Osu.Multiplayer;

namespace API.Osu;

public class Converters
{
	public static class OsuMultiplayerLobbyData
	{
		public static List<MatchData> FromMultiplayerLobbyData(MultiplayerLobbyData data)
		{
			var matchDatas = new List<MatchData>();

			foreach (var game in data.Games)
			{
				foreach (var score in game.Scores)
				{
					var matchData = new MatchData
					{
						OsuMatchId = long.TryParse(data.Match.MatchId, out long osuMatchId) ? osuMatchId : 0,
						GameId = long.TryParse(game.GameId, out long gameId) ? gameId : 0,
						ScoringType = game.ScoringType,
						OsuBeatmapId = long.TryParse(game.BeatmapId, out long beatmapId) ? beatmapId : 0,
						GameRawMods = int.TryParse(game.Mods, out int gameRawMods) ? gameRawMods : 0,
						RawMods = int.TryParse(score.EnabledMods, out int rawMods) ? rawMods : 0,
						MatchName = data.Match.Name,
						Mode = game.PlayMode,
						MatchStartDate = data.Match.StartTime,
						TeamType = game.TeamType,
						Team = score.Team,
						Score = double.TryParse(score.PlayerScore, out double playerScore) ? playerScore : 0,
						PlayerId = int.TryParse(score.UserId, out int playerId) ? playerId : 0
					};

					matchDatas.Add(matchData);
				}
			}

			return matchDatas;
		}
	}
}