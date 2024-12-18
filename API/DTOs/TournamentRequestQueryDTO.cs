using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using API.DTOs.Interfaces;
using Database.Enums;

namespace API.DTOs;

/// <summary>
/// Filtering parameters for tournaments requests
/// </summary>
public class TournamentRequestQueryDTO : IPaginated
{
    [Required]
    [Range(1, int.MaxValue)]
    public int Page { get; init; }

    [Required]
    [Range(1, 100)]
    public int PageSize { get; init; }

    /// <summary>
    /// Filters results for only tournaments that are verified
    /// </summary>
    [DefaultValue(true)]
    public bool Verified { get; init; } = true;

    /// <summary>
    /// Filters results for only tournaments played in a specified ruleset
    /// </summary>
    [EnumDataType(typeof(Ruleset))]
    public Ruleset? Ruleset { get; init; }

    /// <summary>
    /// The key used to sort results by
    /// </summary>
    [DefaultValue(TournamentQuerySortType.StartTime)]
    [EnumDataType(typeof(TournamentQuerySortType))]
    public TournamentQuerySortType Sort { get; init; } = TournamentQuerySortType.StartTime;

    /// <summary>
    /// Whether the results are sorted in descending order by the <see cref="Sort"/>
    /// </summary>
    public bool Descending { get; init; }
}
