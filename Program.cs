using System.Diagnostics;
using System.Windows.Forms;

namespace Cremey;

internal static class Program
{
    [STAThread]
    private static async Task Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        string baseDir = AppContext.BaseDirectory.TrimEnd('\\');
        string exePath = Environment.ProcessPath ?? Path.Combine(baseDir, "Cremey.exe");
        string toolPath = Path.Combine(baseDir, "SoundVolumeView.exe");
        string configFile = Path.Combine(baseDir, "CremeyConfig.txt");

        // Migrate legacy config if present and new config doesn't exist
        if (!File.Exists(configFile) && File.Exists(Path.Combine(baseDir, "SonusConfig.txt")))
        {
            try { File.Copy(Path.Combine(baseDir, "SonusConfig.txt"), configFile, true); } catch { }
        }

        string assetsDir = Path.Combine(baseDir, "assets");
        string setupImagePath = File.Exists(Path.Combine(assetsDir, "silly.jpg"))
            ? Path.Combine(assetsDir, "silly.jpg")
            : Path.Combine(baseDir, "silly.jpg");

        string notifyImagePath = File.Exists(Path.Combine(assetsDir, "cleared.png"))
            ? Path.Combine(assetsDir, "cleared.png")
            : Path.Combine(baseDir, "cleared.png");

        bool forceNotifyOnly = args.Any(a => a.Equals("--notify", StringComparison.OrdinalIgnoreCase) ||
                                             a.Equals("-notify", StringComparison.OrdinalIgnoreCase));

        if (forceNotifyOnly)
        {
            NotificationPopup.ShowNotificationModal(notifyImagePath);
            return;
        }

        // Validate tool dependency
        if (!File.Exists(toolPath))
        {
            MessageBox.Show(
                $"Error: SoundVolumeView.exe not found!\nChecked: {toolPath}",
                "Cremey Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }

        bool forceSetup = args.Any(a => a.Equals("--setup", StringComparison.OrdinalIgnoreCase) ||
                                        a.Equals("-s", StringComparison.OrdinalIgnoreCase) ||
                                        a.Equals("/setup", StringComparison.OrdinalIgnoreCase));

        bool forceNow = args.Any(a => a.Equals("--now", StringComparison.OrdinalIgnoreCase) ||
                                      a.Equals("-n", StringComparison.OrdinalIgnoreCase));

        // Setup Mode (first run or explicitly requested)
        if (!File.Exists(configFile) || forceSetup)
        {
            using var setupForm = new SetupForm(configFile, setupImagePath, exePath, baseDir);
            var dialogResult = setupForm.ShowDialog();

            if (dialogResult != DialogResult.OK)
            {
                return; // User canceled
            }
        }

        // Clean / Disable Phase
        if (File.Exists(configFile))
        {
            string raw = (await File.ReadAllTextAsync(configFile)).Trim();
            if (string.Equals(raw, "NONE", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(raw))
            {
                return;
            }

            var targets = raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                             .Where(s => !string.IsNullOrEmpty(s))
                             .ToList();

            if (targets.Count > 0)
            {
                await DeviceManager.RunDeviceCleanerAsync(toolPath, targets, forceNow);

                // Show notification popup
                NotificationPopup.ShowNotificationModal(notifyImagePath);
            }
        }
    }
}