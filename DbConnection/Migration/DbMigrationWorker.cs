// Copyright 2020-2022 Aumoa.lib. All right reserved.

using Dapper;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using MySql.Data.MySqlClient;

namespace DbConnection.Migration;

public abstract class DbMigrationWorker : BackgroundService
{
    public record Configuration
    {
        public required string ConnectionString { get; init; }

        public required ulong Version { get; init; }
    }

    private readonly Configuration Config;
    private readonly ILogger Logger;

    public DbMigrationWorker(IOptions<Configuration> Config, ILogger<DbMigrationWorker> Logger)
    {
        this.Config = Config.Value;
        this.Logger = Logger;
    }

    protected abstract Task<Dictionary<ulong, DbMigrateLevel>> GetMigrateLevelsAsync(CancellationToken SToken = default);

    protected override async Task ExecuteAsync(CancellationToken SToken = default)
    {
        using var Conn = new MySqlConnection(Config.ConnectionString);
        Conn.Open();

        using var Transaction = await Conn.BeginTransactionAsync(SToken);
        Dictionary<ulong, DbMigrateLevel> MigrateLevels = await GetMigrateLevelsAsync(SToken);
        bool bExists = false;

        try
        {
            const string QUERY = "SELECT `Version` FROM `MigrateHistory` WHERE `InstalledRank` = 0";
            var Row = await Conn.QueryFirstAsync(QUERY);

            if ((ulong)Row.Version != Config.Version)
            {
                Logger.LogCritical("Schema version is mismatched. Please migrate version manually.");
                return;
            }

            bExists = true;
        }
        catch (Exception)
        {
            bExists = false;
        }

        if (bExists == false)
        {
            const string QUERY = """
                CREATE TABLE `MigrateHistory` (
                    `InstalledRank` BIGINT UNSIGNED NOT NULL,
                    `Version` BIGINT UNSIGNED NOT NULL,
                    `Name` VARCHAR(128) NOT NULL,
                    `InstalledDate` DATETIME NOT NULL DEFAULT NOW(),
                    `Hash` BIGINT UNSIGNED NOT NULL,
                    `bSuccess` BOOL NOT NULL,
                    PRIMARY KEY (`InstalledRank`),
                    INDEX `IDX__InstalledRank__Version` (`Version`),
                    INDEX `IDX__InstalledRank__InstalledDate` (`InstalledDate`),
                    INDEX `IDX__InstalledRank__bSuccess` (`bSuccess`)
                );

                INSERT INTO `MigrateHistory`(`InstalledRank`, `Version`, `Name`, `Hash`, `bSuccess`)
                    VALUES(0, @Version, 'Init', 0, TRUE);
            """;

            await Conn.ExecuteAsync(QUERY, new { Config.Version });
        }
        else
        {
            string QUERY = """
                SELECT * FROM `MigrateHistory` WHERE `bSuccess` = FALSE
            """;
            var Rows = await Conn.QueryAsync(QUERY);
            if (Rows.Any())
            {
                Logger.LogCritical("{} rows is not successfully migrated. Please edit table information manually. InstalledRanks = {}", Rows.Count(), string.Join(", ", Rows.Select(p => (ulong)p.InstalledRank)));
                return;
            }

            QUERY = """
                SELECT * FROM `MigrateHistory`
            """;
            Rows = await Conn.QueryAsync(QUERY);
            foreach (var Row in Rows)
            {
                ulong Version = (ulong)Row.Version;
                MigrateLevels.Remove(Version);
            }
        }

        foreach (var (Level, Migrate) in MigrateLevels)
        {
            await Conn.ExecuteAsync(Migrate.Query);

            string QUERY = "SELECT COUNT(*) FROM `MigrateHistory`";
            ulong Count = await Conn.ExecuteScalarAsync<ulong>(QUERY);

            QUERY = """
                INSERT INTO `MigrateHistory`
                    (`InstalledRank`, `Version`, `Name`, `Hash`, `bSuccess`)
                    VALUES (@Count, @Version, @Name, @Hash, TRUE);
            """;
            await Conn.ExecuteAsync(QUERY, new { Count, Version = Migrate.Level, Migrate.Name, Hash = Migrate.GetQueryHashCode() });

            Logger.LogInformation("Migrate version {}__{} applied.", Level, Migrate.Name);
        }
    }
}
