// ReSharper disable CommentTypo

namespace API.DTOs;

/// <summary>
/// Represents a tournament
/// </summary>
public class TournamentDTO : TournamentCompactDTO
{
    /// <summary>
    /// All associated match data
    /// </summary>
    /// <remarks>Will be empty for bulk requests such as List</remarks>
    public ICollection<MatchDTO> Matches { get; init; } = new List<MatchDTO>();
}
