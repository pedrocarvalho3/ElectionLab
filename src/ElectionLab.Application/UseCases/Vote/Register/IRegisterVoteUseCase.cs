using ElectionLab.Domain.Enums;

namespace ElectionLab.Application.UseCases.Vote.Register;

public interface IRegisterVoteUseCase
{
    Task Execute(Guid pollingStationId, int? candidateNumber, VoteType type);
}