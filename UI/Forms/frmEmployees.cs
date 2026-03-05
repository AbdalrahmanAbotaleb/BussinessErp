using System;
using System.Drawing;
using System.Windows.Forms;
using BussinessErp.BLL;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.UI.Forms
{
    public class frmEmployees : Form
    {
        private DataGridView dgv;
        private TextBox txtSearch, txtName, txtPosition, txtSalary, txtPhone;
        private DateTimePicker dtpHireDate;
        private Button btnAdd, btnUpdate, btnDelete, btnClear;
        private EmployeeService _svc = new EmployeeService();
        private int _selectedId = 0;

        public frmEmployees() { InitializeComponent(); }

        private void InitializeComponent()
        {
            this.Text = LanguageManager.Get("nav_employees");
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.Padding = new Padding(20);

            LanguageManager.ApplyRTL(this);

            var lblTitle = new Label { Text = "👥 " + LanguageManager.Get("nav_employees"), Font = UIHelper.FontTitle, ForeColor = UIHelper.TextDark, Dock = DockStyle.Top, Height = 45 };

            // Search bar
            var panelSearch = new Panel { Dock = DockStyle.Top, Height = 45, Padding = new Padding(0, 5, 0, 5) };
            txtSearch = new TextBox { Width = 300, Dock = DockStyle.Left, Font = UIHelper.FontBody };
            
            // Live Search Event with Debouncing
            var debouncedSearch = UIHelper.Debounce(async () => {
                dgv.DataSource = string.IsNullOrEmpty(txtSearch.Text) ? await _svc.GetAllAsync() : await _svc.SearchAsync(txtSearch.Text);
            }, 300);
            txtSearch.TextChanged += (s, e) => debouncedSearch();

            panelSearch.Controls.Add(txtSearch);

            // Grid
            dgv = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgv);
            dgv.SelectionChanged += (s, e) => {
                if (dgv.CurrentRow?.DataBoundItem is Employee emp)
                {
                    _selectedId = emp.Id;
                    txtName.Text = emp.Name;
                    txtPosition.Text = emp.Position;
                    txtSalary.Text = emp.Salary.ToString("F2");
                    txtPhone.Text = emp.Phone;
                    dtpHireDate.Value = emp.HireDate >= dtpHireDate.MinDate ? emp.HireDate : DateTime.Now;
                }
            };

            // Input panel
            var panelInput = new Panel { Dock = DockStyle.Bottom, Height = 190, BackColor = UIHelper.CardBg, Padding = new Padding(15) };
            
            var tableInput = new TableLayoutPanel { Dock = DockStyle.Left, Width = 600, ColumnCount = 4, RowCount = 3 };
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // Row 1
            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_name"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody, ForeColor = UIHelper.TextDark }, 0, 0);
            txtName = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableInput.Controls.Add(txtName, 1, 0);

            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_salary"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody, ForeColor = UIHelper.TextDark }, 2, 0);
            txtSalary = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableInput.Controls.Add(txtSalary, 3, 0);

            // Row 2
            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_position"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody, ForeColor = UIHelper.TextDark }, 0, 1);
            txtPosition = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableInput.Controls.Add(txtPosition, 1, 1);

            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_phone"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody, ForeColor = UIHelper.TextDark }, 2, 1);
            txtPhone = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableInput.Controls.Add(txtPhone, 3, 1);

            // Row 3
            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_hire_date"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody, ForeColor = UIHelper.TextDark }, 0, 2);
            dtpHireDate = new DateTimePicker { Dock = DockStyle.Top, Width = 200 };
            tableInput.Controls.Add(dtpHireDate, 1, 2);

            // Buttons
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

            // Events
            btnAdd.Click += async (s, e) => {
                var emp = new Employee { Name = txtName.Text, Position = txtPosition.Text, Salary = decimal.TryParse(txtSalary.Text, out var sal) ? sal : 0, Phone = txtPhone.Text, HireDate = dtpHireDate.Value };
                var (ok, err) = await _svc.AddAsync(emp);
                if (ok) { ClearInputs(); await LoadDataAsync(); } else UIHelper.ShowError(err);
            };
            btnUpdate.Click += async (s, e) => {
                if (_selectedId == 0) return;
                var emp = new Employee { Id = _selectedId, Name = txtName.Text, Position = txtPosition.Text, Salary = decimal.TryParse(txtSalary.Text, out var sal) ? sal : 0, Phone = txtPhone.Text, HireDate = dtpHireDate.Value };
                var (ok, err) = await _svc.UpdateAsync(emp);
                if (ok) { ClearInputs(); await LoadDataAsync(); } else UIHelper.ShowError(err);
            };
            btnDelete.Click += async (s, e) => {
                if (_selectedId == 0) return;
                if (UIHelper.ShowConfirm(LanguageManager.Get("msg_delete_record")) == DialogResult.Yes)
                { await _svc.DeleteAsync(_selectedId); ClearInputs(); await LoadDataAsync(); }
            };
            btnClear.Click += (s, e) => ClearInputs();

            this.Controls.Add(dgv);
            this.Controls.Add(panelInput);
            this.Controls.Add(panelSearch);
            this.Controls.Add(lblTitle);

            this.Load += async (s, e) => {
                await LoadDataAsync();

                // RBAC UI Enforcement
                btnAdd.Enabled = AuthService.IsAdmin;
                btnUpdate.Enabled = AuthService.IsAdmin;
                btnDelete.Enabled = AuthService.IsAdmin;
            };
        }

        private async System.Threading.Tasks.Task LoadDataAsync() { dgv.DataSource = await _svc.GetAllAsync(); }
        private void ClearInputs() { _selectedId = 0; txtName.Clear(); txtPosition.Clear(); txtSalary.Clear(); txtPhone.Clear(); dtpHireDate.Value = DateTime.Now; }
    }
}
