Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

# --- PATH SETUP ---
if ($PSScriptRoot) {
    $ScriptPath = $PSScriptRoot
} else {
    $ScriptPath = [System.AppDomain]::CurrentDomain.BaseDirectory
}
$ScriptPath = $ScriptPath.TrimEnd('\')
$ToolPath = "$ScriptPath\SoundVolumeView.exe"
$ConfigFile = "$ScriptPath\SonusConfig.txt"
$ImagePath = "$ScriptPath\silly.jpg"
$ExePath = [System.Diagnostics.Process]::GetCurrentProcess().MainModule.FileName

# --- CHECK FOR TOOL ---
if (-not (Test-Path $ToolPath)) {
    [System.Windows.Forms.MessageBox]::Show("Error: SoundVolumeView.exe not found!`nChecked: $ToolPath", "Sonus Error", "OK", "Error")
    Exit
}

# --- CONFIGURATION & SETUP WIZARD ---
$Targets = @() # This will hold the devices the user wants to kill

if (-not (Test-Path $ConfigFile)) {
    # -- DEFINE THE SETUP FORM --
    $Form = New-Object System.Windows.Forms.Form
    $Form.Text = "Sonus Setup"
    $Form.Size = New-Object System.Drawing.Size(400, 550)
    $Form.StartPosition = "CenterScreen"
    $Form.FormBorderStyle = "FixedDialog"
    $Form.MaximizeBox = $false
    $Form.MinimizeBox = $false

    # -- IMAGE --
    if (Test-Path $ImagePath) {
        $PictureBox = New-Object System.Windows.Forms.PictureBox
        $PictureBox.Size = New-Object System.Drawing.Size(360, 180)
        $PictureBox.Location = New-Object System.Drawing.Point(10, 10)
        $PictureBox.ImageLocation = $ImagePath
        $PictureBox.SizeMode = "Zoom"
        $Form.Controls.Add($PictureBox)
    }

    # -- INSTRUCTION LABEL --
    $Label = New-Object System.Windows.Forms.Label
    $Label.Location = New-Object System.Drawing.Point(10, 200)
    $Label.Size = New-Object System.Drawing.Size(360, 40)
    $Label.Text = "Select the Sonar devices you want to DISABLE/HIDE:`n(Unchecked items will be kept active)"
    $Label.Font = New-Object System.Drawing.Font("Segoe UI", 9, [System.Drawing.FontStyle]::Bold)
    $Form.Controls.Add($Label)

    # -- CHECKLIST BOX --
    $CheckList = New-Object System.Windows.Forms.CheckedListBox
    $CheckList.Location = New-Object System.Drawing.Point(20, 245)
    $CheckList.Size = New-Object System.Drawing.Size(340, 100)
    $CheckList.CheckOnClick = $true
    
    # Add the standard Sonar devices
    [void]$CheckList.Items.Add("SteelSeries Sonar - Gaming", $true)
    [void]$CheckList.Items.Add("SteelSeries Sonar - Chat", $true)
    [void]$CheckList.Items.Add("SteelSeries Sonar - Media", $true)
    [void]$CheckList.Items.Add("SteelSeries Sonar - Aux", $true)
    [void]$CheckList.Items.Add("SteelSeries Sonar - Stream", $true)
    $Form.Controls.Add($CheckList)

    # -- STARTUP CHECKBOX --
    $ChkStartup = New-Object System.Windows.Forms.CheckBox
    $ChkStartup.Text = "Run Sonus automatically on startup?"
    $ChkStartup.Location = New-Object System.Drawing.Point(20, 360)
    $ChkStartup.Size = New-Object System.Drawing.Size(340, 30)
    $ChkStartup.Checked = $true
    $Form.Controls.Add($ChkStartup)

    # -- SAVE BUTTON --
    $BtnSave = New-Object System.Windows.Forms.Button
    $BtnSave.Location = New-Object System.Drawing.Point(140, 410)
    $BtnSave.Size = New-Object System.Drawing.Size(100, 35)
    $BtnSave.Text = "Finish"
    $BtnSave.DialogResult = "OK"
    $Form.Controls.Add($BtnSave)

    # -- SHOW FORM --
    $Result = $Form.ShowDialog()

    if ($Result -eq "OK") {
        # 1. Get Selected Devices
        $SelectedDevices = $CheckList.CheckedItems | ForEach-Object { $_.ToString() }
        
        # 2. Save Config File (Format: Device1,Device2,Device3)
        if ($SelectedDevices) {
            $ConfigString = $SelectedDevices -join ","
            $ConfigString | Out-File $ConfigFile -Encoding UTF8
        } else {
            "NONE" | Out-File $ConfigFile -Encoding UTF8
        }

        # 3. Handle Startup Shortcut
        if ($ChkStartup.Checked) {
            try {
                $StartupFolder = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup"
                $ShortcutPath = "$StartupFolder\Sonus2.lnk"
                $WScriptShell = New-Object -ComObject WScript.Shell
                $Shortcut = $WScriptShell.CreateShortcut($ShortcutPath)
                $Shortcut.TargetPath = $ExePath
                $Shortcut.WorkingDirectory = $ScriptPath
                $Shortcut.Description = "Sonus Audio Cleaner"
                $Shortcut.Save()
                [System.Windows.Forms.MessageBox]::Show("Setup Complete!`nSonus will run in the background.", "Sonus", "OK", "Information")
            } catch {
                [System.Windows.Forms.MessageBox]::Show("Could not create startup shortcut.", "Error", "OK", "Error")
            }
        }
    } else {
        Exit # User cancelled setup
    }
    $Form.Dispose()
}

# --- READ CONFIGURATION ---
if (Test-Path $ConfigFile) {
    $RawConfig = Get-Content $ConfigFile -Raw
    if ($RawConfig -ne "NONE") {
        $Targets = $RawConfig.Split(',')
    }
}

# If config is empty or "NONE", we have nothing to kill, so exit.
if ($Targets.Count -eq 0) {
    Exit
}

# --- TRAY ICON & MONITORING ---
$TrayIcon = New-Object System.Windows.Forms.NotifyIcon
$TrayIcon.Icon = [System.Drawing.Icon]::ExtractAssociatedIcon($ExePath)
$TrayIcon.Text = "Sonus: Monitoring..."
$TrayIcon.Visible = $true

$MaxWaitTime = 120 # 2 Minutes max wait time
$StartTime = Get-Date

do {
    # Check if process is running
    $SSProcess = Get-Process "SteelSeriesGG" -ErrorAction SilentlyContinue

    if ($SSProcess) {
        $TrayIcon.Text = "Sonus: Sonar detected! Cleaning..."
        
        # Wait 8 seconds for Sonar to fully initialize devices
        Start-Sleep -Seconds 8 
        
        # --- THE KILL PHASE ---
        try {
            foreach ($Device in $Targets) {
                # Trim spaces just in case
                $CleanName = $Device.Trim()
                if ($CleanName.Length -gt 0) {
                    & $ToolPath /Disable $CleanName
                }
            }
            $TrayIcon.ShowBalloonTip(3000, "Sonus", "Cleaned: $($Targets.Count) devices.", "Info")
        } catch {
            $TrayIcon.ShowBalloonTip(3000, "Sonus Error", "Failed to disable devices.", "Error")
        }

        Start-Sleep -Seconds 4
        break # Exit loop after cleaning
    }

    # Check timeout
    $TimeElapsed = (Get-Date) - $StartTime
    if ($TimeElapsed.TotalSeconds -ge $MaxWaitTime) {
        $TrayIcon.Text = "Sonus: Sonar not found (Timeout)."
        break
    }

    Start-Sleep -Seconds 2 
} while ($true)

# Cleanup
$TrayIcon.Visible = $false
$TrayIcon.Dispose()