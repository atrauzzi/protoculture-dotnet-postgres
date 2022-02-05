using System.Data.Common;

namespace Protoculture.Postgres.Embedded.Extensions;

public static class DbConnectionStringExtensions
{
    public static DbConnectionStringBuilder ForDatabase(this DbConnectionStringBuilder connectionStringBuilder, string database)
    {
        connectionStringBuilder.Add("Database", database);
        
        return connectionStringBuilder;
    }
}
