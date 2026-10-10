using ElectionLab.Application.UseCases.Election.Register;
using Microsoft.AspNetCore.Mvc;

namespace ElectionLab.Api.Controllers;

[Route("[controller]")]
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
}