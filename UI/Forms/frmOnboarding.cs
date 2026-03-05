using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BussinessErp.Helpers;
using BussinessErp.Properties;

namespace BussinessErp.UI.Forms
{
    public sealed class frmOnboarding : Form
    {
        // ── Constants & Theme ────────────────────────────────────────────────
        private static readonly Color _bg = Color.FromArgb(28, 32, 38);
        private static readonly Color _cardBg = Color.FromArgb(38, 44, 52);
        private static readonly Color _teal = Color.FromArgb(0, 150, 136);
        private static readonly Color _textWhite = Color.FromArgb(245, 246, 248);
        private static readonly Color _textMuted = Color.FromArgb(140, 150, 160);

        // ── UI Controls ─────────────────────────────────────────────────────
        private Panel _contentPanel;
        private Panel[] _pages;
        private Panel _dotsPanel;
        private Button _btnNext;
        private Button _btnBack;
        private Button _btnSkip;
        private Button _btnGetStarted;

        // ── State ──────────────────────────────────────────────────────────
        private int _currentPage = 0;
        private Timer _slideTimer;
        private int _targetX = 0;
        private const int SLIDE_SPEED = 40;

        public frmOnboarding()
        {
            InitializeComponent();
            LanguageManager.ApplyRTL(this);
            SetupPages();
            UpdateNavigation();
        }

        private void InitializeComponent()
        {
            Text = LanguageManager.Get("onboard_welcome");
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(800, 600);
            BackColor = _bg;
            DoubleBuffered = true;

            // Container for clipping
            var container = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            // Main sliding panel
            _contentPanel = new Panel
            {
                Width = 800 * 4,
                Height = 600,
                Location = new Point(0, 0),
                BackColor = Color.Transparent
            };
            container.Controls.Add(_contentPanel);

            // Dots indicator
            _dotsPanel = new Panel
            {
                Size = new Size(100, 30),
                Location = new Point(350, 540),
                BackColor = Color.Transparent
            };
            _dotsPanel.Paint += DotsPanel_Paint;

            // Navigation Buttons
            _btnBack = new Button { Text = LanguageManager.Get("onboard_back"), Size = new Size(100, 40), Location = new Point(50, 530) };
            _btnNext = new Button { Text = LanguageManager.Get("onboard_next"), Size = new Size(100, 40), Location = new Point(650, 530) };
            _btnSkip = new Button { Text = LanguageManager.Get("onboard_skip"), Size = new Size(80, 30), Location = new Point(710, 10), FlatStyle = FlatStyle.Flat };
            _btnGetStarted = new Button { Text = LanguageManager.Get("onboard_get_started"), Size = new Size(150, 45), Location = new Point(600, 527), Visible = false };

            UIHelper.StyleButton(_btnBack, Color.FromArgb(100, 38, 44, 52), _textWhite);
            UIHelper.StyleButton(_btnNext, Color.FromArgb(180, 0, 150, 136), _textWhite);
            UIHelper.StyleButton(_btnGetStarted, Color.FromArgb(200, 0, 150, 136), _textWhite);
            
            _btnSkip.ForeColor = _textMuted;
            _btnSkip.FlatAppearance.BorderSize = 0;
            _btnSkip.BackColor = Color.Transparent;
            _btnSkip.Font = UIHelper.FontSmall;

            _btnBack.Click += (s, e) => MovePage(-1);
            _btnNext.Click += (s, e) => MovePage(1);
            _btnSkip.Click += (s, e) => FinishOnboarding();
            _btnGetStarted.Click += (s, e) => FinishOnboarding();

            // Add controls in order (Container at back, buttons at front)
            Controls.Add(_dotsPanel);
            Controls.Add(_btnBack);
            Controls.Add(_btnNext);
            Controls.Add(_btnSkip);
            Controls.Add(_btnGetStarted);
            Controls.Add(container);

            _btnBack.BringToFront();
            _btnNext.BringToFront();
            _btnSkip.BringToFront();
            _btnGetStarted.BringToFront();
            _dotsPanel.BringToFront();

            _slideTimer = new Timer { Interval = 10 };
            _slideTimer.Tick += SlideTimer_Tick;

            // Rounded corners for the form
            Region = Region.FromHrgn(NativeMethods.CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
        }

        private void SetupPages()
        {
            _pages = new Panel[4];
            
            _pages[0] = CreatePage(LanguageManager.Get("onboard_ai_title"), 
                LanguageManager.Get("onboard_ai_desc"),
                @"docs\onboarding_ai_brain.png");

            _pages[1] = CreatePage(LanguageManager.Get("onboard_sales_title"), 
                LanguageManager.Get("onboard_sales_desc"),
                @"docs\onboarding_sales.png");

            _pages[2] = CreatePage(LanguageManager.Get("onboard_inventory_title"), 
                LanguageManager.Get("onboard_inventory_desc"),
                @"docs\onboarding_inventory.png");

            _pages[3] = CreatePage(LanguageManager.Get("onboard_reporting_title"), 
                LanguageManager.Get("onboard_reporting_desc"),
                @"docs\onboarding_reporting.png");

            for (int i = 0; i < 4; i++)
            {
                _pages[i].Location = new Point(i * 800, 0);
                _contentPanel.Controls.Add(_pages[i]);
            }
        }

        private Panel CreatePage(string title, string description, string imgFileName)
        {
            var p = new Panel { Size = new Size(800, 600), BackColor = Color.Transparent };

            var lblTitle = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = _textWhite,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(700, 50),
                Location = new Point(50, 30)
            };

            var lblDesc = new Label
            {
                Text = description,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ForeColor = _textMuted,
                TextAlign = ContentAlignment.MiddleCenter,
                Size = new Size(640, 60),
                Location = new Point(80, 85)
            };

            var pb = new PictureBox
            {
                Size = new Size(420, 420), // Increased from original 300x300
                Location = new Point(190, 140),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent
            };

            // Load image with path resolution
            try 
            {
                string basePath = AppDomain.CurrentDomain.BaseDirectory;
                string fullPath = Path.Combine(basePath, imgFileName);

                if (!File.Exists(fullPath))
                    fullPath = Path.Combine(basePath, "..", "..", imgFileName);
                
                if (File.Exists(fullPath))
                {
                    pb.Image = Image.FromFile(fullPath);
                }
            } 
            catch (Exception ex) 
            {
                AppLogger.Error("Failed to load onboarding image: " + imgFileName, ex);
            }

            p.Controls.Add(lblTitle);
            p.Controls.Add(lblDesc);
            p.Controls.Add(pb);

            return p;
        }

