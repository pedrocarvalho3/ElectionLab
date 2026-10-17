# ElectionLab

.NET 10 API using Dapper and Npgsql with PostgreSQL. Repositories execute
parameterized SQL and open short-lived connections from a shared connection pool.
Each current repository operation is a single atomic INSERT.

## Run locally

```sh
docker compose up -d
dotnet run --project src/ElectionLab.Api -- --migrate
dotnet run --project src/ElectionLab.Api
```

The API uses `ConnectionStrings:Database` from its configuration. Override it with
the `ConnectionStrings__Database` environment variable when needed. The migration
command applies embedded SQL scripts, records their filenames in
`__SchemaMigrations`, and exits without starting the web server. Migrations run in
a transaction and serialize concurrent migration commands using a database lock.
Normal API startup does not change the schema.

## Existing databases

The SQL baseline matches the former EF schema, including quoted column names,
integer enum values, foreign keys, unique indexes, and the four-character station
code limit. It can run against a database created by the former EF migrations or
an empty database. Existing tables and data are retained; the old
`__EFMigrationsHistory` table can remain unused.

The baseline assumes existing tables match the former EF schema; it does not
repair manually changed column definitions. No database recreation or data export
is required. Dapper does not validate field lengths before sending SQL, so the
database still rejects station codes longer than four characters.

For future schema changes, add ordered SQL files such as `002_AddExample.sql` in
`src/ElectionLab.Infrastructure/Persistence/Migrations`, rebuild, and run the
migration command. Applied scripts are immutable; use a new script for changes.
Scripts run inside the migrator's transaction and must not include BEGIN/COMMIT.
EF configuration classes, DbContext, packages, and C# migrations have been removed.

## Tests

```sh
dotnet test ElectionLab.sln
```

PostgreSQL integration tests run when `ELECTIONLAB_TEST_DATABASE` is set to an
Npgsql connection string. They create and drop randomly named schemas, requiring
schema creation permission, and leave application tables alone. Without that
variable these tests are reported as skipped.

For the local Docker database:

```sh
ELECTIONLAB_TEST_DATABASE='Host=localhost;Port=5432;Database=electionlab;Username=postgres;Password=postgres' dotnet test ElectionLab.sln
```
