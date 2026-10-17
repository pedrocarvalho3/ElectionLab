using Dapper;
using Npgsql;

namespace ElectionLab.Infrastructure.Persistence;

public class DatabaseMigrator(NpgsqlDataSource dataSource)
{
    public async Task MigrateAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        // Serialize migration runs before creating or reading the migration history.
        await connection.ExecuteAsync(new CommandDefinition(
            "SELECT pg_advisory_xact_lock(1746919381);",
            transaction: transaction, cancellationToken: cancellationToken));

        await connection.ExecuteAsync(new CommandDefinition("""
            CREATE TABLE IF NOT EXISTS "__SchemaMigrations" (
                "Name" text PRIMARY KEY,
                "AppliedAt" timestamp with time zone NOT NULL DEFAULT now()
            );
            """, transaction: transaction, cancellationToken: cancellationToken));

        var applied = (await connection.QueryAsync<string>(new CommandDefinition(
            "SELECT \"Name\" FROM \"__SchemaMigrations\";",
            transaction: transaction, cancellationToken: cancellationToken))).ToHashSet();

        var assembly = typeof(DatabaseMigrator).Assembly;
        const string prefix = "ElectionLab.Infrastructure.Persistence.Migrations.";
        var scripts = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(prefix, StringComparison.Ordinal)
                && name.EndsWith(".sql", StringComparison.Ordinal))
            .OrderBy(name => name, StringComparer.Ordinal);

        foreach (var resourceName in scripts)
        {
            var name = resourceName[prefix.Length..];
            if (applied.Contains(name))
                continue;

            await using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException($"Migration resource '{resourceName}' was not found.");
            using var reader = new StreamReader(stream);
            var sql = await reader.ReadToEndAsync(cancellationToken);

            await connection.ExecuteAsync(new CommandDefinition(sql,
                transaction: transaction, cancellationToken: cancellationToken));
            await connection.ExecuteAsync(new CommandDefinition(
                "INSERT INTO \"__SchemaMigrations\" (\"Name\") VALUES (@Name);",
                new { Name = name }, transaction, cancellationToken: cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
