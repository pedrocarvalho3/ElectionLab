using Dapper;
using ElectionLab.Domain;
using ElectionLab.Infrastructure.Persistence;
using ElectionLab.Infrastructure.Repositories;
using Npgsql;
using Xunit;

namespace ElectionLab.Infrastructure.IntegrationTests;

public class PersistenceTests
{
    [PostgresFact]
    public async Task Repositories_ShouldPersistAllFields()
    {
        await using var database = await TestDatabase.CreateAsync();
        await new DatabaseMigrator(database.DataSource).MigrateAsync();

        var election = new Election("Votação d'outubro; --");
        election.Open();
        var candidate = new Candidate(election.Id, "Ana d'Ávila", 12);
        var station = new PollingStation(election.Id, "0001");
        station.Open();
        station.Close();

        await new ElectionRepository(database.DataSource).AddAsync(election);
        await new CandidateRepository(database.DataSource).AddAsync(candidate);
        await new PollingStationRepository(database.DataSource).AddAsync(station);

        await using var connection = await database.DataSource.OpenConnectionAsync();
        var storedElection = await connection.QuerySingleAsync("SELECT * FROM elections;");
        Assert.Equal(election.Id, (Guid)storedElection.Id);
        Assert.Equal(election.Name, (string)storedElection.Name);
        Assert.Equal((int)election.Status, (int)storedElection.Status);

        var storedCandidate = await connection.QuerySingleAsync("SELECT * FROM candidates;");
        Assert.Equal(candidate.Id, (Guid)storedCandidate.Id);
        Assert.Equal(election.Id, (Guid)storedCandidate.ElectionId);
        Assert.Equal(candidate.Name, (string)storedCandidate.Name);
        Assert.Equal(candidate.Number, (int)storedCandidate.Number);

        var storedStation = await connection.QuerySingleAsync("SELECT * FROM polling_stations;");
        Assert.Equal(station.Id, (Guid)storedStation.Id);
        Assert.Equal(election.Id, (Guid)storedStation.ElectionId);
        Assert.Equal("0001", (string)storedStation.Code);
        Assert.Equal((int)station.Status, (int)storedStation.Status);
        // PostgreSQL stores timestamps with microsecond precision.
        Assert.True(Math.Abs((station.OpenedAt!.Value - (DateTime)storedStation.OpenedAt).Ticks) < 10);
        Assert.True(Math.Abs((station.ClosedAt!.Value - (DateTime)storedStation.ClosedAt).Ticks) < 10);

        var draft = new PollingStation(election.Id, "0002");
        await new PollingStationRepository(database.DataSource).AddAsync(draft);
        Assert.True(await connection.ExecuteScalarAsync<bool>("""
            SELECT "OpenedAt" IS NULL AND "ClosedAt" IS NULL
            FROM polling_stations WHERE "Id" = @Id;
            """, new { draft.Id }));
    }

    [PostgresFact]
    public async Task Repositories_ShouldRetainDatabaseConstraints()
    {
        await using var database = await TestDatabase.CreateAsync();
        await new DatabaseMigrator(database.DataSource).MigrateAsync();
        var elections = new ElectionRepository(database.DataSource);
        var stations = new PollingStationRepository(database.DataSource);
        var candidates = new CandidateRepository(database.DataSource);
        var election = new Election("First");
        var otherElection = new Election("Second");
        await elections.AddAsync(election);
        await elections.AddAsync(otherElection);

        await stations.AddAsync(new PollingStation(election.Id, "0001"));
        await stations.AddAsync(new PollingStation(otherElection.Id, "0001"));
        await candidates.AddAsync(new Candidate(election.Id, "Ana", 12));
        await candidates.AddAsync(new Candidate(otherElection.Id, "Ana", 12));

        await AssertSqlState("23505", () => stations.AddAsync(new PollingStation(election.Id, "0001")));
        await AssertSqlState("23505", () => candidates.AddAsync(new Candidate(election.Id, "Other", 12)));
        await AssertSqlState("23503", () => stations.AddAsync(new PollingStation(Guid.NewGuid(), "0002")));
        await AssertSqlState("23503", () => candidates.AddAsync(new Candidate(Guid.NewGuid(), "Ana", 13)));
        await AssertSqlState("22001", () => stations.AddAsync(new PollingStation(election.Id, "00001")));
        await AssertSqlState("22001", () => elections.AddAsync(new Election(new string('a', 201))));
        await AssertSqlState("22001", () => candidates.AddAsync(new Candidate(election.Id, new string('a', 201), 13)));

        await using var connection = await database.DataSource.OpenConnectionAsync();
        await AssertSqlState("23503", async () =>
        {
            await connection.ExecuteAsync("DELETE FROM elections WHERE \"Id\" = @Id;", new { election.Id });
        });
    }

