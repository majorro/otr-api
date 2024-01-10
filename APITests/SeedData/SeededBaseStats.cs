using API.Entities;

namespace APITests.SeedData;

public static class SeededBaseStats
{
	public static BaseStats Get() => new()
	{
		Id = 1,
		PlayerId = 1,
		Mode = 0,
		Rating = 1245.324,
		Volatility = 100.5231,
		Percentile = 0.3431,
		GlobalRank = 20,
		CountryRank = 2,
		MatchCostAverage = 1.23424,
		Created = new DateTime(2023, 11, 11),
		Updated = new DateTime(2023, 11, 12)
	};

	public static List<BaseStats> GetLeaderboard(int size = 25)
	{
		var lb = new List<BaseStats>();
		for (int i = 0; i < size; i++)
		{
			lb.Add(new BaseStats
			{
				Id = i,
				PlayerId = i,
				Mode = 0,
				Rating = 1245.324,
				Volatility = 100.5231,
				Percentile = 0.3431,
				GlobalRank = 20,
				CountryRank = 2,
				MatchCostAverage = 1.23424,
				Created = new DateTime(2023, 11, 11),
				Updated = new DateTime(2023, 11, 12)
			});
		}

		return lb;
	}
}