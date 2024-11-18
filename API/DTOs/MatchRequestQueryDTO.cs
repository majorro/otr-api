using System.ComponentModel.DataAnnotations;
using API.DTOs.Interfaces;
using Database.Enums;
using Database.Enums.Queries;
using Database.Enums.Verification;

namespace API.DTOs;

/// <summary>
/// Filtering parameters for matches requests
/// </summary>
public class MatchRequestQueryDTO : IPaginatedRequestQueryDTO
{
    [Required]
    [Range(1, int.MaxValue)]
    public int Page { get; init; }

    [Required]
    [Range(1, 100)]
    public int PageSize { get; init; }

    /// <summary>
    /// Filters results for only matches played in a specified ruleset
    /// </summary>
    public Ruleset? Ruleset { get; init; }

    /// <summary>
    /// Filters results for only matches with a partially matching name
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Filters results for only matches that occurred after a specified date
    /// </summary>
    public DateTime? DateMin { get; init; }

    /// <summary>
    /// Filters results for only matches that occurred before a specified date
    /// </summary>
    public DateTime? DateMax { get; init; }

    /// <summary>
    /// Filters results for only matches with a specified verification status
    /// </summary>
    public VerificationStatus? VerificationStatus { get; init; }

    /// <summary>
    /// Filters results for only matches with a specified rejection reason
    /// </summary>
    public MatchRejectionReason? RejectionReason { get; init; }

    /// <summary>
    /// Filters results for only matches with a specified processing status
    /// </summary>
    public MatchProcessingStatus? ProcessingStatus { get; init; }

    /// <summary>
    /// Filters results for only matches submitted by a user with a specified id
    /// </summary>
    public int? SubmittedBy { get; init; }

    /// <summary>
    /// Filters results for only matches verified by a user with a specified id
    /// </summary>
    public int? VerifiedBy { get; init; }

    /// <summary>
    /// The key used to sort results by
    /// </summary>
    public MatchQuerySortType? Sort { get; init; } = MatchQuerySortType.StartTime;

    /// <summary>
    /// Whether the results are sorted in descending order by the <see cref="Sort"/>
    /// </summary>
    public bool? Descending { get; init; }
}
