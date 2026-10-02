using System.Drawing;
using System.Windows.Forms;

namespace CremeGG;

public class SetupForm : Form
{
    private readonly string _configFile;
    private readonly string _imagePath;
    private readonly string _exePath;
    private readonly string _baseDir;

    private readonly CheckedListBox _checkList;
    private readonly CheckBox _chkStartup;
    private readonly Button _btnSave;

    public List<string> SelectedDevices { get; private set; } = new();

    public SetupForm(string configFile, string imagePath, string exePath, string baseDir)
    {
        _configFile = configFile;
        _imagePath = imagePath;
        _exePath = exePath;
        _baseDir = baseDir;

        // Window properties
        Text = "CremeGG Setup";
        Size = new Size(400, 520);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Font = new Font("Segoe UI", 9f);

        string logoIco = Path.Combine(_baseDir, "assets", "logo.ico");
        if (File.Exists(logoIco))
        {
            try { Icon = new Icon(logoIco); } catch { }
        }

        // Header / PictureBox
        int currentY = 10;
        if (File.Exists(_imagePath))
        {
            try
            {
                var pic = new PictureBox
                {
                    Location = new Point(10, currentY),
                    Size = new Size(360, 180),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Image = Image.FromFile(_imagePath)
                };
                Controls.Add(pic);
                currentY += 190;
            }
            catch { }
        }

        // Instruction label
        var label = new Label
        {
            Location = new Point(12, currentY),
            Size = new Size(360, 40),
            Text = "Select the Sonar devices you want to DISABLE/HIDE:\n(Unchecked items will be kept active)",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold)
        };
        Controls.Add(label);
        currentY += 45;

        // Checked list box
        _checkList = new CheckedListBox
        {
            Location = new Point(20, currentY),
            Size = new Size(340, 110),
            CheckOnClick = true
        };

        // Determine existing target selections if config exists
        HashSet<string>? existingTargets = null;
        if (File.Exists(_configFile))
        {
            var raw = File.ReadAllText(_configFile).Trim();
            if (!string.Equals(raw, "NONE", StringComparison.OrdinalIgnoreCase))
            {
                existingTargets = new HashSet<string>(
                    raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
                    StringComparer.OrdinalIgnoreCase
                );
            }
        }

        string[] standardDevices =
        [
            "SteelSeries Sonar - Gaming",
            "SteelSeries Sonar - Chat",
            "SteelSeries Sonar - Media",
            "SteelSeries Sonar - Aux",
            "SteelSeries Sonar - Stream"
        ];

        foreach (var device in standardDevices)
        {
            bool isChecked = existingTargets == null || existingTargets.Contains(device);
            _checkList.Items.Add(device, isChecked);
        }
        Controls.Add(_checkList);
        currentY += 120;

        // Startup checkbox
        _chkStartup = new CheckBox
        {
            Text = "Run CremeGG automatically on startup?",
            Location = new Point(20, currentY),
            Size = new Size(340, 25),
            Checked = File.Exists(_configFile) ? ShortcutManager.IsStartupEnabled() : true
        };
        Controls.Add(_chkStartup);
        currentY += 35;

        // Finish button
        _btnSave = new Button
        {
            Location = new Point(135, currentY),
            Size = new Size(110, 34),
            Text = "Finish",
            Font = new Font("Segoe UI", 9f, FontStyle.Bold),
            DialogResult = DialogResult.OK
        };
        _btnSave.Click += OnSaveClicked;
        Controls.Add(_btnSave);

        AcceptButton = _btnSave;
        ClientSize = new Size(384, currentY + 50);
    }

    private void OnSaveClicked(object? sender, EventArgs e)
    {
        SelectedDevices.Clear();
        foreach (var item in _checkList.CheckedItems)
        {
            if (item != null)
            {
                SelectedDevices.Add(item.ToString()!);
            }
        }

        // 1. Write Config
        string configData = SelectedDevices.Count > 0 ? string.Join(",", SelectedDevices) : "NONE";
        File.WriteAllText(_configFile, configData, System.Text.Encoding.UTF8);

        // 2. Handle Startup
        ShortcutManager.SetStartup(_chkStartup.Checked, _exePath, _baseDir);

        MessageBox.Show("Setup Complete!\nCremeGG will run automatically on startup.", "CremeGG", MessageBoxButtons.OK, MessageBoxIcon.Information);
        Close();
    }
}
