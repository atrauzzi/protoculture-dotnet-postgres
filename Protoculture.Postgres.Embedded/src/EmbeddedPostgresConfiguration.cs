using System.Reflection;
using static System.IO.Path;

namespace Protoculture.Postgres.Embedded;

public struct EmbeddedPostgresConfiguration
{
    public bool ShowOutput { get; init; } = true;

    public string ResourcesPath { get; init; } = $"{GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{DirectorySeparatorChar}postgres";

    public readonly string ExecutablePath(PostgresExecutable postgresExecutable) => $"{ResourcesPath}{DirectorySeparatorChar}bin{DirectorySeparatorChar}{postgresExecutable.ExecutableName()}";

    public string BasePath { get; init; } = $"{GetTempPath()}protoculture-postgres-embedded-{Guid.NewGuid()}";

    public int Port { get; init; } = new Random().Next(10000, 65534);

    public bool Transient { get; init; }

    public bool TerminateWhenDisposed { get; init; } = true;
    
    private readonly string dataPath = "data";

    public string DataPath
    {
        get => AbsoluteOrRelative(BasePath, dataPath);
        init => dataPath = value;
    }

    private readonly string socketPath = "";

    public string SocketPath
    {
        get => AbsoluteOrRelative(BasePath, socketPath);
        init => socketPath = value;
    }

    public string SocketFilePath => $"{SocketPath}{DirectorySeparatorChar}.s.PGSQL.{Port}";

    public string SocketConnectionString => $"Host={SocketPath};Port={Port};Database=postgres";
    
    private string AbsoluteOrRelative(string basePath, string path) => IsPathRooted(path) switch
    {
        true => path,
        false when string.IsNullOrEmpty(path) => basePath,
        false => $"{basePath}{DirectorySeparatorChar}{path}",
    };
}
