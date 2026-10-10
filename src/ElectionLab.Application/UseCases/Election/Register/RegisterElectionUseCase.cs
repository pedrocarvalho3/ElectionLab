using ElectionLab.Application.Abstractions;
using ElectionEntity = ElectionLab.Domain.Election;

namespace ElectionLab.Application.UseCases.Election.Register;

public class RegisterElectionUseCase(IElectionRepository repository) 
    : IRegisterElectionUseCase
{
    public async Task Execute(RegisterElectionRequest request)
    {
        var election = new ElectionEntity(request.Name);
        await repository.AddAsync(election);
    }
}