using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BussinessErp.BLL;
using BussinessErp.Helpers;
using BussinessErp.Properties;

namespace BussinessErp.UI.Forms
{
    public class frmLogin : Form
    {
        private TextBox txtUsername, txtPassword;
        private Button btnLogin, btnTogglePassword;
        private Label lblTitle, lblSubtitle, lblError;
        private Panel panelCard;

        public frmLogin()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = LanguageManager.Get("app_name") + " — " + LanguageManager.Get("login");
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new Size(450, 500);
            this.BackColor = UIHelper.DarkBg;
            
            LanguageManager.ApplyRTL(this);

            // Card panel
            panelCard = new Panel
            {
                Size = new Size(380, 400),
                Location = new Point(35, 50),
                BackColor = UIHelper.CardBg,
                Padding = new Padding(30)
            };

            // Title
            lblTitle = new Label
            {
                Text = "🧠 " + LanguageManager.Get("app_name"),
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = UIHelper.PrimaryTeal,
                AutoSize = false,
                Size = new Size(320, 45),
                Location = new Point(30, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            lblSubtitle = new Label
            {
                Text = LanguageManager.Get("erp_subtitle"),
                Font = UIHelper.FontSmall,
                ForeColor = UIHelper.TextMuted,
                AutoSize = false,
                Size = new Size(320, 25),
                Location = new Point(30, 68),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Username
            var lblUser = new Label
            {
                Text = LanguageManager.Get("username"),
                Font = UIHelper.FontBodyBold,
                ForeColor = UIHelper.TextDark,
                Location = new Point(30, 120),
                AutoSize = true
            };

            txtUsername = new TextBox
            {
                Font = new Font("Segoe UI", 12F),
                Location = new Point(30, 145),
                Size = new Size(320, 35),
                BorderStyle = BorderStyle.FixedSingle
            };

            // Password
            var lblPass = new Label
            {
                Text = LanguageManager.Get("password"),
                Font = UIHelper.FontBodyBold,
                ForeColor = UIHelper.TextDark,
                Location = new Point(30, 195),
                AutoSize = true
            };

            txtPassword = new TextBox
            {
                Font = new Font("Segoe UI", 12F),
                Location = new Point(30, 220),
                Size = new Size(320, 35),
                BorderStyle = BorderStyle.FixedSingle,
                PasswordChar = '●'
            };

            btnTogglePassword = new Button
            {
                Text = "👁️",
                Size = new Size(30, 30),
                Location = LanguageManager.IsArabic ? new Point(5, 2) : new Point(txtPassword.Width - 35, 2),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = UIHelper.TextMuted,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand
            };
            btnTogglePassword.FlatAppearance.BorderSize = 0;
            txtPassword.Controls.Add(btnTogglePassword);

            btnTogglePassword.Click += (s, e) => {
                if (txtPassword.PasswordChar == '●')
                {
                    txtPassword.PasswordChar = '\0';
                    btnTogglePassword.Text = "🙈";
                }
                else
                {
                    txtPassword.PasswordChar = '●';
                    btnTogglePassword.Text = "👁️";
                }
            };

            // Error label
            lblError = new Label
            {
                Text = "",
                Font = UIHelper.FontSmall,
                ForeColor = UIHelper.AccentRed,
                Location = new Point(30, 265),
                AutoSize = false,
                Size = new Size(320, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Login button
            btnLogin = new Button
            {
                Text = LanguageManager.Get("login"),
                Size = new Size(320, 45),
                Location = new Point(30, 300),
                FlatStyle = FlatStyle.Flat,
                BackColor = UIHelper.PrimaryTeal,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;

            // Close button
            var btnClose = new Button
            {
                Text = "✕",
                Size = new Size(30, 30),
                Location = new Point(340, 5),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = UIHelper.TextMuted,
                Font = new Font("Segoe UI", 10F),
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Application.Exit();

            // Version label
            var lblVersion = new Label
            {
                Text = "v1.0 — Default: admin / admin123",
                Font = new Font("Segoe UI", 8F),
                ForeColor = UIHelper.TextMuted,
                Location = new Point(30, 360),
                AutoSize = false,
                Size = new Size(320, 20),
                TextAlign = ContentAlignment.MiddleCenter
            };

            panelCard.Controls.AddRange(new Control[] {
                lblTitle, lblSubtitle, lblUser, txtUsername, lblPass, txtPassword,
                lblError, btnLogin, lblVersion
            });
            this.Controls.Add(panelCard);
            this.Controls.Add(btnClose);

            txtUsername.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) txtPassword.Focus(); };
            txtPassword.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnLogin_Click(s, e); };

            this.ActiveControl = txtUsername;
        }

        private async void BtnLogin_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            btnLogin.Enabled = false;
            btnLogin.Text = LanguageManager.Get("logging_in");


            try
            {
                // Wait for background initialization to finish if it's not ready yet
                int retryCount = 0;
                while (!DatabaseHelper.IsInitialized && retryCount < 10)
                {
                    btnLogin.Text = LanguageManager.Get("initializing_db");
                    await Task.Delay(500);
                    retryCount++;
                }

                var authService = new AuthService();
                var user = await authService.LoginAsync(txtUsername.Text.Trim(), txtPassword.Text);

                if (user != null)
                {
                    // Persist last username for welcome-back greeting on next splash
                    Settings.Default.LastUsername = txtUsername.Text.Trim();
                    Settings.Default.Save();

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    lblError.Text = LanguageManager.Get("invalid_login");
                }
            }
            catch (Exception ex)
            {
                lblError.Text = LanguageManager.Get("connection_error");
                AppLogger.Error("Login failed", ex);
            }
            finally
            {
                btnLogin.Enabled = true;
                btnLogin.Text = LanguageManager.Get("login");
            }
        }
    }
}