    [PostgresFact]
    public async Task Migrator_ShouldAdoptExistingSchemaAndPreserveDataOnRerun()
    {
        await using var database = await TestDatabase.CreateAsync();
        await using var connection = await database.DataSource.OpenConnectionAsync();
        // Simulate the tables left by the former EF migrations, without a Dapper ledger.
        await using var stream = typeof(DatabaseMigrator).Assembly.GetManifestResourceStream(
            "ElectionLab.Infrastructure.Persistence.Migrations.001_InitialSchema.sql")!;
        using var reader = new StreamReader(stream);
        await connection.ExecuteAsync(await reader.ReadToEndAsync());
        await connection.ExecuteAsync("""
            CREATE TABLE "__EFMigrationsHistory" ("MigrationId" text PRIMARY KEY);
            INSERT INTO "__EFMigrationsHistory" VALUES ('legacy');
            """);
        var election = new Election("Existing election");
        await new ElectionRepository(database.DataSource).AddAsync(election);
        await new CandidateRepository(database.DataSource).AddAsync(new Candidate(election.Id, "Existing", 12));
        await new PollingStationRepository(database.DataSource).AddAsync(new PollingStation(election.Id, "0001"));

        var migrator = new DatabaseMigrator(database.DataSource);
        await migrator.MigrateAsync();
        await migrator.MigrateAsync();

        Assert.Equal(election.Name, await connection.QuerySingleAsync<string>("SELECT \"Name\" FROM elections;"));
        Assert.Equal(1, await connection.ExecuteScalarAsync<int>("SELECT count(*) FROM candidates;"));
        Assert.Equal("0001", await connection.QuerySingleAsync<string>("SELECT \"Code\" FROM polling_stations;"));
        Assert.Equal(2, await connection.ExecuteScalarAsync<int>("SELECT count(*) FROM \"__SchemaMigrations\";"));
        Assert.Equal("legacy", await connection.QuerySingleAsync<string>("SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\";"));
    }

    [PostgresFact]
    public async Task Migrator_ShouldSerializeConcurrentRuns()
    {
        await using var database = await TestDatabase.CreateAsync();
        await Task.WhenAll(
            new DatabaseMigrator(database.DataSource).MigrateAsync(),
            new DatabaseMigrator(database.DataSource).MigrateAsync());

        await using var connection = await database.DataSource.OpenConnectionAsync();
        Assert.Equal(2, await connection.ExecuteScalarAsync<int>("SELECT count(*) FROM \"__SchemaMigrations\";"));
    }

    [PostgresFact]
    public async Task Votes_ShouldStoreCandidateBlankAndNullVotesAndRetainDataOnRerun()
    {
        await using var database = await TestDatabase.CreateAsync();
        var migrator = new DatabaseMigrator(database.DataSource);
        await migrator.MigrateAsync();
        var election = new Election("Vote test");
        var candidate = new Candidate(election.Id, "Ana", 12);
        await new ElectionRepository(database.DataSource).AddAsync(election);
        await new CandidateRepository(database.DataSource).AddAsync(candidate);

        await using var connection = await database.DataSource.OpenConnectionAsync();
        const string insert = """
            INSERT INTO votes ("Id", "ElectionId", "CandidateId", "Type", "CreatedAt")
            VALUES (@Id, @ElectionId, @CandidateId, @Type, @CreatedAt);
            """;
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        await connection.ExecuteAsync(insert, new
        {
            Id = id, ElectionId = election.Id, CandidateId = candidate.Id, Type = 0, CreatedAt = createdAt
        });
        foreach (var type in new[] { 1, 2 })
            await connection.ExecuteAsync(insert, new
            {
                Id = Guid.NewGuid(), ElectionId = election.Id, CandidateId = (Guid?)null,
                Type = type, CreatedAt = createdAt
            });

        await migrator.MigrateAsync();

        var stored = await connection.QuerySingleAsync("SELECT * FROM votes WHERE \"Id\" = @Id;", new { Id = id });
        Assert.Equal(election.Id, (Guid)stored.ElectionId);
        Assert.Equal(candidate.Id, (Guid)stored.CandidateId);
        Assert.Equal(0, (int)stored.Type);
        Assert.True(Math.Abs((createdAt - (DateTime)stored.CreatedAt).Ticks) < 10);
        Assert.Equal(3, await connection.ExecuteScalarAsync<int>("SELECT count(*) FROM votes;"));
        Assert.Equal(2, await connection.ExecuteScalarAsync<int>(
            "SELECT count(*) FROM votes WHERE \"CandidateId\" IS NULL;"));

        await AssertSqlState("23503", async () =>
        {
            await connection.ExecuteAsync("DELETE FROM candidates WHERE \"Id\" = @Id;", new { candidate.Id });
        });
    }

