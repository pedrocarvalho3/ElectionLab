using Dapper;
using ElectionLab.Application.Abstractions;
using ElectionLab.Domain;
using Npgsql;

namespace ElectionLab.Infrastructure.Repositories;

public class CandidateRepository(NpgsqlDataSource dataSource) : ICandidateRepository
{
    public async Task AddAsync(Candidate candidate)
    {
        const string sql = """
            INSERT INTO candidates ("Id", "ElectionId", "Name", "Number")
            VALUES (@Id, @ElectionId, @Name, @Number);
            """;

        await using var connection = await dataSource.OpenConnectionAsync();
        await connection.ExecuteAsync(sql, new
        {
            candidate.Id,
            candidate.ElectionId,
            candidate.Name,
            candidate.Number
        });
    }
    
    public async Task<Candidate?> GetByNumberAsync(Guid electionId, int number)
    {
        const string sql = """
                           SELECT *
                           FROM candidates
                           WHERE "ElectionId" = @ElectionId AND Number = @Number;
                           """;

        await using var connection = await dataSource.OpenConnectionAsync();
        var candidate = await connection.QuerySingleOrDefaultAsync<Candidate>(sql, new
        {
            ElectionId = electionId,
            Number = number
        });
        
        return candidate;
    }
}
