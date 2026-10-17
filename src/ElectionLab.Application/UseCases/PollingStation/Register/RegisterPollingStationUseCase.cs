using ElectionLab.Application.Abstractions;
using PollingStationEntity = ElectionLab.Domain.PollingStation;

namespace ElectionLab.Application.UseCases.PollingStation.Register;

public class RegisterPollingStationUseCase(IPollingStationRepository repository)
    : IRegisterPollingStationUseCase
{
    public async Task Execute(Guid electionId, string code)
    {
        var pollingStation = new PollingStationEntity(electionId, code);
        await repository.AddAsync(pollingStation);
    }
}
