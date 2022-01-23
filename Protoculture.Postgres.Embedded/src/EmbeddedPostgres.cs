using System.Diagnostics;

namespace Protoculture.Postgres.Embedded;

public class EmbeddedPostgres : IDisposable
{
    private readonly EmbeddedPostgresConfiguration configuration;

    private TaskCompletionSource<bool> ready = new();

    private Process? serverProcess;
    
    public EmbeddedPostgres(EmbeddedPostgresConfiguration configuration)
    {
        this.configuration = configuration;
    }

    public async Task Start()
    {
        await InitDb();

        await StartDatabase();
    }

    public void Dispose()
    {
        Stop().Wait();
    }
    
    private async Task InitDb()
    {
        Directory.CreateDirectory(configuration.DataPath);
        
        var initdbProcessStartInfo = new ProcessStartInfo
        {
            FileName = configuration.ExecutablePath(PostgresExecutable.Initdb),
            Arguments = string.Join(" ", new List<string>
            {
                $"-D {configuration.DataPath}",
            }),
            Environment =
            {
                { "TZ", "UTC" },
            },
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        var initdbProcess = Process.Start(initdbProcessStartInfo);

        if (initdbProcess == null)
        {
            throw new("Problem running initdb.");
        }
        
        await initdbProcess.WaitForExitAsync();
    }

    private async Task StartDatabase()
    {
        ready = new();
        
        var serverProcessStartInfo = new ProcessStartInfo
        {
            FileName = configuration.ExecutablePath(PostgresExecutable.Postgres),
            Arguments = string.Join(" ", new List<string>
            {
                $"-D {configuration.DataPath}",
                $"-p {configuration.Port}",
                $"-k {configuration.SocketPath}",
            }),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        
        serverProcess = new()
        {
            StartInfo = serverProcessStartInfo,
        };
        
        serverProcess.OutputDataReceived += OnServerProcessOutputDataReceived;
        serverProcess.ErrorDataReceived += OnServerProcessOutputDataReceived;
        
        serverProcess.Start();

        serverProcess.BeginOutputReadLine();
        serverProcess.BeginErrorReadLine();

        await ready.Task;
    }

    public async Task Stop()
    {
        if (serverProcess == null)
        {
            return;
        }

        var pgctlProcess = Process.Start(new ProcessStartInfo
        {
            FileName = configuration.ExecutablePath(PostgresExecutable.Pgctl),
            Arguments = string.Join(" ", new List<string>
            {
                $"-D {configuration.DataPath}",
                "-m fast",
                "stop",
            }),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        });

        await pgctlProcess!.WaitForExitAsync();
    }
    
    private void OnServerProcessOutputDataReceived(object sender, DataReceivedEventArgs args)
    {
        if (args.Data == null)
        {
            return;
        }

        if (configuration.ShowOutput)
        {
            Console.WriteLine(args.Data);
        }
        
        if (args.Data.Contains("database system is ready to accept connections"))
        {
            ready.SetResult(true);
        }

        if (args.Data.Contains("database system is shut down"))
        {
            CleanUp();
        }
    }

    private void CleanUp()
    {
        serverProcess?.Dispose();
        serverProcess = null;
        
        Directory.Delete(configuration.BasePath, true);
        
        ready = new();
    }
}
