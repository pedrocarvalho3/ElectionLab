using ElectionLab.Application.Abstractions;
using CandidateEntity = ElectionLab.Domain.Candidate;

namespace ElectionLab.Application.UseCases.Candidate.Register;

public class RegisterCandidateUseCase(ICandidateRepository repository) 
    : IRegisterCandidateUseCase
{
    public async Task Execute(Guid electionId, RegisterCandidateRequest request)
    {
        var candidate = new CandidateEntity(electionId, request.Name, request.Number);
        await repository.AddAsync(candidate);
    }
}