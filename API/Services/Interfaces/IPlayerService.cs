using API.DTOs;
using API.Osu;

namespace API.Services.Interfaces;

public interface IPlayerService
{
	Task<IEnumerable<PlayerDTO>> GetAllAsync();
	Task<PlayerDTO?> GetByOsuIdAsync(long osuId, bool eagerLoad = false, OsuEnums.Mode mode = OsuEnums.Mode.Standard, int offsetDays = -1);
	Task<IEnumerable<PlayerDTO>> GetByOsuIdsAsync(IEnumerable<long> osuIds);
	Task<IEnumerable<PlayerRanksDTO>> GetAllRanksAsync();
	Task<IEnumerable<PlayerRatingDTO>> GetTopRatingsAsync(int n, OsuEnums.Mode mode);
	Task<string?> GetUsernameAsync(long osuId);
	Task<int?> GetIdAsync(long osuId);
	Task<long?> GetOsuIdAsync(int id);
}