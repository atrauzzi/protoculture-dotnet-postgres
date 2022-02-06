using System.Data.Common;

namespace Protoculture.Postgres.Embedded.Extensions;

public static class DbConnectionStringExtensions
{
    public static DbConnectionStringBuilder ForDatabase(this DbConnectionStringBuilder connectionStringBuilder, string database)
    {
        connectionStringBuilder.Add("Database", database);
        
        return connectionStringBuilder;
    }

    public static DbConnectionStringBuilder IncludeErrorDetail(this DbConnectionStringBuilder connectionStringBuilder)
    {
        connectionStringBuilder.Add("Include Error Detail", "");
        
        return connectionStringBuilder;
    }
    
    public static DbConnectionStringBuilder LogParameters(this DbConnectionStringBuilder connectionStringBuilder)
    {
        connectionStringBuilder.Add("Log Parameters", "");
        
        return connectionStringBuilder;
    }
}
