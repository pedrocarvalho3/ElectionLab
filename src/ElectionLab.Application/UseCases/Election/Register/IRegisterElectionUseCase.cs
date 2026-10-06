namespace ElectionLab.Application.UseCases.Election.Register;

public interface IRegisterElectionUseCase
{
    Task Execute(RegisterElectionRequest request);
}