using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
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

    public bool NotRunning => ! Running;
    
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

        await StartServer();
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

    public async Task Stop()
    {
        EnsureRunning();

        await CommandUtils.Run(Configuration.ExecutablePath(PostgresExecutable.Pgctl), new List<string>
        {
            $"-D {Configuration.DataPath}",
            "-m fast",
            "stop",
        });

        await shutdown.Task;
    }

    public async Task CreateDatabase(string name)
    {
        EnsureRunning();

        var arguments = new List<string>
        {
            $"--port {Configuration.Port}",
        };
        
        if (Configuration.UseSockets)
        {
            arguments.Add($"--host {Configuration.SocketPath}");
        }

        arguments.Add(name);

        await CommandUtils.Run(Configuration.ExecutablePath(PostgresExecutable.Createdb), arguments);
    }

    private async Task InitDb()
    {
        if (Directory.Exists(Configuration.DataPath))
        {
            return;
        }
        
        Directory.CreateDirectory(Configuration.DataPath);

        await CommandUtils.Run(
            Configuration.ExecutablePath(PostgresExecutable.Initdb), 
            new List<string>
            {
                $"-D {Configuration.DataPath}",
            },
            new Dictionary<string, string?>
            {
                { "TZ", "UTC" },
            }
        );
    }

    private async Task StartServer()
    {
        ResetControlStates();

        var arguments = new List<string>
        {
            $"-D {Configuration.DataPath}",
            $"-p {Configuration.Port}",
        };

        if (Configuration.UseSockets)
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

        var timeout = TimeSpan.FromSeconds(30);
        var deadline = DateTimeOffset.Now.Add(timeout);

        while (DateTimeOffset.Now < deadline)
        {
            if (serverProcess.HasExited)
            {
                throw new("Postgres process exited unexpectedly before starting.");
            }

            try
            {
                using var tcpClient = new TcpClient();
                await tcpClient.ConnectAsync("127.0.0.1", Configuration.Port);
                initialization.SetResult(true);
                return;
            }
            catch
            {
                // ignored
            }

            await Task.Delay(500);
        }

        throw new("Postgres failed to start within 30 seconds.");
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

    private void EnsureRunning()
    {
        if (NotRunning)
        {
            throw new("Server is not running.");
        }
    }
}
