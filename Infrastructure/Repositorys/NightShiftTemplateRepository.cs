using Application.Common.Interfaces.Repositorys;

using Domain.Common.Enums;
using Domain.Simulations.NightShift;

using Infrastructure.Persistence.Context;
using Infrastructure.Repositorys.Base;

using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositorys;

public class NightShiftTemplateRepository : BaseRepository<NightShiftTemplateEntity>, INightShiftTemplateRepository
{
    public NightShiftTemplateRepository(SlaisDbContext context)
        : base(context)
    {
    }

    public Task<List<NightShiftTemplateEntity>> GetActiveAsync()
    {
        return _context
            .GetNoTrackingSet<NightShiftTemplateEntity>()
            .Include(template => template.Texts)
            .Where(template => template.State == States.Active && template.DeleteDate == null)
            .OrderBy(template => template.SortOrder)
            .ThenBy(template => template.Key)
            .ToListAsync();
    }

    public Task<NightShiftTemplateEntity?> GetActiveByKeyAsync(string key)
    {
        return _context
            .GetTrackingSet<NightShiftTemplateEntity>()
            .Include(template => template.Texts)
            .FirstOrDefaultAsync(template => template.State == States.Active
                && template.DeleteDate == null
                && template.Key == key);
    }

    public Task<NightShiftTemplateEntity?> GetByGuidAsync(Guid templateGuid)
    {
        return _context
            .GetTrackingSet<NightShiftTemplateEntity>()
            .Include(template => template.Texts)
            .FirstOrDefaultAsync(template => template.Guid == templateGuid
                && template.State != States.Deleted);
    }

    public Task<NightShiftTemplateEntity?> GetByKeyAsync(string key)
    {
        return _context
            .GetTrackingSet<NightShiftTemplateEntity>()
            .Include(template => template.Texts)
            .FirstOrDefaultAsync(template => template.Key == key
                && template.State != States.Deleted);
    }

    public Task<List<NightShiftTemplateEntity>> GetAllAsync()
    {
        return _context
            .GetNoTrackingSet<NightShiftTemplateEntity>()
            .Include(template => template.Texts)
            .Where(template => template.State != States.Deleted)
            .OrderBy(template => template.SortOrder)
            .ThenBy(template => template.Key)
            .ToListAsync();
    }
}
