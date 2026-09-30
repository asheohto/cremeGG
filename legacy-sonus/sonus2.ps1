Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

if ($PSScriptRoot) {
    $ScriptPath = $PSScriptRoot
} else {
      $ScriptPath = [System.AppDomain]::CurrentDomain.BaseDirectory
}

$ScriptPath = $ScriptPath.TrimEnd('\')
$ToolPath = "$ScriptPath\SoundVolumeView.exe"
$ConfigFile = "$ScriptPath\SonusConfig.txt"
$ImagePath =   "$ScriptPath\silly.jpg"
$ExePath = [System.Diagnostics.Process]::GetCurrentProcess().MainModule.FileName

if (-not (Test-Path $ToolPath)) {
    [System.Windows.Forms.MessageBox]::Show("Error: SoundVolumeView.exe not found in folder!`nChecked: $ToolPath", "Sonus Error", "OK", "Error")
    Exit
}

if (-not (Test-Path $ConfigFile)) {
    
    $StartupFolder = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs\Startup"
    $ShortcutPath = "$StartupFolder\Sonus2.lnk"
    $UserChoice = "No"

    if (Test-Path $ImagePath) {
        $Form = New-Object System.Windows.Forms.Form
        $Form.Text = "Sonus Setup"
        $Form.Size = New-Object System.Drawing.Size(400, 450)
        $Form.StartPosition = "CenterScreen"
        $Form.FormBorderStyle = "FixedDialog"
        $Form.MaximizeBox = $false
        $Form.MinimizeBox = $false

        $PictureBox = New-Object System.Windows.Forms.PictureBox
        $PictureBox.Size = New-Object System.Drawing.Size(360, 250)
        $PictureBox.Location = New-Object System.Drawing.Point(10, 10)
        $PictureBox.ImageLocation = $ImagePath
        $PictureBox.SizeMode = "Zoom"
        $Form.Controls.Add($PictureBox)

        $Label = New-Object System.Windows.Forms.Label
        $Label.Location = New-Object System.Drawing.Point(10, 270)
        $Label.Size = New-Object System.Drawing.Size(360, 60)
        $Label.Text = "Welcome to Sonus!`n`nDo you want this tool to run automatically when you log in?"
        $Label.TextAlign = "TopCenter"
        $Label.Font = New-Object System.Drawing.Font("Segoe UI", 10)
        $Form.Controls.Add($Label)

        $BtnYes = New-Object System.Windows.Forms.Button
        $BtnYes.Location = New-Object System.Drawing.Point(90, 340)
        $BtnYes.Size = New-Object System.Drawing.Size(100, 35)
        $BtnYes.Text = "Yes please!"
        $BtnYes.DialogResult = "Yes"
        $Form.Controls.Add($BtnYes)

        $BtnNo = New-Object System.Windows.Forms.Button
        $BtnNo.Location = New-Object System.Drawing.Point(200, 340)
        $BtnNo.Size = New-Object System.Drawing.Size(100, 35)
        $BtnNo.Text = "No thanks"
        $BtnNo.DialogResult = "No"
        $Form.Controls.Add($BtnNo)

        $Result = $Form.ShowDialog()
        if ($Result -eq "Yes") { 
             $UserChoice = "Yes" 
        }
        $Form.Dispose()

    } else {
        $Question = "Welcome to Sonus!`n`nDo you want this tool to run automatically when you log in?"
        $Result = [System.Windows.Forms.MessageBox]::Show($Question, "Sonus Setup", "YesNo", "Question")
        if ($Result -eq "Yes") { 
            $UserChoice = "Yes" 
        }
    }

    if ($UserChoice -eq "Yes") {
        try {
            $WScriptShell = New-Object -ComObject WScript.Shell
            $Shortcut = $WScriptShell.CreateShortcut($ShortcutPath)
            $Shortcut.TargetPath = $ExePath
            $Shortcut.WorkingDirectory = $ScriptPath
            $Shortcut.Description = "Sonus Audio Cleaner"
            $Shortcut.Save()
            
            [System.Windows.Forms.MessageBox]::Show("Success! Sonus will now run on startup.", "Sonus", "OK", "Information")
        } catch {
             [System.Windows.Forms.MessageBox]::Show("Could not create startup shortcut.", "Error", "OK", "Error")
        }
    }

    "SetupCompleted=True" | Out-File $ConfigFile -Encoding UTF8
}

$TrayIcon = New-Object System.Windows.Forms.NotifyIcon
$TrayIcon.Icon = [System.Drawing.Icon]::ExtractAssociatedIcon($ExePath)
$TrayIcon.Text = "Sonus: Monitoring..."
$TrayIcon.Visible = $true

$MaxRetries = 60 
$SonarFound = $false

for ($i = 0; $i -lt $MaxRetries; $i++) {
    
    $SSProcess = Get-Process "SteelSeriesGG" -ErrorAction SilentlyContinue
    
    if ($SSProcess) {
        $TrayIcon.Text = "Sonus: Sonar detected! Cleaning..."
        Start-Sleep -Seconds 8 
        $SonarFound = $true
        break
    }
    
    Start-Sleep -Seconds 2
}

if (-not $SonarFound) {
    $TrayIcon.Text = "Sonus: Sonar not found."
    Start-Sleep -Seconds 5
}

try {
    if ($SonarFound) {
       & $ToolPath /Disable "SteelSeries Sonar - Gaming"
       & $ToolPath /Disable "SteelSeries Sonar - Chat"
       & $ToolPath /Disable "SteelSeries Sonar - Media"
       & $ToolPath /Disable "SteelSeries Sonar - Aux"
       & $ToolPath /Disable "SteelSeries Sonar - Stream"
       & $ToolPath /Disable "SteelSeries Sonar - Microphone"

       $TrayIcon.ShowBalloonTip(3000, "Sonus", "Sonar devices disabled.", "Info")
       Start-Sleep -Seconds 4
    }
} catch {
    $TrayIcon.ShowBalloonTip(3000, "Sonus Error", "Failed to disable devices.", "Error")
} finally {
    $TrayIcon.Visible = $false
    $TrayIcon.Dispose()
}