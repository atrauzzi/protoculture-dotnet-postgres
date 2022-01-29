using System;
using System.Runtime.InteropServices;
using static System.IO.Path;

namespace Protoculture.Postgres.Embedded;

public static class Util
{
    public static string CurrentOperatingSystem
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "linux";
            }
            
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "windows";
            }

            throw new NotSupportedException("Unsupported operating system.");
        }
    }

    public static string CurrentCpuArchitecture => RuntimeInformation.ProcessArchitecture switch
    {
        Architecture.X64 => "x86_64",
        _ => throw new NotSupportedException("Unsupported CPU architecture."),
    };
    
    public static string AbsoluteOrRelative(string basePath, string path) => IsPathRooted(path) switch
    {
        true => path,
        false when string.IsNullOrEmpty(path) => basePath,
        false => $"{basePath}{DirectorySeparatorChar}{path}",
    };
}
