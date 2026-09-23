using Application.Common.Authentication;
using Application.Common.Base;
using Application.Common.DTOs.Simulations.NightShift;
using Application.Utils.Logger;
using Application.Utils.Mediator.Interfaces;

using AutoMapper;

using Domain.Common.Exceptions;

namespace Application.Simulations.NightShift.Querys.GetNightShiftCases;

public class GetNightShiftCasesQueryHandler : BaseHandler<GetNightShiftCasesQuery>,
    IRequestHandler<GetNightShiftCasesQuery, List<NightShiftCaseResponseDto>>
{
    public GetNightShiftCasesQueryHandler(
        IMapper mapper,
        ISlaisLogger<GetNightShiftCasesQuery> logger)
        : base(mapper, logger)
    {
    }

    public Task<List<NightShiftCaseResponseDto>> HandleAsync(
        GetNightShiftCasesQuery request,
        IAuthentication? authentication = null,
        CancellationToken cancellationToken = default)
    {
        if (authentication!.UserRole == Domain.Common.Enums.Roles.Server)
        {
            throw new SlaisException(NightShiftErrorCodes.Forbidden);
        }

        var language = NightShiftLanguage.Parse(request.Language);
        var cases = Cases.CuratedCases.For(language)
            .Select(patientCase => new NightShiftCaseResponseDto
            {
                Key = patientCase.Key,
                Name = patientCase.Name,
                Situation = patientCase.Situation,
                Lernziel = patientCase.LearningGoal
            })
            .ToList();
        cases.Add(new NightShiftCaseResponseDto
        {
            Key = Cases.CuratedCases.RandomKey,
            Name = NightShiftLanguage.RandomCaseName(language),
            Situation = NightShiftLanguage.RandomCaseSituation(language),
            Lernziel = string.Empty
        });
        return Task.FromResult(cases);
    }
}