        private void MovePage(int direction)
        {
            int next = _currentPage + direction;
            if (next >= 0 && next < 4)
            {
                _currentPage = next;
                _targetX = -(_currentPage * 800);
                _slideTimer.Start();
                UpdateNavigation();
                _dotsPanel.Invalidate();
            }
        }

        private void SlideTimer_Tick(object sender, EventArgs e)
        {
            int currentX = _contentPanel.Left;
            if (Math.Abs(currentX - _targetX) < SLIDE_SPEED)
            {
                _contentPanel.Left = _targetX;
                _slideTimer.Stop();
            }
            else
            {
                if (currentX > _targetX) _contentPanel.Left -= SLIDE_SPEED;
                else _contentPanel.Left += SLIDE_SPEED;
            }
        }

        private void UpdateNavigation()
        {
            _btnBack.Visible = _currentPage > 0;
            _btnNext.Visible = _currentPage < 3;
            _btnGetStarted.Visible = _currentPage == 3;
        }

        private void DotsPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            int dotSize = 10;
            int spacing = 20;
            int startX = (_dotsPanel.Width - (4 * dotSize + 3 * spacing)) / 2;

            for (int i = 0; i < 4; i++)
            {
                var brush = i == _currentPage ? new SolidBrush(_teal) : new SolidBrush(Color.FromArgb(80, 90, 100));
                e.Graphics.FillEllipse(brush, startX + i * (dotSize + spacing), 10, dotSize, dotSize);
            }
        }

        private void FinishOnboarding()
        {
            Settings.Default.IsFirstLaunch = false;
            Settings.Default.Save();
            DialogResult = DialogResult.OK;
            Close();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // Subtle border
            using (var pen = new Pen(Color.FromArgb(50, 60, 70), 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }
    }

    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        public static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);
    }
}
