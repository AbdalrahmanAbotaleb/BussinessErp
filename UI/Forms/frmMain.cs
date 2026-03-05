using System;
using System.Drawing;
using System.Windows.Forms;
using BussinessErp.BLL;
using BussinessErp.Helpers;

namespace BussinessErp.UI.Forms
{
    public class frmMain : Form
    {
        private Panel panelSidebar, panelTopBar, panelContent;
        private Label lblTitle, lblUser, lblRole;
        private Button btnDashboard, btnEmployees, btnCustomers, btnSuppliers,
                       btnProducts, btnSales, btnPurchases, btnExpenses,
                       btnUsers, btnFinancials, btnAIBrain, btnChatbot, btnBackupRestore, btnLogout, btnLang;

        public frmMain()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "AI Business Brain — ERP System";
            this.Size = new Size(1280, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.IsMdiContainer = true;
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.WindowState = FormWindowState.Maximized;

            // ===== SIDEBAR =====
            panelSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 240,
                BackColor = UIHelper.DarkSidebar,
                Padding = new Padding(0)
            };

            // Logo area
            var panelLogo = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = UIHelper.DarkBg,
                Padding = new Padding(15)
            };

            lblTitle = new Label
            {
                Text = "🧠 " + LanguageManager.Get("app_name"),
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = UIHelper.PrimaryTeal,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            panelLogo.Controls.Add(lblTitle);

            // Nav buttons
            int y = 0;
            btnDashboard = CreateNavButton(LanguageManager.Get("nav_dashboard"), y); btnDashboard.Tag = "frmDashboard"; y += 44;
            btnEmployees = CreateNavButton(LanguageManager.Get("nav_employees"), y); btnEmployees.Tag = "frmEmployees"; y += 44;
            btnCustomers = CreateNavButton(LanguageManager.Get("nav_customers"), y); btnCustomers.Tag = "frmCustomers"; y += 44;
            btnSuppliers = CreateNavButton(LanguageManager.Get("nav_suppliers"), y); btnSuppliers.Tag = "frmSuppliers"; y += 44;
            btnProducts = CreateNavButton(LanguageManager.Get("nav_products"), y); btnProducts.Tag = "frmProducts"; y += 44;
            btnSales = CreateNavButton(LanguageManager.Get("nav_sales"), y); btnSales.Tag = "frmSales"; y += 44;
            btnPurchases = CreateNavButton(LanguageManager.Get("nav_purchases"), y); btnPurchases.Tag = "frmPurchases"; y += 44;
            btnExpenses = CreateNavButton(LanguageManager.Get("nav_expenses"), y); btnExpenses.Tag = "frmExpenses"; y += 44;

            // Separator
            var separator = new Panel
            {
                BackColor = Color.FromArgb(60, 65, 75),
                Size = new Size(200, 1),
                Location = new Point(20, y)
            };
            y += 10;

            btnAIBrain = CreateNavButton(LanguageManager.Get("nav_ai_brain"), y); btnAIBrain.Tag = "frmAIBrain"; y += 44;
            btnAIBrain.ForeColor = UIHelper.PrimaryTealLight;

            btnChatbot = CreateNavButton(LanguageManager.Get("nav_chatbot"), y); btnChatbot.Tag = "frmChatbot"; y += 44;
            btnChatbot.ForeColor = Color.FromArgb(0, 150, 136);

            btnUsers = CreateNavButton(LanguageManager.Get("nav_users"), y); btnUsers.Tag = "frmUsers"; y += 44;

            btnFinancials = CreateNavButton(LanguageManager.Get("nav_financials"), y); btnFinancials.Tag = "frmFinancialReports"; y += 44;
            btnFinancials.ForeColor = Color.Gold;

            btnBackupRestore = CreateNavButton(LanguageManager.Get("nav_backup_restore"), y); btnBackupRestore.Tag = "frmBackupRestore"; y += 44;
            btnBackupRestore.ForeColor = UIHelper.AccentOrange;

            // Nav panel
            var panelNav = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIHelper.DarkSidebar,
                Padding = new Padding(0, 10, 0, 0),
                AutoScroll = true
            };

            panelNav.Controls.AddRange(new Control[] {
                btnBackupRestore, btnFinancials, btnUsers, btnChatbot, btnAIBrain, separator,
                btnExpenses, btnPurchases, btnSales, btnProducts,
                btnSuppliers, btnCustomers, btnEmployees, btnDashboard
            });

            // Logout at bottom
            btnLogout = new Button
            {
                Text = LanguageManager.Get("logout"),
                Dock = DockStyle.Bottom,
                Height = 44,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(180, 40, 40),
                ForeColor = UIHelper.TextWhite,
                Font = UIHelper.FontSidebar,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btnLogout.FlatAppearance.BorderSize = 0;

            panelSidebar.Controls.Add(panelNav);
            panelSidebar.Controls.Add(btnLogout);
            panelSidebar.Controls.Add(panelLogo);

            // ===== TOP BAR =====
            panelTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = UIHelper.CardBg,
                Padding = new Padding(15, 0, 15, 0)
            };

