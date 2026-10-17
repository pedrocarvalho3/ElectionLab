namespace ElectionLab.Application.UseCases.PollingStation.Register;

public interface IRegisterPollingStationUseCase
{
    Task Execute(Guid electionId, string code);
}
