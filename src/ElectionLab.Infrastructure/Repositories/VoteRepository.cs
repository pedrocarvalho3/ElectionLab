using Dapper;
using ElectionLab.Application.Abstractions;
using ElectionLab.Domain;
using Npgsql;

namespace ElectionLab.Infrastructure.Repositories;

public class VoteRepository(NpgsqlDataSource dataSource) : IVoteRepository
{
    public async Task AddAsync(Vote vote)
    {
        const string sql = """
                           INSERT INTO votes ("Id", "PollingStationId", "ElectionId", "CandidateId", "Type", "CreatedAt")
                           VALUES (@Id, @PollingStationId, @ElectionId, @CandidateId, @Type, @CreatedAt);
                           """;

        await using var connection = await dataSource.OpenConnectionAsync();
        await connection.ExecuteAsync(sql, new
        {
            vote.Id,
            vote.PollingStationId,
            vote.ElectionId,
            vote.CandidateId,
            Type = (int)vote.Type,
            vote.CreatedAt
        });
    }
}