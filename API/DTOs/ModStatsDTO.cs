namespace API.DTOs;

/// <summary>
///  Represents some information about a player's mod stats.
///  e.g. how many times has the player played/won with some mod?
/// </summary>
public class ModStatsDTO
{
	public int GamesPlayed { get; set; }
	public int GamesWon { get; set; }
	public double Winrate { get; set; }
}