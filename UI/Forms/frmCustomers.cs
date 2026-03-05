using System;
using System.Drawing;
using System.Windows.Forms;
using BussinessErp.BLL;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.UI.Forms
{
    public class frmCustomers : Form
    {
        private DataGridView dgv;
        private TextBox txtSearch, txtName, txtPhone, txtEmail, txtAddress;
        private Button btnAdd, btnUpdate, btnDelete, btnClear;
        private CustomerService _svc = new CustomerService();
        private int _selectedId = 0;

        public frmCustomers() { InitializeComponent(); }

        private void InitializeComponent()
        {
            this.Text = LanguageManager.Get("nav_customers");
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.Padding = new Padding(20);

            LanguageManager.ApplyRTL(this);

            var lblTitle = new Label { Text = "🤝 " + LanguageManager.Get("nav_customers"), Font = UIHelper.FontTitle, ForeColor = UIHelper.TextDark, Dock = DockStyle.Top, Height = 45 };

            var panelSearch = new Panel { Dock = DockStyle.Top, Height = 45, Padding = new Padding(0, 5, 0, 5) };
            txtSearch = new TextBox { Width = 300, Dock = DockStyle.Left, Font = UIHelper.FontBody };
            
            // Live Search Event with Debouncing
            var debouncedSearch = UIHelper.Debounce(async () => {
                dgv.DataSource = string.IsNullOrEmpty(txtSearch.Text) ? await _svc.GetAllAsync() : await _svc.SearchAsync(txtSearch.Text);
            }, 300);
            txtSearch.TextChanged += (s, e) => debouncedSearch();
            
            panelSearch.Controls.Add(txtSearch);

            dgv = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgv);
            dgv.SelectionChanged += (s, e) => {
                if (dgv.CurrentRow?.DataBoundItem is Customer c)
                { _selectedId = c.Id; txtName.Text = c.Name; txtPhone.Text = c.Phone; txtEmail.Text = c.Email; txtAddress.Text = c.Address; }
            };

            var panelInput = new Panel { Dock = DockStyle.Bottom, Height = 140, BackColor = UIHelper.CardBg, Padding = new Padding(15) };
            
            var tableInput = new TableLayoutPanel { Dock = DockStyle.Left, Width = 600, ColumnCount = 4, RowCount = 2 };
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // Row 1
            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_name"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 0, 0);
            txtName = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableInput.Controls.Add(txtName, 1, 0);

            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_email"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 2, 0);
            txtEmail = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableInput.Controls.Add(txtEmail, 3, 0);

            // Row 2
            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_phone"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 0, 1);
            txtPhone = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableInput.Controls.Add(txtPhone, 1, 1);

            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_address"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 2, 1);
            txtAddress = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableInput.Controls.Add(txtAddress, 3, 1);

            var panelActions = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 0, 0, 0) };
            btnAdd = new Button { Text = LanguageManager.Get("btn_add"), Location = new Point(10, 12), Size = new Size(100, 35) };
            UIHelper.StyleButton(btnAdd, UIHelper.PrimaryTeal);
            btnUpdate = new Button { Text = LanguageManager.Get("btn_update"), Location = new Point(120, 12), Size = new Size(100, 35) };
            UIHelper.StyleButton(btnUpdate, UIHelper.AccentBlue);
            btnDelete = new Button { Text = LanguageManager.Get("btn_delete"), Location = new Point(10, 67), Size = new Size(100, 35) };
            UIHelper.StyleButton(btnDelete, UIHelper.AccentRed);
            btnClear = new Button { Text = LanguageManager.Get("btn_refresh"), Location = new Point(120, 67), Size = new Size(100, 35) };
            UIHelper.StyleButton(btnClear, UIHelper.TextMuted);

            panelActions.Controls.AddRange(new Control[] { btnAdd, btnUpdate, btnDelete, btnClear });

            panelInput.Controls.Add(panelActions);
            panelInput.Controls.Add(tableInput);

            btnAdd.Click += async (s, e) => {
                var c = new Customer { Name = txtName.Text, Phone = txtPhone.Text, Email = txtEmail.Text, Address = txtAddress.Text };
                var (ok, err) = await _svc.AddAsync(c);
                if (ok) { ClearInputs(); await LoadAsync(); } else UIHelper.ShowError(err);
            };
            btnUpdate.Click += async (s, e) => {
                if (_selectedId == 0) return;
                var c = new Customer { Id = _selectedId, Name = txtName.Text, Phone = txtPhone.Text, Email = txtEmail.Text, Address = txtAddress.Text };
                var (ok, err) = await _svc.UpdateAsync(c);
                if (ok) { ClearInputs(); await LoadAsync(); } else UIHelper.ShowError(err);
            };
            btnDelete.Click += async (s, e) => {
                if (_selectedId == 0) return;
                if (UIHelper.ShowConfirm(LanguageManager.Get("msg_delete_record")) == DialogResult.Yes)
                { await _svc.DeleteAsync(_selectedId); ClearInputs(); await LoadAsync(); }
            };
            btnClear.Click += (s, e) => ClearInputs();

            this.Controls.Add(dgv);
            this.Controls.Add(panelInput);
            this.Controls.Add(panelSearch);
            this.Controls.Add(lblTitle);

            this.Load += async (s, e) => {
                await LoadAsync();

                // RBAC UI Enforcement
                btnDelete.Enabled = AuthService.IsAdmin;
            };
        }

        private async System.Threading.Tasks.Task LoadAsync() { dgv.DataSource = await _svc.GetAllAsync(); }
        private void ClearInputs() { _selectedId = 0; txtName.Clear(); txtPhone.Clear(); txtEmail.Clear(); txtAddress.Clear(); }
    }
}
