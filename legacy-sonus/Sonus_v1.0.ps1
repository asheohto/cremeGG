<#
.SYNOPSIS
    Sonus - SteelSeries Sonar De-Clutter Utility
.DESCRIPTION
    Runs in background with a System Tray icon. Checks for startup entry.
    Dependencies: SoundVolumeView.exe (NirSoft)
#>

# --- Configuration ---
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# 1. Pathing: Get the location of the actual EXE file
$ScriptPath = [System.AppDomain]::CurrentDomain.BaseDirectory
$ToolPath   = "$ScriptPath\SoundVolumeView.exe"
$ExePath    = [System.Diagnostics.Process]::GetCurrentProcess().MainModule.FileName

# Validate dependency
if (-not (Test-Path $ToolPath)) {
    [System.Windows.Forms.MessageBox]::Show("Error: SoundVolumeView.exe not found!", "Sonus Error", "OK", "Error")
    Exit
}

# --- Auto-Startup Logic ---
$StartupFolder = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup"
$ShortcutPath  = "$StartupFolder\Sonus.lnk"

# Check if shortcut exists. If NOT, ask the user.
if (-not (Test-Path $ShortcutPath)) {
    $Question = "Do you want Sonus to run automatically when you log in?"
    $Result   = [System.Windows.Forms.MessageBox]::Show($Question, "Sonus Setup", "YesNo", "Question")

    if ($Result -eq "Yes") {
        try {
            $WScriptShell = New-Object -ComObject WScript.Shell
            $Shortcut = $WScriptShell.CreateShortcut($ShortcutPath)
            $Shortcut.TargetPath = $ExePath
            $Shortcut.WorkingDirectory = $ScriptPath
            $Shortcut.Description = "Sonus Audio Cleaner"
            $Shortcut.Save()
            
            [System.Windows.Forms.MessageBox]::Show("Success! Sonus will now run on startup.", "Sonus", "OK", "Information")
        } catch {
            [System.Windows.Forms.MessageBox]::Show("Could not create startup shortcut. You may need to do it manually.", "Error", "OK", "Error")
        }
    }
}

# --- System Tray Setup ---
$TrayIcon = New-Object System.Windows.Forms.NotifyIcon
# Extract icon from the running EXE
$TrayIcon.Icon = [System.Drawing.Icon]::ExtractAssociatedIcon($ExePath)
$TrayIcon.Text = "Sonus: Waiting for Sonar..."
$TrayIcon.Visible = $true

# Show start bubble
$TrayIcon.ShowBalloonTip(3000, "Sonus Started", "Waiting 30s for Sonar to launch...", "Info")

# --- Execution ---

# 2. Wait for SteelSeries Sonar
#Start-Sleep -Seconds 30

try {
    $TrayIcon.Text = "Sonus: Cleaning..."

    # 3. Force Headphone Default
    & $ToolPath /SetDefault "Headphone" all

    # 4. Disable Clutter
    & $ToolPath /Disable "SteelSeries Sonar - Gaming"
    & $ToolPath /Disable "SteelSeries Sonar - Chat"
    & $ToolPath /Disable "SteelSeries Sonar - Media"
    & $ToolPath /Disable "SteelSeries Sonar - Aux"
    & $ToolPath /Disable "SteelSeries Sonar - Stream"

    # 5. Success Notification
    $TrayIcon.ShowBalloonTip(3000, "Sonus", "Sonar has been vanquished! ^o^", "Info")
    
    # Wait a moment for bubble to be seen
    Start-Sleep -Seconds 3

} catch {
    $TrayIcon.ShowBalloonTip(3000, "Sonus Error", "Failed to clean audio devices.", "Error")
} finally {
    $TrayIcon.Visible = $false
    $TrayIcon.Dispose()
}
