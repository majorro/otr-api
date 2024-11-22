using System.Diagnostics.CodeAnalysis;
using Database.Entities;
using Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories.Implementations;

[SuppressMessage("Performance", "CA1862:Use the \'StringComparison\' method overloads to perform case-insensitive string comparisons")]
[SuppressMessage("ReSharper", "SpecifyStringComparison")]
public class UserRepository(OtrContext context, IUserSettingsRepository userSettingsRepository) : RepositoryBase<User>(context), IUserRepository
{
    private readonly OtrContext _context = context;

    public override async Task<User> CreateAsync(User entity)
    {
        entity.Settings = await userSettingsRepository.GenerateDefaultAsync(entity.PlayerId);
        return await base.CreateAsync(entity);
    }

    public override async Task<User?> GetAsync(int id) =>
        await UserBaseQuery().FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByPlayerIdAsync(int playerId, bool loadSettings = false)
    {
        IQueryable<User> query = _context.Users;
        query = loadSettings
            ? query.Include(u => u.Settings)
            : query;

        return await query.FirstOrDefaultAsync(u => u.PlayerId == playerId);
    }

    public async Task<User> GetByPlayerIdOrCreateAsync(int playerId)
    {
        User? user = await GetByPlayerIdAsync(playerId);
        if (user is not null)
        {
            return user;
        }

        return await CreateAsync(new User { PlayerId = playerId });
    }

    public async Task<int?> GetPlayerIdAsync(int id) =>
        await _context.Users.Where(u => u.Id == id).Select(u => u.PlayerId).FirstOrDefaultAsync();

    public async Task<IEnumerable<OAuthClient>> GetClientsAsync(int id) =>
        await _context.Users.Where(u => u.Id == id).Select(u => u.Clients).FirstOrDefaultAsync()
        ?? new List<OAuthClient>();

    public async Task<IEnumerable<Match>> GetSubmissionsAsync(int id) =>
        await _context.Users.Where(u => u.Id == id).Select(u => u.SubmittedMatches).FirstOrDefaultAsync()
        ?? new List<Match>();

    private IQueryable<User> UserBaseQuery() =>
        _context.Users
            .Include(x => x.Settings)
            .Include(x => x.Player);
}
