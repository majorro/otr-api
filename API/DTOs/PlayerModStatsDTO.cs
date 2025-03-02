using Common.Enums.Enums;

namespace API.DTOs;

/// <summary>
/// Represents counts of participation in games of differing mod combinations
/// </summary>
public class PlayerModStatsDTO
{
    public Mods Mods { get; set; }
    public int Count { get; set; }
    public int AverageScore { get; set; }
}
