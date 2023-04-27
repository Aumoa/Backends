// Copyright 2020-2022 Aumoa.lib. All right reserved.

using DbConnection.Migration;

using Microsoft.Extensions.Options;

using OAuthService.Options;

namespace OAuthService.Services;

public class MigrationWorker : DbMigrationWorker
{
    public MigrationWorker(IOptions<DbAccountsOption> Config, ILogger<MigrationWorker> Logger)
        : base(Config, Logger)
    {
    }

    protected override Task<Dictionary<ulong, DbMigrateLevel>> GetMigrateLevelsAsync(CancellationToken SToken = default)
    {
        return Task.FromResult(new Dictionary<ulong, DbMigrateLevel>()
        {
            [1] = new DbMigrateLevel
            {
                Name = "CreateAccount",
                Level = 1,
                Query = """
                    CREATE TABLE `Account` (
                        `Id` BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
                        `PlatformId` VARCHAR(128) NOT NULL,
                        `CreateDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        `UpdateDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        PRIMARY KEY (`Id`),
                        UNIQUE INDEX `UNQ__Account__PlatformId` (`PlatformId`),
                        INDEX `IDX__Account__UpdateDate` (`UpdateDate`)
                    );
                """
            },
            [2] = new DbMigrateLevel
            {
                Name = "CreateAttribute",
                Level = 2,
                Query = """
                    CREATE TABLE `Attribute` (
                	    `Id` BIGINT UNSIGNED NOT NULL,
                        `Category` SMALLINT UNSIGNED NOT NULL,
                        `Content` JSON NOT NULL,
                        `CreateDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        `UpdateDate` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                        PRIMARY KEY (`Id`, `Category`)
                    );

                    CREATE TABLE `Permission` (
                        `Id` BIGINT UNSIGNED NOT NULL,
                        `Category` SMALLINT UNSIGNED NOT NULL,
                        PRIMARY KEY (`Id`, `Category`)
                    );
                """
            },
            [3] = new DbMigrateLevel
            {
                Name = "AddPassword",
                Level = 3,
                Query = """
                    ALTER TABLE `Account`
                        ADD COLUMN `Password` VARCHAR(64) NOT NULL AFTER `PlatformId`;
                """
            }
        });
    }
}