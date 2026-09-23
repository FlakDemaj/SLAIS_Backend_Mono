using Application.Common.DTOs.Simulations.NightShift;

using AutoMapper;

using Domain.Common.Enums;
using Domain.Simulations.NightShift;

namespace Application.Common.Mappers.Simulations;

public sealed class NightShiftMapperProfile : Profile
{
    public NightShiftMapperProfile()
    {
        CreateMap<NightShiftSessionEntity, NightShiftSessionSummaryResponseDto>()
            .ForMember(dto => dto.SessionId, options => options.MapFrom(entity => entity.Guid))
            .ForMember(dto => dto.TemplateId, options => options.MapFrom(entity => entity.TemplateGuid))
            .ForMember(dto => dto.HasFeedback, options => options.MapFrom(entity => entity.Feedback != null))
            .ForMember(dto => dto.Turns, options => options.MapFrom(entity => entity.Messages.Count(message =>
                message.Role == NightShiftMessageRole.Student
                || message.Role == NightShiftMessageRole.Action)));

        CreateMap<NightShiftMessageEntity, NightShiftSessionMessageDto>()
            .ForMember(dto => dto.Role, options => options.MapFrom(entity => ToRole(entity.Role)))
            .ForMember(dto => dto.Text, options => options.MapFrom(entity => entity.Content))
            .ForMember(dto => dto.At, options => options.MapFrom(entity => entity.CreatedAt));

        CreateMap<NightShiftFeedbackEntity, NightShiftFeedbackResponseDto>()
            .ForMember(dto => dto.Feedback, options => options.MapFrom(entity => entity.Summary))
            .ForMember(dto => dto.Summary, options => options.MapFrom(entity => entity.Summary))
            .ForMember(dto => dto.Scores, options => options.MapFrom(entity => new NightShiftDimensionScoresDto
            {
                Fachlich = entity.ScoreProfessional,
                Sympathie = entity.ScoreRapport,
                Empathie = entity.ScoreEmpathy,
                Zuhoeren = entity.ScoreListening,
                Klarheit = entity.ScoreClarity
            }))
            .ForMember(dto => dto.PerDimension, options => options.MapFrom(entity => new NightShiftDimensionTextsDto
            {
                Fachlich = entity.TextProfessional,
                Sympathie = entity.TextRapport,
                Empathie = entity.TextEmpathy,
                Zuhoeren = entity.TextListening,
                Klarheit = entity.TextClarity
            }))
            .ForMember(dto => dto.Labels, options => options.Ignore());
    }

    private static string ToRole(NightShiftMessageRole role)
    {
        switch (role)
        {
            case NightShiftMessageRole.Patient:
                return "patient";
            case NightShiftMessageRole.Student:
                return "student";
            case NightShiftMessageRole.Action:
                return "action";
            default:
                return string.Empty;
        }
    }
}
