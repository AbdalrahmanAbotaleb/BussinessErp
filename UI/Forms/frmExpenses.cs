using System;
using System.Drawing;
using System.Windows.Forms;
using BussinessErp.BLL;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.UI.Forms
{
    public class frmExpenses : Form
    {
        private DataGridView dgv;
        private TextBox txtTitle, txtAmount, txtDescription;
        private ComboBox cmbCategory;
        private DateTimePicker dtpDate;
        private Button btnAdd, btnUpdate, btnDelete, btnClear;
        private DateTimePicker dtpFrom, dtpTo;
        private Button btnFilter;
        private ExpenseService _svc = new ExpenseService();
        private int _selectedId = 0;

        public frmExpenses() { InitializeComponent(); }

        private void InitializeComponent()
        {
            this.Text = LanguageManager.Get("nav_expenses");
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.Padding = new Padding(20);

            LanguageManager.ApplyRTL(this);

            var lblTitle = new Label { Text = "💳 " + LanguageManager.Get("nav_expenses"), Font = UIHelper.FontTitle, ForeColor = UIHelper.TextDark, Dock = DockStyle.Top, Height = 45 };

            // Filter bar
            var panelFilter = new Panel { Dock = DockStyle.Top, Height = 45, Padding = new Padding(0, 5, 0, 5) };
            panelFilter.Controls.Add(new Label { Text = LanguageManager.Get("from"), Dock = DockStyle.Left, Font = UIHelper.FontBody, AutoSize = false, Width = 45, TextAlign = ContentAlignment.MiddleLeft });
            dtpFrom = new DateTimePicker { Dock = DockStyle.Left, Width = 140, Value = DateTime.Now.AddMonths(-1) };
            panelFilter.Controls.Add(dtpFrom);
            panelFilter.Controls.Add(new Label { Text = " " + LanguageManager.Get("to"), Dock = DockStyle.Left, Font = UIHelper.FontBody, AutoSize = false, Width = 35, TextAlign = ContentAlignment.MiddleLeft });
            dtpTo = new DateTimePicker { Dock = DockStyle.Left, Width = 140 };
            panelFilter.Controls.Add(dtpTo);
            btnFilter = new Button { Text = LanguageManager.Get("btn_filter"), Dock = DockStyle.Left, Width = 80 };
            UIHelper.StyleButton(btnFilter, UIHelper.AccentBlue);
            panelFilter.Controls.Add(btnFilter);
            btnFilter.Click += async (s, e) => { dgv.DataSource = await _svc.GetByDateRangeAsync(dtpFrom.Value, dtpTo.Value); };

            dgv = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgv);
            dgv.SelectionChanged += (s, e) => {
                if (dgv.CurrentRow?.DataBoundItem is Expense exp)
                { _selectedId = exp.Id; txtTitle.Text = exp.Title; txtAmount.Text = exp.Amount.ToString("F2"); cmbCategory.Text = exp.Category; txtDescription.Text = exp.Description; dtpDate.Value = exp.Date; }
            };

            var panelInput = new Panel { Dock = DockStyle.Bottom, Height = 140, BackColor = UIHelper.CardBg, Padding = new Padding(15) };
            
            var tableInput = new TableLayoutPanel { Dock = DockStyle.Left, Width = 600, ColumnCount = 4, RowCount = 3 };
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            // Row 1
            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_title"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 0, 0);
            txtTitle = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableInput.Controls.Add(txtTitle, 1, 0);

            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_amount"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 2, 0);
            txtAmount = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableInput.Controls.Add(txtAmount, 3, 0);

            // Row 2
            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_category"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 0, 1);
            cmbCategory = new ComboBox { Dock = DockStyle.Top, Font = UIHelper.FontBody, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategory.Items.AddRange(new[] { "Rent", "Utilities", "Salaries", "Marketing", "Office Supplies", "Maintenance", "Insurance", "Transportation", "Equipment", "Other" });
            tableInput.Controls.Add(cmbCategory, 1, 1);

            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_date"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 2, 1);
            dtpDate = new DateTimePicker { Dock = DockStyle.Top };
            tableInput.Controls.Add(dtpDate, 3, 1);

            // Row 3
            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_notes"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 0, 2);
            txtDescription = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableInput.Controls.Add(txtDescription, 1, 2);
            tableInput.SetColumnSpan(txtDescription, 3);

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
                var exp = new Expense { Title = txtTitle.Text, Amount = decimal.TryParse(txtAmount.Text, out var a) ? a : 0, Category = cmbCategory.Text, Description = txtDescription.Text, Date = dtpDate.Value };
                var (ok, err) = await _svc.AddAsync(exp);
                if (ok) { ClearInputs(); await LoadAsync(); } else UIHelper.ShowError(err);
            };
            btnUpdate.Click += async (s, e) => {
                if (_selectedId == 0) return;
                var exp = new Expense { Id = _selectedId, Title = txtTitle.Text, Amount = decimal.TryParse(txtAmount.Text, out var a) ? a : 0, Category = cmbCategory.Text, Description = txtDescription.Text, Date = dtpDate.Value };
                var (ok, err) = await _svc.UpdateAsync(exp);
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
            this.Controls.Add(panelFilter);
            this.Controls.Add(lblTitle);
            this.Load += async (s, e) => await LoadAsync();
        }

        private async System.Threading.Tasks.Task LoadAsync() { dgv.DataSource = await _svc.GetAllAsync(); }
        private void ClearInputs() { _selectedId = 0; txtTitle.Clear(); txtAmount.Clear(); cmbCategory.SelectedIndex = -1; txtDescription.Clear(); dtpDate.Value = DateTime.Now; }
    }
}
