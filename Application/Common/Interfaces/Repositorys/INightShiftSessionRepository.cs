using Domain.Simulations.NightShift;

namespace Application.Common.Interfaces.Repositorys;

public interface INightShiftSessionRepository : IBaseRepository<NightShiftSessionEntity>
{
    Task<NightShiftSessionEntity?> GetByGuidAsync(Guid sessionGuid);

    Task<List<NightShiftSessionEntity>> GetByUserGuidAsync(Guid userGuid);

    Task AddMessageAsync(NightShiftMessageEntity message);

    Task AddFeedbackAsync(NightShiftFeedbackEntity feedback);
}
