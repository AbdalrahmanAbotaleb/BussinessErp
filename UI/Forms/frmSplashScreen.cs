using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using BussinessErp.BLL;
using BussinessErp.Helpers;
using BussinessErp.Properties;

namespace BussinessErp.UI.Forms
{
    /// <summary>
    /// Professional animated splash screen.
    /// Features:
    ///   1. Dynamic version read from AssemblyInfo.
    ///   2. DB connection status icon (✔ / ⚠) shown after init.
    ///   3. Welcome-back greeting for the last logged-in user.
    /// </summary>
    public sealed class frmSplashScreen : Form
    {
        // ── Controls ──────────────────────────────────────────────────────────
        private Label _lblAppName;
        private Label _lblTagline;
        private Label _lblVersion;
        private Label _lblStatus;
        private Label _lblStatusIcon;   // ✔ or ⚠
        private Label _lblWelcome;      // "Welcome back, ahmed" greeting
        private Panel _progressBar;
        private Panel _progressTrack;
        private Panel _accentLine;

        // ── Animation state ───────────────────────────────────────────────────
        private Timer _fadeTimer;
        private bool  _fadingIn     = true;
        private bool  _initDone     = false;
        private bool  _dbConnected  = false;   // set by init result
        private int   _dotCount     = 0;
        private Timer _dotTimer;
        private int   _progressValue = 0;
        private Timer _progressTimer;

        // ── Theme ─────────────────────────────────────────────────────────────
        private static readonly Color _teal      = Color.FromArgb(0, 150, 136);
        private static readonly Color _tealDark  = Color.FromArgb(0, 121, 107);
        private static readonly Color _bg        = Color.FromArgb(28, 32, 38);
        private static readonly Color _bgCard    = Color.FromArgb(38, 44, 52);
        private static readonly Color _textWhite = Color.FromArgb(245, 246, 248);
        private static readonly Color _textMuted = Color.FromArgb(140, 150, 160);
        private static readonly Color _green     = Color.FromArgb(76, 175, 80);
        private static readonly Color _orange    = Color.FromArgb(255, 152, 0);

        private const int MIN_SPLASH_MS = 2200;
        private DateTime _splashStart;

