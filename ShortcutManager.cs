using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace CremeGG;

public static class ShortcutManager
{
    private static readonly string StartupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
    private static readonly string PrimaryShortcutPath = Path.Combine(StartupFolder, "CremeGG.lnk");
    private static readonly string[] LegacyShortcutPaths =
    [
        Path.Combine(StartupFolder, "Cremey.lnk"), // rebrand: clean up the pre-CremeGG startup entry
        Path.Combine(StartupFolder, "Sonus.lnk"),
        Path.Combine(StartupFolder, "Sonus2.lnk")
    ];

    public static bool IsStartupEnabled()
    {
        return File.Exists(PrimaryShortcutPath) || LegacyShortcutPaths.Any(File.Exists);
    }

    public static bool SetStartup(bool enable, string targetExePath, string workingDirectory)
    {
        try
        {
            foreach (var legacyPath in LegacyShortcutPaths)
            {
                if (File.Exists(legacyPath))
                {
                    try { File.Delete(legacyPath); } catch { }
                }
            }

            if (!enable)
            {
                if (File.Exists(PrimaryShortcutPath))
                {
                    File.Delete(PrimaryShortcutPath);
                }
                return true;
            }

            return CreateShortcut(PrimaryShortcutPath, targetExePath, workingDirectory, "CremeGG Audio Cleaner");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to set startup shortcut: {ex.Message}");
            return false;
        }
    }

    private static bool CreateShortcut(string shortcutPath, string targetPath, string workingDir, string description)
    {
        try
        {
            // Primary method: Native Win32 COM IShellLinkW
            var clsidShellLink = new Guid("00021401-0000-0000-C000-000000000046");
            var iidShellLink = new Guid("000214F9-0000-0000-C000-000000000046");

            int hr = CoCreateInstance(in clsidShellLink, IntPtr.Zero, 1 /* CLSCTX_INPROC_SERVER */, in iidShellLink, out object objShellLink);
            if (hr == 0 && objShellLink is IShellLinkW shellLink)
            {
                shellLink.SetPath(targetPath);
                shellLink.SetWorkingDirectory(workingDir);
                shellLink.SetDescription(description);

                string iconPath = Path.Combine(workingDir, "assets", "logo.ico");
                if (!File.Exists(iconPath)) iconPath = targetPath;
                shellLink.SetIconLocation(iconPath, 0);

                if (shellLink is IPersistFile persistFile)
                {
                    persistFile.Save(shortcutPath, true);
                    return true;
                }
            }
        }
        catch
        {
            // Fallback: PowerShell WScript.Shell
        }

        return CreateShortcutFallback(shortcutPath, targetPath, workingDir, description);
    }

    private static bool CreateShortcutFallback(string shortcutPath, string targetPath, string workingDir, string description)
    {
        try
        {
            string iconPath = Path.Combine(workingDir, "assets", "logo.ico");
            if (!File.Exists(iconPath)) iconPath = targetPath;
            string psScript = $"$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut('{shortcutPath.Replace("'", "''")}'); $s.TargetPath = '{targetPath.Replace("'", "''")}'; $s.WorkingDirectory = '{workingDir.Replace("'", "''")}'; $s.IconLocation = '{iconPath.Replace("'", "''")},0'; $s.Description = '{description.Replace("'", "''")}'; $s.Save();";
            
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -NonInteractive -WindowStyle Hidden -Command \"{psScript}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            using var proc = Process.Start(psi);
            proc?.WaitForExit(3000);
            return File.Exists(shortcutPath);
        }
        catch
        {
            return false;
        }
    }

    [DllImport("ole32.dll", ExactSpelling = true)]
    private static extern int CoCreateInstance(
        in Guid rclsid,
        IntPtr pUnkOuter,
        uint dwClsContext,
        in Guid riid,
        [MarshalAs(UnmanagedType.Interface)] out object ppv);

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("000214F9-0000-0000-C000-000000000046")]
    private interface IShellLinkW
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, IntPtr pfd, uint fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
        void Resolve(IntPtr hwnd, uint fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0000010b-0000-0000-C000-000000000046")]
    private interface IPersistFile
    {
        void GetClassID(out Guid pClassID);
        void IsDirty();
        void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
        void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
        void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
        void GetCurFile([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder ppszFileName);
    }
}
