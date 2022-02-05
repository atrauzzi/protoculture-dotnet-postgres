using System;
using System.Data.Common;
using static System.IO.Path;
using static Protoculture.Postgres.Embedded.EnvironmentUtils;

namespace Protoculture.Postgres.Embedded;

public class EmbeddedPostgresConfiguration
{
    public bool ShowOutput { get; init; } = true;

    public string ResourcesRoot { get; init; } = $"{AssemblyPath}{Slash}postgres{Slash}{CurrentOperatingSystem}{Slash}{CurrentCpuArchitecture}";

    public string LibPath => $"{ResourcesRoot}{Slash}lib";
    
    public string ExecutablePath(PostgresExecutable postgresExecutable) => $"{ResourcesRoot}{Slash}bin{Slash}{postgresExecutable.ExecutableName()}";

    public string BasePath { get; init; } = $"{GetTempPath()}protoculture-postgres-embedded-{Guid.NewGuid()}";

    public int Port { get; init; } = new Random().Next(10000, 65534);

    public bool Transient { get; init; }

    public bool TerminateWhenDisposed { get; init; } = true;

    public bool UseSockets { get; init; } = CurrentOperatingSystem != "windows";
    
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
    
    public string SocketFilePath => $"{SocketPath}{Slash}.s.PGSQL.{Port}";

    public DbConnectionStringBuilder SocketConnectionString => new()
    {
        { "Host", SocketPath },
        { "Port", Port },
    };

    public DbConnectionStringBuilder TcpConnectionString => new()
    {
        { "Host", "127.0.0.1" },
        { "Port", Port },
    };

    public DbConnectionStringBuilder BestConnectionString => UseSockets
        ? SocketConnectionString
        : TcpConnectionString;
}
