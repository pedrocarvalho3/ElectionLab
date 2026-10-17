using ElectionLab.Application.UseCases.Candidate.Register;
using ElectionLab.Application.UseCases.Election.Register;
using ElectionLab.Application.UseCases.PollingStation.Register;
using Microsoft.AspNetCore.Mvc;

namespace ElectionLab.Api.Controllers;

[Route("elections")]
[ApiController]
public class ElectionController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Register(
        [FromBody] RegisterElectionRequest request,
        [FromServices] IRegisterElectionUseCase useCase)
    {
        await useCase.Execute(request);
        return Created();
    }
    
    [HttpPost("{electionId}/candidates")]
    public async Task<IActionResult> Register(
        [FromRoute] Guid electionId,
        [FromBody] RegisterCandidateRequest request,
        [FromServices] IRegisterCandidateUseCase useCase)
    {
        await useCase.Execute(electionId, request);
        return Created();
    }

    [HttpPost("{electionId}/stations")]
    public async Task<IActionResult> Register(
        [FromRoute] Guid electionId,
        [FromBody] string code,
        [FromServices] IRegisterPollingStationUseCase useCase)
    {
        await useCase.Execute(electionId, code);
        return Created();
    }
}
