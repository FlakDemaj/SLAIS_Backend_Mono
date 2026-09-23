using Domain.Simulations.NightShift;

namespace Application.Common.Interfaces.Repositorys;

public interface INightShiftTemplateRepository : IBaseRepository<NightShiftTemplateEntity>
{
    Task<List<NightShiftTemplateEntity>> GetActiveAsync();

    Task<NightShiftTemplateEntity?> GetActiveByKeyAsync(string key);

    Task<NightShiftTemplateEntity?> GetByGuidAsync(Guid templateGuid);

    Task<List<NightShiftTemplateEntity>> GetAllAsync();
}
