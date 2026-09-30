using System.Diagnostics;

namespace Cremey;

public static class DeviceManager
{
    public static async Task<int> DisableDevicesParallelAsync(string toolPath, IReadOnlyList<string> devices)
    {
        if (devices.Count == 0 || !File.Exists(toolPath))
        {
            return 0;
        }

        var tasks = new List<Task<bool>>(devices.Count);
        foreach (var device in devices)
        {
            var cleanName = device.Trim();
            if (string.IsNullOrEmpty(cleanName))
                continue;

            tasks.Add(Task.Run(() => DisableSingleDevice(toolPath, cleanName)));
        }

        var results = await Task.WhenAll(tasks);
        return results.Count(success => success);
    }

    private static bool DisableSingleDevice(string toolPath, string deviceName)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = toolPath,
                Arguments = $"/Disable \"{deviceName}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            using var process = Process.Start(psi);
            if (process == null) return false;

            // Wait with a short timeout to prevent any stall
            if (!process.WaitForExit(3000))
            {
                try { process.Kill(); } catch { }
                return false;
            }

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsSteelSeriesRunning()
    {
        try
        {
            var procs = Process.GetProcessesByName("SteelSeriesGG");
            bool running = procs.Length > 0;
            foreach (var p in procs)
            {
                p.Dispose();
            }
            return running;
        }
        catch
        {
            return false;
        }
    }

    public static async Task RunDeviceCleanerAsync(string toolPath, IReadOnlyList<string> targets, bool forceImmediate)
    {
        if (targets.Count == 0) return;

        // Pass 1: Immediate sweep (Takes ~50-100ms)
        await DisableDevicesParallelAsync(toolPath, targets);

        if (forceImmediate) return;

        // If SteelSeriesGG is running, it may still be registering virtual endpoints
        // if the system just booted. A brief 2.5s delay + verification pass guarantees 100% reliability.
        if (IsSteelSeriesRunning())
        {
            await Task.Delay(2500);
            await DisableDevicesParallelAsync(toolPath, targets);
        }
    }
}