        // ─────────────────────────────────────────────────────────────────────
        public frmSplashScreen()
        {
            InitializeComponent();
            SetupAnimationTimers();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Form Load
        // ─────────────────────────────────────────────────────────────────────
        protected override void OnLoad(EventArgs e)
        {
            _splashStart = DateTime.UtcNow;
            Opacity = 0;

            // ── Feature 1: Dynamic version ────────────────────────────────────
            var ver = Assembly.GetExecutingAssembly().GetName().Version;
            _lblVersion.Text = string.Format(LanguageManager.Get("splash_version"), ver.Major, ver.Minor, ver.Build);

            // ── Feature 3: Welcome-back greeting ─────────────────────────────
            string last = Settings.Default.LastUsername;
            if (!string.IsNullOrWhiteSpace(last))
            {
                _lblWelcome.Text    = string.Format(LanguageManager.Get("splash_welcome"), last);
                _lblWelcome.Visible = true;
            }

            // Ensure centering happens AFTER text is set
            if (this.Controls.Count > 0 && this.Controls[0] is Panel card)
            {
                // Find the logoPanel which is first child of card usually
                Panel logo = card.Controls.Count > 0 ? card.Controls[0] as Panel : null;
                CentreLabels(card, logo);
            }

            base.OnLoad(e); // Triggers Load event

            _fadeTimer.Start();
            _dotTimer.Start();
            _progressTimer.Start();

            _ = RunInitializationAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Async init
        // ─────────────────────────────────────────────────────────────────────
        private async Task RunInitializationAsync()
        {
            try
            {
                SetStatus(LanguageManager.Get("initializing_db"));
                await DatabaseHelper.InitializeDatabaseAsync();
                _dbConnected = true;

                SetStatus(LanguageManager.Get("warming_up")); // or reuse "Initializing DB" if simpler
                var auth = new AuthService();
                await auth.EnsureUserAsync("admin",    "admin123", "Admin");
                await auth.EnsureUserAsync("manager1", "admin123", "Manager");
                await auth.EnsureUserAsync("emp1",     "admin123", "Employee");

                SetStatus("..."); // Or just leave empty
                await Task.Delay(300);

                AppLogger.Info("Splash-screen initialization completed.");
            }
            catch (Exception ex)
            {
                AppLogger.Error("Splash initialization error", ex);
                _dbConnected = false;
                SetStatus(LanguageManager.Get("connection_error"));
                await Task.Delay(600);
            }
            finally
            {
                _initDone = true;
            }
        }

        // Thread-safe helpers
        private void SetStatus(string text)
        {
            if (InvokeRequired)
                Invoke((Action)(() => { _lblStatus.Tag = text; _lblStatus.Text = text; }));
            else
            {
                _lblStatus.Tag  = text;
                _lblStatus.Text = text;
            }
        }

        private void ShowStatusIcon()
        {
            // ── Feature 2: DB connection status icon ─────────────────────────
            if (_dbConnected)
            {
                _lblStatusIcon.Text      = "✔";
                _lblStatusIcon.ForeColor = _green;
            }
            else
            {
                _lblStatusIcon.Text      = "⚠";
                _lblStatusIcon.ForeColor = _orange;
            }
            _lblStatusIcon.Visible = true;
        }

        // ─────────────────────────────────────────────────────────────────────
        // Timers
        // ─────────────────────────────────────────────────────────────────────
        private void FadeTimer_Tick(object sender, EventArgs e)
        {
            if (_fadingIn)
            {
                Opacity += 0.07;
                if (Opacity >= 1.0)
                {
                    Opacity = 1.0;
                    _fadingIn = false;
                    _fadeTimer.Stop();
                    _ = WaitThenFadeOutAsync();
                }
            }
            else
            {
                Opacity -= 0.07;
                if (Opacity <= 0)
                {
                    Opacity = 0;
                    _fadeTimer.Stop();
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
        }

        private async Task WaitThenFadeOutAsync()
        {
            while (true)
            {
                int elapsed = (int)(DateTime.UtcNow - _splashStart).TotalMilliseconds;
                if (_initDone && elapsed >= MIN_SPLASH_MS)
                    break;
                await Task.Delay(80);
            }

            if (InvokeRequired)
                Invoke((Action)(() => SnapToReady()));
            else
                SnapToReady();

            await Task.Delay(500);
            _fadeTimer.Start();
        }

        private void SnapToReady()
        {
            _progressValue  = 100;
            _lblStatus.Tag  = LanguageManager.Get("splash_ready");
            _lblStatus.Text = LanguageManager.Get("splash_ready");
            _dotTimer.Stop();
            UpdateProgressBar();
            ShowStatusIcon();   // ← show ✔ or ⚠ now
        }

        private void DotTimer_Tick(object sender, EventArgs e)
        {
            _dotCount = (_dotCount + 1) % 4;
            string baseText = (_lblStatus.Tag as string) ?? _lblStatus.Text;
            _lblStatus.Text = baseText + new string('.', _dotCount);
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (_progressValue < 90)
            {
                _progressValue += _initDone ? 5 : 1;
                if (_progressValue > 90 && !_initDone) _progressValue = 90;
                UpdateProgressBar();
            }
        }

        private void UpdateProgressBar()
        {
            int maxWidth = _progressTrack.Width - 2;
            _progressBar.Width = Math.Max(0, (int)(maxWidth * (_progressValue / 100.0)));
        }

        // ─────────────────────────────────────────────────────────────────────
        // UI construction
        // ─────────────────────────────────────────────────────────────────────
        private void InitializeComponent()
        {
            Text            = "BussinessERP";
            FormBorderStyle = FormBorderStyle.None;
            StartPosition   = FormStartPosition.CenterScreen;
            Size            = new Size(560, 340);
            BackColor       = _bg;
            ShowInTaskbar   = false;
            TopMost         = true;
            DoubleBuffered  = true;

            // Logo circle
            var logoPanel = new Panel
            {
                Size      = new Size(80, 80),
                Location  = new Point(240, 28),
                BackColor = Color.Transparent
            };
            logoPanel.Paint += LogoPanel_Paint;

            // App name
            _lblAppName = new Label
            {
                Text      = "BussinessERP",
                Font      = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = _textWhite,
                AutoSize  = true,
                Location  = new Point(0, 120),
                BackColor = Color.Transparent
            };

            // Tagline
            _lblTagline = new Label
            {
                Text      = LanguageManager.Get("erp_subtitle"),
                Font      = new Font("Segoe UI", 10, FontStyle.Regular),
                ForeColor = _textMuted,
                AutoSize  = true,
                Location  = new Point(0, 170),
                BackColor = Color.Transparent
            };

            // Feature 1 — version (text set in OnLoad from Assembly)
            _lblVersion = new Label
            {
                Text      = "v1.0.0",
                Font      = new Font("Segoe UI", 8, FontStyle.Regular),
                ForeColor = Color.FromArgb(80, 90, 100),
                AutoSize  = true,
                Location  = new Point(0, 215), // Moved down to make room for welcome
                BackColor = Color.Transparent
            };

            // Feature 3 — welcome-back greeting (hidden until OnLoad)
            _lblWelcome = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 15, FontStyle.Italic),
                ForeColor = Color.FromArgb(0, 188, 170),
                AutoSize  = true,
                Location  = new Point(0, 190), // Moved up to the "level" of previous text
                BackColor = Color.Transparent,
                Visible   = false
            };

            // Status text
            _lblStatus = new Label
            {
                Text      = LanguageManager.Get("splash_starting"),
                Font      = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = _teal,
                AutoSize  = true,
                Location  = new Point(20, 300),
                BackColor = Color.Transparent
            };

            // Feature 2 — status icon (hidden until init done)
            _lblStatusIcon = new Label
            {
                Text      = "",
                Font      = new Font("Segoe UI", 11, FontStyle.Bold),
                AutoSize  = true,
                Location  = new Point(20, 264),
                BackColor = Color.Transparent,
                Visible   = false
            };

            // Progress track
            _progressTrack = new Panel
            {
                Size      = new Size(500, 4),
                Location  = new Point(30, 295),
                BackColor = Color.FromArgb(50, 60, 70)
            };

            // Progress fill
            _progressBar = new Panel
            {
                Size      = new Size(0, 4),
                Location  = new Point(1, 0),
                BackColor = _teal
            };
            _progressTrack.Controls.Add(_progressBar);

            // Bottom accent line
            _accentLine = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 3,
                BackColor = _teal
            };

            // Card
            var card = new Panel
            {
                Size      = new Size(500, 260),
                Location  = new Point(30, 25),
                BackColor = _bgCard
            };
            card.Paint  += CardPanel_Paint;
            card.Resize += (s, e) => CentreLabels(card, logoPanel);

            card.Controls.Add(logoPanel);
            card.Controls.Add(_lblAppName);
            card.Controls.Add(_lblTagline);
            card.Controls.Add(_lblVersion);
            card.Controls.Add(_lblWelcome);
            card.Controls.Add(_lblStatus);
            card.Controls.Add(_lblStatusIcon);

            Controls.Add(card);
            Controls.Add(_progressTrack);
            Controls.Add(_accentLine);

            Load += (s, e) => CentreLabels(card, logoPanel);
        }

        private void CentreLabels(Panel card, Panel logo)
        {
            int cx = card.Width / 2;
            logo.Left           = cx - logo.Width           / 2;
            _lblAppName.Left    = cx - _lblAppName.Width    / 2;
            _lblTagline.Left    = cx - _lblTagline.Width    / 2;
            _lblVersion.Left    = cx - _lblVersion.Width    / 2;
            _lblWelcome.Left    = cx - _lblWelcome.Width    / 2;
            // Status and icon stay left-aligned
        }

        private void LogoPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var brush = new LinearGradientBrush(
                new Rectangle(0, 0, 80, 80), _teal, _tealDark, 45f))
            {
                e.Graphics.FillEllipse(brush, 2, 2, 76, 76);
            }
            using (var f = new Font("Segoe UI", 28, FontStyle.Bold))
            using (var fmt = new StringFormat
            {
                Alignment     = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                e.Graphics.DrawString("E", f, Brushes.White,
                    new RectangleF(0, 0, 80, 80), fmt);
            }
        }

        private void CardPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var card = sender as Panel;
            using (var pen = new Pen(Color.FromArgb(55, 65, 75), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            }
        }

        private void SetupAnimationTimers()
        {
            _fadeTimer           = new Timer { Interval = 25 };
            _fadeTimer.Tick     += FadeTimer_Tick;

            _dotTimer            = new Timer { Interval = 400 };
            _dotTimer.Tick      += DotTimer_Tick;

            _progressTimer       = new Timer { Interval = 60 };
            _progressTimer.Tick += ProgressTimer_Tick;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && Opacity > 0)
                e.Cancel = true;
            base.OnFormClosing(e);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fadeTimer?.Dispose();
                _dotTimer?.Dispose();
                _progressTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
