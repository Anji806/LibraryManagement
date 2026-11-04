using Dapper;

namespace LibraryManagement.Data;

/// <summary>
/// Adds nullable book preview columns through Dapper.
/// </summary>
public static class BookPreviewContentInitializer
{
    private const string SchemaSql = @"
IF COL_LENGTH(N'dbo.Books', N'PreviewIntroduction') IS NULL
    ALTER TABLE dbo.Books ADD PreviewIntroduction nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.Books', N'PreviewTheme') IS NULL
    ALTER TABLE dbo.Books ADD PreviewTheme nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.Books', N'PreviewAbstract') IS NULL
    ALTER TABLE dbo.Books ADD PreviewAbstract nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.Books', N'PreviewMotto') IS NULL
    ALTER TABLE dbo.Books ADD PreviewMotto nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.Books', N'PreviewWhyRead') IS NULL
    ALTER TABLE dbo.Books ADD PreviewWhyRead nvarchar(max) NULL;
IF COL_LENGTH(N'dbo.Books', N'CoverUrl') IS NULL
    ALTER TABLE dbo.Books ADD CoverUrl nvarchar(max) NULL;
";

    public static async Task InitializeAsync(IDbConnectionFactory connectionFactory)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        if (connection.GetType().FullName?.Contains("SqliteConnection", StringComparison.Ordinal) == true)
        {
            var columns = await connection.QueryAsync<dynamic>("PRAGMA table_info('Books')", transaction: transaction);
            var existingColumns = columns.Select(column => (string)column.name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var previewColumns = new[]
            {
                "PreviewIntroduction", "PreviewTheme", "PreviewAbstract", "PreviewMotto",
                "PreviewWhyRead", "CoverUrl"
            };

            foreach (var column in previewColumns.Where(column => !existingColumns.Contains(column)))
                await connection.ExecuteAsync($"ALTER TABLE Books ADD COLUMN {column} TEXT NULL", transaction: transaction);

        }
        else
        {
            await connection.ExecuteAsync(new CommandDefinition(SchemaSql, transaction: transaction));
        }

        await transaction.CommitAsync();
    }
}
