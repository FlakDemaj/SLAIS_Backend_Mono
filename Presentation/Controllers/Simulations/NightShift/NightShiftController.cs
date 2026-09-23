using System.Text.Json;

using Application.Common.DTOs.Simulations.NightShift;
using Application.Simulations.NightShift.Commands.FinishNightShiftSession;
using Application.Simulations.NightShift.Commands.SendNightShiftMessage;
using Application.Simulations.NightShift.Commands.StartNightShiftSession;
using Application.Simulations.NightShift.Querys.GetNightShiftCases;
using Application.Simulations.NightShift.Querys.GetNightShiftSession;
using Application.Simulations.NightShift.Querys.GetNightShiftSessions;
using Application.Utils.Interfaces.Mediator;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using Presentation.Utils;

namespace Presentation.Controllers.Simulations.NightShift;

[Authorize(Roles = $"{Role.Student},{Role.Teacher},{Role.Admin},{Role.SuperAdmin}")]
[Route("rest/[controller]")]
public class NightShiftController : BaseRestController
{
    public NightShiftController(
        IMediator mediator)
        : base(mediator)
    {
    }

    [HttpGet("cases")]
    public async Task<ActionResult<List<NightShiftCaseResponseDto>>> GetCasesAsync(
        [FromQuery] string? lang,
        CancellationToken cancellationToken)
    {
        return await _mediator.SendAsync(
            new GetNightShiftCasesQuery { Language = lang },
            Authentication,
            cancellationToken);
    }

    [HttpPost("start")]
    public async Task<ActionResult<StartNightShiftSessionResponseDto>> StartAsync(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] StartNightShiftSessionCommand? body,
        CancellationToken cancellationToken)
    {
        return await _mediator.SendAsync(
            body ?? new StartNightShiftSessionCommand(),
            Authentication,
            cancellationToken);
    }

    [HttpPost("chat/{sessionId:guid}")]
    public async Task<ActionResult<NightShiftMessageResponseDto>> ChatAsync(
        [FromRoute] Guid sessionId,
        [FromBody] JsonElement body,
        CancellationToken cancellationToken)
    {
        return await _mediator.SendAsync(
            new SendNightShiftMessageCommand
            {
                SessionGuid = sessionId,
                Text = GetText(body),
                Kind = GetKind(body)
            },
            Authentication,
            cancellationToken);
    }

    [HttpPost("finish/{sessionId:guid}")]
    public async Task<ActionResult<NightShiftFeedbackResponseDto>> FinishAsync(
        [FromRoute] Guid sessionId,
        CancellationToken cancellationToken)
    {
        return await _mediator.SendAsync(
            new FinishNightShiftSessionCommand { SessionGuid = sessionId },
            Authentication,
            cancellationToken);
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<List<NightShiftSessionSummaryResponseDto>>> GetSessionsAsync(
        CancellationToken cancellationToken)
    {
        return await _mediator.SendAsync(
            new GetNightShiftSessionsQuery(),
            Authentication,
            cancellationToken);
    }

    [HttpGet("sessions/{sessionId:guid}")]
    public async Task<ActionResult<NightShiftSessionDetailResponseDto>> GetSessionAsync(
        [FromRoute] Guid sessionId,
        CancellationToken cancellationToken)
    {
        return await _mediator.SendAsync(
            new GetNightShiftSessionQuery { SessionGuid = sessionId },
            Authentication,
            cancellationToken);
    }

    private static string GetText(JsonElement body)
    {
        if (body.ValueKind == JsonValueKind.String)
        {
            return body.GetString() ?? string.Empty;
        }

        if (body.ValueKind == JsonValueKind.Object
            && body.TryGetProperty("text", out var text))
        {
            return text.GetString() ?? string.Empty;
        }

        return string.Empty;
    }

    private static string GetKind(JsonElement body)
    {
        if (body.ValueKind == JsonValueKind.Object
            && body.TryGetProperty("kind", out var kind))
        {
            return kind.GetString() ?? "say";
        }

        return "say";
    }
}
