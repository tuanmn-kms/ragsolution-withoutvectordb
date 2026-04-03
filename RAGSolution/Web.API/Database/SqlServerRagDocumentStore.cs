using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;

namespace Web.API.Database;

public sealed class SqlServerRagDocumentStore(IConfiguration configuration) : IRagDocumentStore
{
    private const string ConnectionStringName = "RagSqlServer";

    private readonly string _connectionString = configuration.GetConnectionString(ConnectionStringName)
        ?? throw new InvalidOperationException($"Missing connection string '{ConnectionStringName}'.");

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseExistsAsync(cancellationToken);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            IF OBJECT_ID('dbo.RagDocuments', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.RagDocuments
                (
                    Id NVARCHAR(200) NOT NULL PRIMARY KEY,
                    Title NVARCHAR(300) NOT NULL,
                    Content NVARCHAR(MAX) NOT NULL,
                    MetadataJson NVARCHAR(MAX) NULL,
                    CreatedUtc DATETIME2 NOT NULL,
                    UpdatedUtc DATETIME2 NOT NULL
                )
            END
            """;

        await using var command = new SqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task EnsureDatabaseExistsAsync(CancellationToken cancellationToken)
    {
        var builder = new SqlConnectionStringBuilder(_connectionString);

        if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
        {
            throw new InvalidOperationException("Connection string must include a database name.");
        }

        var databaseName = builder.InitialCatalog;

        var masterBuilder = new SqlConnectionStringBuilder(_connectionString)
        {
            InitialCatalog = "master"
        };

        await using var masterConnection = new SqlConnection(masterBuilder.ConnectionString);
        await masterConnection.OpenAsync(cancellationToken);

        const string sql = """
            DECLARE @dbName SYSNAME = @DatabaseName;

            IF DB_ID(@dbName) IS NULL
            BEGIN
                DECLARE @createSql NVARCHAR(MAX) = N'CREATE DATABASE ' + QUOTENAME(@dbName);
                EXEC(@createSql);
            END
            """;

        await using var command = new SqlCommand(sql, masterConnection);
        command.Parameters.Add("@DatabaseName", SqlDbType.NVarChar, 128).Value = databaseName;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task UpsertDocumentAsync(StoredRagDocument document, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            MERGE dbo.RagDocuments AS target
            USING (SELECT @Id AS Id) AS source
            ON target.Id = source.Id
            WHEN MATCHED THEN
                UPDATE SET
                    Title = @Title,
                    Content = @Content,
                    MetadataJson = @MetadataJson,
                    UpdatedUtc = SYSUTCDATETIME()
            WHEN NOT MATCHED THEN
                INSERT (Id, Title, Content, MetadataJson, CreatedUtc, UpdatedUtc)
                VALUES (@Id, @Title, @Content, @MetadataJson, SYSUTCDATETIME(), SYSUTCDATETIME());
            """;

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add("@Id", SqlDbType.NVarChar, 200).Value = document.Id;
        command.Parameters.Add("@Title", SqlDbType.NVarChar, 300).Value = document.Title;
        command.Parameters.Add("@Content", SqlDbType.NVarChar).Value = document.Content;
        command.Parameters.Add("@MetadataJson", SqlDbType.NVarChar).Value = SerializeMetadata(document.Metadata);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StoredRagDocument>> GetAllDocumentsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT Id, Title, Content, MetadataJson
            FROM dbo.RagDocuments
            ORDER BY UpdatedUtc DESC;
            """;

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var documents = new List<StoredRagDocument>();

        while (await reader.ReadAsync(cancellationToken))
        {
            var id = reader.GetString(0);
            var title = reader.GetString(1);
            var content = reader.GetString(2);
            var metadataJson = reader.IsDBNull(3) ? null : reader.GetString(3);

            documents.Add(new StoredRagDocument(id, title, content, DeserializeMetadata(metadataJson)));
        }

        return documents;
    }

    public async Task<RagDatabaseHealth> CheckHealthAsync(CancellationToken cancellationToken = default)
    {
        var builder = new SqlConnectionStringBuilder(_connectionString);
        var server = builder.DataSource;
        var database = builder.InitialCatalog;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = new SqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync(cancellationToken);

            return new RagDatabaseHealth(true, server, database, "SQL Server connection is healthy.");
        }
        catch (Exception ex)
        {
            return new RagDatabaseHealth(false, server, database, ex.Message);
        }
    }

    private static string SerializeMetadata(IReadOnlyDictionary<string, string> metadata)
    {
        if (metadata.Count == 0)
        {
            return "{}";
        }

        return JsonSerializer.Serialize(metadata);
    }

    private static IReadOnlyDictionary<string, string> DeserializeMetadata(string? metadataJson)
    {
        if (string.IsNullOrWhiteSpace(metadataJson))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(metadataJson);
        return metadata is null
            ? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, string>(metadata, StringComparer.OrdinalIgnoreCase);
    }
}
