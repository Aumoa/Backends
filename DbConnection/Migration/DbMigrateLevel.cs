// Copyright 2020-2022 Aumoa.lib. All right reserved.

using DbConnection.Misc;

namespace DbConnection.Migration;

public record DbMigrateLevel
{
    public required string Name { get; set; }

    public required int Level { get; set; }

    public required string Query { get; set; }

    public ulong GetQueryHashCode()
    {
        return CRC64.Generate64(Query);
    }
}
