using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Protoculture.Postgres.Embedded;

public static class CommandUtils
{
    public static async Task<int> Run(string executable, IEnumerable<string> arguments, IDictionary<string,string?>? environment = null)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = string.Join(" ", arguments),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        environment?.ToList().ForEach((pair) => startInfo.Environment.Add(pair));

        var process = Process.Start(startInfo);

        if (process == null)
        {
            throw new($"There was a problem starting {executable}.");
        }

        await process.WaitForExitAsync();

        return process.ExitCode;
    }
}