            var topDivider = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = UIHelper.CardBorder };

            lblUser = new Label
            {
                Text = $"{LanguageManager.Get("welcome")}, {AuthService.CurrentUser?.Username ?? "User"}",
                Font = UIHelper.FontBodyBold,
                ForeColor = UIHelper.TextDark,
                Dock = DockStyle.Left,
                AutoSize = false,
                Width = 300,
                TextAlign = ContentAlignment.MiddleLeft
            };

            lblRole = new Label
            {
                Text = $"{LanguageManager.Get("role")}: {AuthService.CurrentRole}",
                Font = UIHelper.FontBody,
                ForeColor = UIHelper.PrimaryTeal,
                Dock = DockStyle.Right,
                AutoSize = false,
                Width = 200,
                TextAlign = ContentAlignment.MiddleRight
            };

            // Language toggle button
            btnLang = new Button
            {
                Text = LanguageManager.Get("language_toggle"),
                Dock = DockStyle.Right,
                Width = 100,
                FlatStyle = FlatStyle.Flat,
                BackColor = UIHelper.CardBg,
                ForeColor = UIHelper.PrimaryTeal,
                Font = UIHelper.FontBodyBold,
                Cursor = Cursors.Hand
            };
            btnLang.FlatAppearance.BorderColor = UIHelper.PrimaryTeal;
            btnLang.FlatAppearance.BorderSize = 1;
            btnLang.Click += (s, e) =>
            {
                string newLang = LanguageManager.IsArabic ? "en" : "ar";
                LanguageManager.SetLanguage(newLang);
                UIHelper.ShowInfo(LanguageManager.IsArabic
                    ? "تم تغيير اللغة إلى العربية. أعد تشغيل التطبيق لتطبيق التغييرات بالكامل."
                    : "Language changed to English. Restart the app to apply changes fully.");
            };

            panelTopBar.Controls.AddRange(new Control[] { lblRole, btnLang, lblUser, topDivider });

            // ===== CONTENT AREA =====
            panelContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(243, 244, 246),
                Padding = new Padding(0)
            };
            
            this.Controls.Add(panelContent);
            this.Controls.Add(panelTopBar);
            this.Controls.Add(panelSidebar);

            // ===== EVENT WIRING =====
            btnDashboard.Click += (s, e) => OpenChildForm(new frmDashboard());
            btnEmployees.Click += (s, e) => OpenChildForm(new frmEmployees());
            btnCustomers.Click += (s, e) => OpenChildForm(new frmCustomers());
            btnSuppliers.Click += (s, e) => OpenChildForm(new frmSuppliers());
            btnProducts.Click += (s, e) => OpenChildForm(new frmProducts());
            btnSales.Click += (s, e) => OpenChildForm(new frmSales());
            btnPurchases.Click += (s, e) => OpenChildForm(new frmPurchases());
            btnExpenses.Click += (s, e) => OpenChildForm(new frmExpenses());
            btnUsers.Click += (s, e) => {
                if (!AuthService.IsAdmin) { UIHelper.ShowWarning(LanguageManager.Get("admin_required")); return; }
                OpenChildForm(new frmUsers());
            };
            btnAIBrain.Click += (s, e) => OpenChildForm(new frmAIBrain());
            btnChatbot.Click += (s, e) => OpenChildForm(new frmChatbot());
            btnFinancials.Click += (s, e) => OpenChildForm(new frmFinancialReports());
            btnBackupRestore.Click += (s, e) => {
                if (!AuthService.IsAdmin) { UIHelper.ShowWarning(LanguageManager.Get("admin_required")); return; }
                OpenChildForm(new frmBackupRestore());
            };
            btnLogout.Click += (s, e) => {
                if (UIHelper.ShowConfirm(LanguageManager.Get("logout_confirm")) == DialogResult.Yes)
                {
                    AuthService.Logout();
                    this.DialogResult = DialogResult.Retry;
                    this.Close();
                }
            };

            // Role-based visibility
            btnUsers.Visible = AuthService.CurrentRole == "Admin";
            btnExpenses.Visible = AuthService.CurrentRole == "Admin" || AuthService.CurrentRole == "Manager";
            btnFinancials.Visible = AuthService.CurrentRole == "Admin" || AuthService.CurrentRole == "Manager";
            btnEmployees.Visible = AuthService.CurrentRole == "Admin";
            btnBackupRestore.Visible = AuthService.CurrentRole == "Admin";

            // Apply RTL if Arabic
            LanguageManager.ApplyRTL(this);

            // Open dashboard by default
            this.Load += (s, e) => OpenChildForm(new frmDashboard());
        }

        private Button _activeButton;

        private Button CreateNavButton(string text, int top)
        {
            var btn = new Button
            {
                Text = text,
                Dock = DockStyle.Top,
                Height = 44,
                FlatStyle = FlatStyle.Flat,
                BackColor = UIHelper.DarkSidebar,
                ForeColor = UIHelper.TextWhite,
                Font = UIHelper.FontSidebar,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(20, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.MouseEnter += (s, e) => {
                if (btn != _activeButton) btn.BackColor = Color.FromArgb(55, 60, 70);
            };
            btn.MouseLeave += (s, e) => {
                if (btn != _activeButton) btn.BackColor = UIHelper.DarkSidebar;
            };
            return btn;
        }

        private Form _activeForm;

        private void OpenChildForm(Form childForm)
        {
            _activeForm?.Close();

            // Highlight active nav button
            if (_activeButton != null) _activeButton.BackColor = UIHelper.DarkSidebar;

            // Find matching button using Tag (more robust than text matching)
            string formType = childForm.GetType().Name;
            foreach (Control c in panelSidebar.Controls)
            {
                if (c is Panel navPanel)
                {
                    foreach (Control nc in navPanel.Controls)
                    {
                        if (nc is Button btn && btn.Tag != null && btn.Tag.ToString() == formType)
                        {
                            _activeButton = btn;
                            btn.BackColor = UIHelper.PrimaryTealDark;
                        }
                    }
                }
            }

            _activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;
            panelContent.Controls.Add(childForm);
            panelContent.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }
    }
}
