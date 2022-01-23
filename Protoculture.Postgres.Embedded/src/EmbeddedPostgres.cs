using System.Diagnostics;

namespace Protoculture.Postgres.Embedded;

public sealed class EmbeddedPostgres : IDisposable, IAsyncDisposable
{
    public readonly EmbeddedPostgresConfiguration Configuration;

    private TaskCompletionSource<bool> ready = new();

    private Process? serverProcess;

    public bool Running => ready.Task.IsCompleted && ready.Task.Result;
    
    public EmbeddedPostgres()
    {
        Configuration = new()
        {
            Transient = true,
        };
    }
    
    public EmbeddedPostgres(EmbeddedPostgresConfiguration configuration)
    {
        Configuration = configuration;
    }

    public async Task Start()
    {
        await InitDb();

        await StartDatabase();
    }

    public async ValueTask DisposeAsync()
    {
        if (Configuration.TerminateWhenDisposed)
        {
            await Stop();
        }
    }
    
    public void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }
    
    private async Task InitDb()
    {
        Directory.CreateDirectory(Configuration.DataPath);
        
        var initdbProcessStartInfo = new ProcessStartInfo
        {
            FileName = Configuration.ExecutablePath(PostgresExecutable.Initdb),
            Arguments = string.Join(" ", new List<string>
            {
                $"-D {Configuration.DataPath}",
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
            FileName = Configuration.ExecutablePath(PostgresExecutable.Postgres),
            Arguments = string.Join(" ", new List<string>
            {
                $"-D {Configuration.DataPath}",
                $"-p {Configuration.Port}",
                $"-k {Configuration.SocketPath}",
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
        serverProcess.Exited += OnServerProcessExited;
        
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
            FileName = Configuration.ExecutablePath(PostgresExecutable.Pgctl),
            Arguments = string.Join(" ", new List<string>
            {
                $"-D {Configuration.DataPath}",
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

        if (Configuration.ShowOutput)
        {
            Console.WriteLine(args.Data);
        }
        
        if (args.Data.Contains("database system is ready to accept connections"))
        {
            ready.SetResult(true);
        }

        if (args.Data.Contains("database system is shut down"))
        {
            ready = new(false);
        }
    }

    private void OnServerProcessExited(object? sender, EventArgs e)
    {
        CleanUp();
    }

    private void CleanUp()
    {
        serverProcess?.Dispose();
        serverProcess = null;

        if (Configuration.Transient)
        {
            Directory.Delete(Configuration.BasePath, true);
        }
        
        ready = new();
    }
}
