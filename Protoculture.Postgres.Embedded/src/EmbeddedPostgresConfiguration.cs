using System;
using System.Reflection;
using System.Runtime.InteropServices;
using static System.IO.Path;
using static Protoculture.Postgres.Embedded.Util;

namespace Protoculture.Postgres.Embedded;

public class EmbeddedPostgresConfiguration
{
    public bool ShowOutput { get; init; } = true;

    public string ResourcesRoot { get; init; } = $"{GetDirectoryName(Assembly.GetExecutingAssembly().Location)}{DirectorySeparatorChar}postgres{DirectorySeparatorChar}{CurrentOperatingSystem}{DirectorySeparatorChar}{CurrentCpuArchitecture}";

    public string ExecutablePath(PostgresExecutable postgresExecutable) => $"{ResourcesRoot}{DirectorySeparatorChar}bin{DirectorySeparatorChar}{postgresExecutable.ExecutableName()}";

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

    public string TcpConnectionString => $"Host=127.0.0.1;Port={Port};Database=postgres";
    
    public bool SupportsSockets => CurrentOperatingSystem != "windows";
}
