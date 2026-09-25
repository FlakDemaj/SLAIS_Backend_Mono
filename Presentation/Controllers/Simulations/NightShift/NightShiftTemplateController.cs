using Application.Common.DTOs.Base;
using Application.Common.DTOs.Simulations.NightShift;
using Application.Simulations.NightShift.Templates.Commands.CreateNightShiftTemplate;
using Application.Simulations.NightShift.Templates.Commands.CreateNightShiftTemplateFromSession;
using Application.Simulations.NightShift.Templates.Commands.DeleteNightShiftTemplate;
using Application.Simulations.NightShift.Templates.Commands.SetNightShiftTemplateState;
using Application.Simulations.NightShift.Templates.Commands.SuggestNightShiftTemplateTranslation;
using Application.Simulations.NightShift.Templates.Commands.UpdateNightShiftTemplate;
using Application.Simulations.NightShift.Templates.Querys.GetNightShiftTemplate;
using Application.Simulations.NightShift.Templates.Querys.GetNightShiftTemplates;
using Application.Utils.Interfaces.Mediator;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Presentation.Utils;

namespace Presentation.Controllers.Simulations.NightShift;

[Authorize(Roles = $"{Role.Admin},{Role.SuperAdmin}")]
[Route("rest/[controller]")]
public class NightShiftTemplateController : BaseRestController
{
    public NightShiftTemplateController(
        IMediator mediator)
        : base(mediator)
    {
    }

    [HttpGet]
    public async Task<ActionResult<List<GetNightShiftTemplateResponseDto>>> GetTemplatesAsync(
        CancellationToken cancellationToken)
    {
        return await _mediator.SendAsync(
            new GetNightShiftTemplatesQuery(),
            Authentication,
            cancellationToken);
    }

    [HttpGet("{templateGuid:guid}")]
    public async Task<ActionResult<GetNightShiftTemplateResponseDto>> GetTemplateAsync(
        [FromRoute] Guid templateGuid,
        CancellationToken cancellationToken)
    {
        return await _mediator.SendAsync(
            new GetNightShiftTemplateQuery { TemplateGuid = templateGuid },
            Authentication,
            cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<CreateObjectResponseDto>> CreateAsync(
        [FromBody] CreateNightShiftTemplateCommand request,
        CancellationToken cancellationToken)
    {
        return await _mediator.SendAsync(request, Authentication, cancellationToken);
    }

    [HttpPut("{templateGuid:guid}")]
    public async Task<ActionResult<GetNightShiftTemplateResponseDto>> UpdateAsync(
        [FromRoute] Guid templateGuid,
        [FromBody] UpdateNightShiftTemplateCommand body,
        CancellationToken cancellationToken)
    {
        var request = new UpdateNightShiftTemplateCommand
        {
            TemplateGuid = templateGuid,
            ExpectedVersion = body.ExpectedVersion,
            Key = body.Key,
            TensionStart = body.TensionStart,
            SortOrder = body.SortOrder,
            German = body.German,
            English = body.English
        };

        return await _mediator.SendAsync(request, Authentication, cancellationToken);
    }

    [HttpPost("{templateGuid:guid}/state")]
    public async Task<ActionResult<GetNightShiftTemplateResponseDto>> SetStateAsync(
        [FromRoute] Guid templateGuid,
        [FromBody] SetNightShiftTemplateStateCommand body,
        CancellationToken cancellationToken)
    {
        var request = new SetNightShiftTemplateStateCommand
        {
            TemplateGuid = templateGuid,
            ExpectedVersion = body.ExpectedVersion,
            State = body.State
        };

        return await _mediator.SendAsync(request, Authentication, cancellationToken);
    }

    [HttpDelete("{templateGuid:guid}")]
    public async Task<ActionResult<CreateObjectResponseDto>> DeleteAsync(
        [FromRoute] Guid templateGuid,
        CancellationToken cancellationToken)
    {
        return await _mediator.SendAsync(
            new DeleteNightShiftTemplateCommand { TemplateGuid = templateGuid },
            Authentication,
            cancellationToken);
    }

    [HttpPost("translate")]
    public async Task<ActionResult<NightShiftTemplateTextDto>> TranslateAsync(
        [FromBody] SuggestNightShiftTemplateTranslationCommand request,
        CancellationToken cancellationToken)
    {
        return await _mediator.SendAsync(request, Authentication, cancellationToken);
    }

    [HttpPost("from-session/{sessionGuid:guid}")]
    public async Task<ActionResult<CreateObjectResponseDto>> CreateFromSessionAsync(
        [FromRoute] Guid sessionGuid,
        [FromBody] FromSessionBody body,
        CancellationToken cancellationToken)
    {
        return await _mediator.SendAsync(
            new CreateNightShiftTemplateFromSessionCommand
            {
                SessionGuid = sessionGuid,
                Key = body.Key
            },
            Authentication,
            cancellationToken);
    }
}

public class FromSessionBody
{
    public required string Key { get; init; }
}
