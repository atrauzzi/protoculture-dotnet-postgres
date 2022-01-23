namespace Protoculture.Postgres.Embedded;

public enum PostgresExecutable
{
    Postgres = 1,
    Initdb,
    Pgctl,
}

public static class PostgresExecutableExtensions
{
    public static string ExecutableName(this PostgresExecutable postgresExecutable) => Enum.GetName(postgresExecutable)?.ToLower() switch
    {
        string name when name[..2].Equals("pg") => $"pg_{name[2..]}",
        string name => name,
        _ => throw new ArgumentOutOfRangeException(nameof(postgresExecutable), "Unknown or unsupported postgres executable."),
    };
}
