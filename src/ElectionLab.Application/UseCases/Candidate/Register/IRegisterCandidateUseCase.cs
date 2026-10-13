namespace ElectionLab.Application.UseCases.Candidate.Register;

public interface IRegisterCandidateUseCase
{
    Task Execute(Guid electionId, RegisterCandidateRequest request);
}