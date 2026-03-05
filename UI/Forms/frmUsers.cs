using System;
using System.Drawing;
using System.Windows.Forms;
using BussinessErp.BLL;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.UI.Forms
{
    public class frmUsers : Form
    {
        private DataGridView dgv;
        private TextBox txtUsername, txtPassword;
        private ComboBox cmbRole;
        private Button btnAdd, btnDelete, btnResetPwd;
        private AuthService _svc = new AuthService();

        public frmUsers() { InitializeComponent(); }

        private void InitializeComponent()
        {
            this.Text = LanguageManager.Get("nav_users");
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.Padding = new Padding(20);

            LanguageManager.ApplyRTL(this);

            var lblTitle = new Label { Text = "🔐 " + LanguageManager.Get("nav_users") + " " + LanguageManager.Get("admin_only"), Font = UIHelper.FontTitle, ForeColor = UIHelper.TextDark, Dock = DockStyle.Top, Height = 45 };

            dgv = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgv);

            var panelInput = new Panel { Dock = DockStyle.Bottom, Height = 100, BackColor = UIHelper.CardBg, Padding = new Padding(15) };
            
            var tableInput = new TableLayoutPanel { Dock = DockStyle.Top, Height = 35, Width = 700, ColumnCount = 6, RowCount = 1 };
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));

            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_username"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 0, 0);
            txtUsername = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableInput.Controls.Add(txtUsername, 1, 0);

            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_password"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 2, 0);
            txtPassword = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody, PasswordChar = '●' };
            tableInput.Controls.Add(txtPassword, 3, 0);

            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_role"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 4, 0);
            cmbRole = new ComboBox { Dock = DockStyle.Top, Font = UIHelper.FontBody, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbRole.Items.AddRange(new[] { "Admin", "Manager", "Employee" });
            cmbRole.SelectedIndex = 2;
            tableInput.Controls.Add(cmbRole, 5, 0);

            var panelActions = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
            btnAdd = new Button { Text = LanguageManager.Get("btn_create_user"), Location = new Point(0, 5), Size = new Size(130, 35) };
            UIHelper.StyleButton(btnAdd, UIHelper.PrimaryTeal);
            btnDelete = new Button { Text = LanguageManager.Get("btn_delete"), Location = new Point(140, 5), Size = new Size(110, 35) };
            UIHelper.StyleButton(btnDelete, UIHelper.AccentRed);
            btnResetPwd = new Button { Text = LanguageManager.Get("btn_reset_pwd"), Location = new Point(260, 5), Size = new Size(150, 35) };
            UIHelper.StyleButton(btnResetPwd, UIHelper.AccentOrange);

            panelActions.Controls.AddRange(new Control[] { btnAdd, btnDelete, btnResetPwd });

            panelInput.Controls.Add(panelActions);
            panelInput.Controls.Add(tableInput);

            btnAdd.Click += async (s, e) => {
                bool ok = await _svc.CreateUserAsync(txtUsername.Text, txtPassword.Text, cmbRole.Text);
                if (ok) { UIHelper.ShowInfo(LanguageManager.Get("msg_user_created")); txtUsername.Clear(); txtPassword.Clear(); await LoadAsync(); }
                else UIHelper.ShowError(LanguageManager.Get("msg_user_exists"));
            };
            btnDelete.Click += async (s, e) => {
                if (dgv.CurrentRow?.DataBoundItem is User u) {
                    if (u.Username == AuthService.CurrentUser?.Username) { UIHelper.ShowWarning(LanguageManager.Get("msg_delete_self")); return; }
                    if (UIHelper.ShowConfirm(LanguageManager.Get("msg_delete_record")) == DialogResult.Yes)
                    { await _svc.DeleteUserAsync(u.Id); await LoadAsync(); }
                }
            };
            btnResetPwd.Click += async (s, e) => {
                if (dgv.CurrentRow?.DataBoundItem is User u && !string.IsNullOrEmpty(txtPassword.Text))
                {
                    bool ok = await _svc.ChangePasswordAsync(u.Id, txtPassword.Text);
                    if (ok) UIHelper.ShowInfo(string.Format(LanguageManager.Get("msg_pwd_reset"), u.Username));
                    else UIHelper.ShowError("Failed to reset password.");
                }
            };

            this.Controls.Add(dgv);
            this.Controls.Add(panelInput);
            this.Controls.Add(lblTitle);
            this.Load += async (s, e) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync() { dgv.DataSource = await _svc.GetAllUsersAsync(); }
    }
}
