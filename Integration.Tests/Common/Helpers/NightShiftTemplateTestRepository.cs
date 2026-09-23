using Domain.Simulations.NightShift;

using Infrastructure.Persistence.Context;

using Tests.Domain.Shared.TestDataCreator;

namespace Integration.Tests.Common.Helpers;

public class NightShiftTemplateTestRepository
{
    private readonly SlaisDbContext _dbContext;

    public NightShiftTemplateTestRepository(SlaisDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<NightShiftTemplateEntity>> SeedCuratedAsync()
    {
        var templates = NightShiftTemplateTestData.CreateCuratedTemplates();
        _dbContext.AddRange(templates);
        await _dbContext.SaveChangesAsync();

        return templates;
    }

    public async Task<NightShiftTemplateEntity> CreateAsync(NightShiftTemplateEntity template)
    {
        _dbContext.Add(template);
        await _dbContext.SaveChangesAsync();

        return template;
    }
}
