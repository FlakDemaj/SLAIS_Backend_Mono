using Application.Common.Authentication;
using Application.Common.Base;
using Application.Common.DTOs.Simulations.NightShift;
using Application.Common.Interfaces.Repositorys;
using Application.Simulations.NightShift.Cases;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using AutoMapper;

using Domain.Common.Enums;
using Domain.Common.Exceptions;

namespace Application.Simulations.NightShift.Querys.GetNightShiftCases;

public class GetNightShiftCasesQueryHandler : BaseHandler<GetNightShiftCasesQuery>,
    IRequestHandler<GetNightShiftCasesQuery, List<NightShiftCaseResponseDto>>
{
    private readonly INightShiftTemplateRepository _nightShiftTemplateRepository;

    public GetNightShiftCasesQueryHandler(
        INightShiftTemplateRepository nightShiftTemplateRepository,
        IMapper mapper,
        ISlaisLogger<GetNightShiftCasesQuery> logger)
        : base(mapper, logger)
    {
        _nightShiftTemplateRepository = nightShiftTemplateRepository;
    }

    public async Task<List<NightShiftCaseResponseDto>> HandleAsync(
        GetNightShiftCasesQuery request,
        IAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        if (authentication!.UserRole == Roles.Server)
        {
            throw new SlaisException(NightShiftErrorCodes.Forbidden);
        }

        var language = NightShiftLanguage.Parse(request.Language);
        var templates = await _nightShiftTemplateRepository.GetActiveAsync();
        var cases = templates
            .Select(template => PatientCaseFactory.FromTemplate(template, language))
            .Select(resolution => new NightShiftCaseResponseDto
            {
                Key = resolution.Case.Key,
                Name = resolution.Case.Name,
                Situation = resolution.Case.Situation,
                Lernziel = resolution.Case.LearningGoal,
                LanguageFallback = resolution.IsLanguageFallback
            })
            .ToList();
        cases.Add(new NightShiftCaseResponseDto
        {
            Key = "random",
            Name = NightShiftLanguage.RandomCaseName(language),
            Situation = NightShiftLanguage.RandomCaseSituation(language),
            Lernziel = string.Empty
        });
        return cases;
    }
}
