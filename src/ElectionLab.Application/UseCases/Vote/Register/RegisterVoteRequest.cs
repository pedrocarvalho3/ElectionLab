using ElectionLab.Domain.Enums;

namespace ElectionLab.Application.UseCases.Vote.Register;

public record RegisterVoteRequest(int? candidateNumber, VoteType type);