using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Protoculture.Postgres.Embedded;

public sealed class EmbeddedPostgres : IDisposable, IAsyncDisposable
{
    public readonly EmbeddedPostgresConfiguration Configuration;

    private TaskCompletionSource<bool> initialization = new();
    private TaskCompletionSource<bool> shutdown = new();

    private Process? serverProcess;

    public bool Running => (
        initialization.Task.IsCompleted 
        && initialization.Task.Result
        && ! shutdown.Task.IsCompleted
        && ((! serverProcess?.HasExited) ?? false)
    );
    
    public EmbeddedPostgres()
    {
        Configuration = new();
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
        ResetControlStates();

        var arguments = new List<string>
        {
            $"-D {Configuration.DataPath}",
            $"-p {Configuration.Port}",
        };

        if (Configuration.SupportsSockets)
        {
            arguments.Add($"-k {Configuration.SocketPath}");
        }

        var serverProcessStartInfo = new ProcessStartInfo
        {
            FileName = Configuration.ExecutablePath(PostgresExecutable.Postgres),
            Arguments = string.Join(" ", arguments),
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

        serverProcess.EnableRaisingEvents = true;
        serverProcess.BeginOutputReadLine();
        serverProcess.BeginErrorReadLine();

        await initialization.Task;
    }

    public async Task Stop()
    {
        if (serverProcess == null)
        {
            return;
        }

        var pgctlProcessInfo = new ProcessStartInfo
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
        };

        var pgctlProcess = Process.Start(pgctlProcessInfo);
       
        await pgctlProcess!.WaitForExitAsync();
        await shutdown.Task;
    }

    private void ResetControlStates()
    {
        initialization = new();
        shutdown = new();
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
            initialization.SetResult(true);
        }

        if (args.Data.Contains("Execution of PostgreSQL by a user with administrative permissions is not permitted"))
        {
            initialization.SetResult(false);
            
            throw new("Postgres cannot be run as an administrator.");
        }
    }

    private void OnServerProcessExited(object? sender, EventArgs e)
    {
        CleanUp();
        
        shutdown.SetResult(true);
    }

    private void CleanUp()
    {
        serverProcess?.Dispose();
        serverProcess = null;

        if (Configuration.Transient)
        {
            Directory.Delete(Configuration.BasePath, true);
        }
        
        initialization = new(false);
    }
}
