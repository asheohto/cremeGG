using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Cremey;

public class NotificationPopup : Form
{
    private readonly Image? _bannerImage;
    private const int TargetDisplayWidth = 420;
    private const int ScreenMargin = 20;

    private const int SlideInDurationMs = 320;
    private const int HoldDurationMs = 1700;
    private const int SlideOutDurationMs = 280;

    private static readonly Color KeyColor = Color.FromArgb(1, 1, 1);

    protected override bool ShowWithoutActivation => true;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE (never steal focus from foreground applications/games)
            cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW (hide from Alt-Tab task switcher)
            return cp;
        }
    }

    public NotificationPopup(string imagePath)
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);

        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;

        BackColor = KeyColor;
        TransparencyKey = KeyColor;

        int calculatedWidth = TargetDisplayWidth;
        int calculatedHeight = 217;

        if (File.Exists(imagePath))
        {
            try
            {
                _bannerImage = Image.FromFile(imagePath);
                if (_bannerImage.Width > 0 && _bannerImage.Height > 0)
                {
                    calculatedHeight = (int)Math.Round(calculatedWidth * (double)_bannerImage.Height / _bannerImage.Width);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load notification image: {ex.Message}");
            }
        }

        Size = new Size(calculatedWidth, calculatedHeight);

        var screen = Screen.PrimaryScreen ?? Screen.AllScreens.FirstOrDefault();
        var workArea = screen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);

        int endX = workArea.Right - Width - ScreenMargin;
        int targetY = workArea.Bottom - Height - ScreenMargin;
        int startX = workArea.Right + 50;

        Location = new Point(startX, targetY);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_bannerImage != null)
        {
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            e.Graphics.DrawImage(_bannerImage, new Rectangle(0, 0, Width, Height));
        }
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        _ = Task.Run(async () =>
        {
            try
            {
                var screen = Screen.PrimaryScreen ?? Screen.AllScreens.FirstOrDefault();
                var workArea = screen?.WorkingArea ?? new Rectangle(0, 0, 1920, 1080);

                int endX = workArea.Right - Width - ScreenMargin;
                int startX = workArea.Right + 50;
                int y = workArea.Bottom - Height - ScreenMargin;

                // Slide In
                var sw = Stopwatch.StartNew();
                while (sw.ElapsedMilliseconds < SlideInDurationMs)
                {
                    double t = (double)sw.ElapsedMilliseconds / SlideInDurationMs;
                    if (t > 1.0) t = 1.0;
                    double ease = 1.0 - Math.Pow(1.0 - t, 3.0); // Ease-out cubic

                    int curX = (int)Math.Round(startX + (endX - startX) * ease);

                    if (IsHandleCreated && !IsDisposed)
                    {
                        BeginInvoke(() => Location = new Point(curX, y));
                    }
                    await Task.Delay(14);
                }

                if (IsHandleCreated && !IsDisposed)
                {
                    BeginInvoke(() => Location = new Point(endX, y));
                }

                // Hold
                await Task.Delay(HoldDurationMs);

                // Slide Out
                sw.Restart();
                while (sw.ElapsedMilliseconds < SlideOutDurationMs)
                {
                    double t = (double)sw.ElapsedMilliseconds / SlideOutDurationMs;
                    if (t > 1.0) t = 1.0;
                    double ease = Math.Pow(t, 3.0); // Ease-in cubic

                    int curX = (int)Math.Round(endX + (startX - endX) * ease);

                    if (IsHandleCreated && !IsDisposed)
                    {
                        BeginInvoke(() => Location = new Point(curX, y));
                    }
                    await Task.Delay(14);
                }

                if (IsHandleCreated && !IsDisposed)
                {
                    BeginInvoke(Close);
                }
            }
            catch
            {
                if (IsHandleCreated && !IsDisposed)
                {
                    BeginInvoke(Close);
                }
            }
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _bannerImage?.Dispose();
        }
        base.Dispose(disposing);
    }

    public static void ShowNotificationModal(string imagePath)
    {
        using var popup = new NotificationPopup(imagePath);
        Application.Run(popup);
    }
}
