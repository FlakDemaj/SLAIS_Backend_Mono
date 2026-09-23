using Application.Common.Interfaces.Repositorys;

using Domain.Simulations.NightShift;

using Infrastructure.Persistence.Context;
using Infrastructure.Repositorys.Base;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositorys;

public class NightShiftSessionRepository : BaseRepository<NightShiftSessionEntity>, INightShiftSessionRepository
{
    public NightShiftSessionRepository(SlaisDbContext context)
        : base(context)
    {
    }

    public Task<NightShiftSessionEntity?> GetByGuidAsync(Guid sessionGuid)
    {
        return _context
            .GetTrackingSet<NightShiftSessionEntity>()
            .Include(session => session.Messages.OrderBy(message => message.SortOrder))
            .Include(session => session.Feedback)
            .FirstOrDefaultAsync(session => session.Guid == sessionGuid);
    }

    public Task<List<NightShiftSessionEntity>> GetByUserGuidAsync(Guid userGuid)
    {
        return _context
            .GetNoTrackingSet<NightShiftSessionEntity>()
            .Include(session => session.Messages.OrderBy(message => message.SortOrder))
            .Include(session => session.Feedback)
            .Where(session => session.UserGuid == userGuid)
            .OrderByDescending(session => session.StartedAt)
            .ToListAsync();
    }

    public Task AddMessageAsync(NightShiftMessageEntity message)
    {
        _context.Add(message);

        return Task.CompletedTask;
    }

    public Task AddFeedbackAsync(NightShiftFeedbackEntity feedback)
    {
        _context.Add(feedback);

        return Task.CompletedTask;
    }
}