    [PostgresFact]
    public async Task Votes_ShouldRejectInvalidTypesCandidatesAndElections()
    {
        await using var database = await TestDatabase.CreateAsync();
        await new DatabaseMigrator(database.DataSource).MigrateAsync();
        var election = new Election("First");
        var otherElection = new Election("Second");
        await new ElectionRepository(database.DataSource).AddAsync(election);
        await new ElectionRepository(database.DataSource).AddAsync(otherElection);
        var candidate = new Candidate(election.Id, "Ana", 12);
        await new CandidateRepository(database.DataSource).AddAsync(candidate);
        await using var connection = await database.DataSource.OpenConnectionAsync();

        async Task InsertVote(Guid electionId, Guid? candidateId, int type)
        {
            await connection.ExecuteAsync("""
                INSERT INTO votes ("Id", "ElectionId", "CandidateId", "Type", "CreatedAt")
                VALUES (@Id, @ElectionId, @CandidateId, @Type, @CreatedAt);
                """, new
            {
                Id = Guid.NewGuid(), ElectionId = electionId, CandidateId = candidateId,
                Type = type, CreatedAt = DateTime.UtcNow
            });
        }

        await AssertSqlState("23514", () => InsertVote(election.Id, null, 0));
        await AssertSqlState("23514", () => InsertVote(election.Id, candidate.Id, 1));
        await AssertSqlState("23514", () => InsertVote(election.Id, candidate.Id, 2));
        await AssertSqlState("23514", () => InsertVote(election.Id, null, 99));
        await AssertSqlState("23503", () => InsertVote(election.Id, Guid.NewGuid(), 0));
        await AssertSqlState("23503", () => InsertVote(otherElection.Id, candidate.Id, 0));
        await AssertSqlState("23503", () => InsertVote(Guid.NewGuid(), null, 1));
        Assert.Equal(0, await connection.ExecuteScalarAsync<int>("SELECT count(*) FROM votes;"));
    }

    private static async Task AssertSqlState(string expected, Func<Task> action)
    {
        var exception = await Assert.ThrowsAsync<PostgresException>(action);
        Assert.Equal(expected, exception.SqlState);
    }

    private sealed class TestDatabase(NpgsqlDataSource admin, NpgsqlDataSource dataSource, string schema)
        : IAsyncDisposable
    {
        public NpgsqlDataSource DataSource => dataSource;

        public static async Task<TestDatabase> CreateAsync()
        {
            var connectionString = Environment.GetEnvironmentVariable("ELECTIONLAB_TEST_DATABASE")!;
            var admin = NpgsqlDataSource.Create(connectionString);
            var schema = "dapper_test_" + Guid.NewGuid().ToString("N");
            await using var connection = await admin.OpenConnectionAsync();
            await connection.ExecuteAsync($"CREATE SCHEMA {schema};");
            var settings = new NpgsqlConnectionStringBuilder(connectionString) { SearchPath = schema };
            return new TestDatabase(admin, NpgsqlDataSource.Create(settings.ConnectionString), schema);
        }

        public async ValueTask DisposeAsync()
        {
            await dataSource.DisposeAsync();
            try
            {
                await using var connection = await admin.OpenConnectionAsync();
                await connection.ExecuteAsync($"DROP SCHEMA {schema} CASCADE;");
            }
            finally
            {
                await admin.DisposeAsync();
            }
        }
    }
}

public sealed class PostgresFactAttribute : FactAttribute
{
    public PostgresFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ELECTIONLAB_TEST_DATABASE")))
            Skip = "Set ELECTIONLAB_TEST_DATABASE to run PostgreSQL integration tests.";
    }
}
