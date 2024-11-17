using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("System Information:");
        Console.WriteLine("-------------------");

        // Get CPU information
        GetCpuInfo();

        // Get GPU information
        GetGpuInfo();

        // Get RAM information
        GetRamInfo();
    }

    static void GetCpuInfo()
    {
        string cpuInfo = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? GetWindowsCpuInfo() : GetLinuxCpuInfo();
        Console.WriteLine($"CPU: {cpuInfo}");
    }

    static string GetWindowsCpuInfo()
    {
        try
        {
            using (var searcher = new System.Management.ManagementObjectSearcher("select Name from Win32_Processor"))
            {
                foreach (var item in searcher.Get())
                {
                    return item["Name"].ToString();
                }
            }
        }
        catch
        {
            return "Unknown CPU";
        }
        return "Unknown CPU";
    }

    static string GetLinuxCpuInfo()
    {
        try
        {
            var cpuInfo = File.ReadAllText("/proc/cpuinfo");
            var lines = cpuInfo.Split('\n');
            foreach (var line in lines)
            {
                if (line.StartsWith("model name"))
                {
                    return line.Split(':')[1].Trim();
                }
            }
        }
        catch
        {
            return "Unable to retrieve CPU info";
        }
        return "Unknown CPU";
    }

    static void GetGpuInfo()
    {
        string gpuInfo = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? GetWindowsGpuInfo() : GetLinuxGpuInfo();
        Console.WriteLine($"GPU: {gpuInfo}");
    }

    static string GetWindowsGpuInfo()
    {
        try
        {
            using (var searcher = new System.Management.ManagementObjectSearcher("select Name from Win32_VideoController"))
            {
                foreach (var item in searcher.Get())
                {
                    return item["Name"].ToString();
                }
            }
        }
        catch
        {
            return "Unknown GPU";
        }
        return "Unknown GPU";
    }

    static string GetLinuxGpuInfo()
    {
        try
        {
            var gpuInfo = RunCommand("lshw -C display");
            return gpuInfo;
        }
        catch
        {
            return "Unable to retrieve GPU info";
        }
    }

    static void GetRamInfo()
    {
        string ramInfo = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? GetWindowsRamInfo() : GetLinuxRamInfo();
        Console.WriteLine($"RAM: {ramInfo}");
    }

    static string GetWindowsRamInfo()
    {
        try
        {
            using (var searcher = new System.Management.ManagementObjectSearcher("select Capacity from Win32_PhysicalMemory"))
            {
                ulong totalCapacity = 0;
                foreach (var item in searcher.Get())
                {
                    totalCapacity += (ulong)item["Capacity"];
                }
                return $"{totalCapacity / (1024 * 1024 * 1024)} GB";
            }
        }
        catch
        {
            return "Unknown RAM";
        }
    }

    static string GetLinuxRamInfo()
    {
        try
        {
            var memInfo = File.ReadAllText("/proc/meminfo");
            var lines = memInfo.Split('\n');
            foreach (var line in lines)
            {
                if (line.StartsWith("MemTotal"))
                {
                    var parts = line.Split(':');
                    return $"{long.Parse(parts[1].Trim().Split(' ')[0]) / (1024 * 1024)} GB";
                }
            }
        }
        catch
        {
            return "Unable to retrieve RAM info";
        }
        return "Unknown RAM";
    }

    static string RunCommand(string command)
    {
        var processInfo = new ProcessStartInfo("bash", $"-c \"{command}\"")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process .Start(processInfo))
        {
            using (var reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                return result.Trim();
            }
        }
    }
}
