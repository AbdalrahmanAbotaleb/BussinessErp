using System;
using System.Drawing;
using System.Windows.Forms;
using BussinessErp.BLL;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.UI.Forms
{
    public class frmProducts : Form
    {
        private DataGridView dgv;
        private TextBox txtSearch, txtName, txtCost, txtSell, txtQty, txtReorder;
        private ComboBox cmbCategory;
        private Button btnAdd, btnUpdate, btnDelete, btnClear;
        private ProductService _svc = new ProductService();
        private int _selectedId = 0;

        public frmProducts() { InitializeComponent(); }

        private void InitializeComponent()
        {
            this.Text = LanguageManager.Get("nav_products");
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.Padding = new Padding(20);

            LanguageManager.ApplyRTL(this);

            var lblTitle = new Label { Text = "📦 " + LanguageManager.Get("nav_products"), Font = UIHelper.FontTitle, ForeColor = UIHelper.TextDark, Dock = DockStyle.Top, Height = 45 };

            // Search + Category filter
            var panelSearch = new Panel { Dock = DockStyle.Top, Height = 45, Padding = new Padding(0, 5, 0, 5) };
            txtSearch = new TextBox { Width = 300, Dock = DockStyle.Left, Font = UIHelper.FontBody };
            cmbCategory = new ComboBox { Width = 180, Dock = DockStyle.Left, Font = UIHelper.FontBody, DropDownStyle = ComboBoxStyle.DropDownList };
            var btnLowStock = new Button { Text = LanguageManager.Get("msg_low_stock"), Dock = DockStyle.Left, Width = 110 }; UIHelper.StyleButton(btnLowStock, UIHelper.AccentOrange);

            // Live Search Events with Debouncing
            var debouncedSearch = UIHelper.Debounce(async () => await PerformSearch(), 300);
            txtSearch.TextChanged += (s, e) => debouncedSearch();
            cmbCategory.SelectedIndexChanged += async (s, e) => await PerformSearch();
            btnLowStock.Click += async (s, e) => { dgv.DataSource = await _svc.GetLowStockAsync(); };

            panelSearch.Controls.Add(btnLowStock);
            panelSearch.Controls.Add(cmbCategory);
            panelSearch.Controls.Add(txtSearch);

            dgv = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgv);
            dgv.SelectionChanged += (s, e) => {
                if (dgv.CurrentRow?.DataBoundItem is Product p)
                { _selectedId = p.Id; txtName.Text = p.Name; txtCost.Text = p.CostPrice.ToString("F2"); txtSell.Text = p.SellPrice.ToString("F2"); txtQty.Text = p.Quantity.ToString(); txtReorder.Text = p.ReorderLevel.ToString(); cmbCategory.Text = p.Category; }
            };

            // Input panel
            var panelInput = new Panel { Dock = DockStyle.Bottom, Height = 140, BackColor = UIHelper.CardBg, Padding = new Padding(15) };
            
            var tableInput = new TableLayoutPanel { Dock = DockStyle.Left, Width = 600, ColumnCount = 6, RowCount = 2 };
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            tableInput.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));

            // Row 1
            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_name"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 0, 0);
            txtName = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableInput.Controls.Add(txtName, 1, 0);

            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_cost"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 2, 0);
            txtCost = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableInput.Controls.Add(txtCost, 3, 0);

            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_sell"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 4, 0);
            txtSell = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableInput.Controls.Add(txtSell, 5, 0);

            // Row 2
            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_qty"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 0, 1);
            txtQty = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableInput.Controls.Add(txtQty, 1, 1);

            tableInput.Controls.Add(new Label { Text = LanguageManager.Get("lbl_reorder"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 2, 1);
            txtReorder = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableInput.Controls.Add(txtReorder, 3, 1);

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

            btnAdd.Click += async (s, e) => {
                var p = new Product
                {
                    Name = txtName.Text, Category = cmbCategory.Text,
                    CostPrice = decimal.TryParse(txtCost.Text, out var c) ? c : 0,
                    SellPrice = decimal.TryParse(txtSell.Text, out var sp) ? sp : 0,
                    Quantity = int.TryParse(txtQty.Text, out var q) ? q : 0,
                    ReorderLevel = int.TryParse(txtReorder.Text, out var r) ? r : 10
                };
                var (ok, err) = await _svc.AddAsync(p);
                if (ok) { ClearInputs(); await LoadAsync(); } else UIHelper.ShowError(err);
            };
            btnUpdate.Click += async (s, e) => {
                if (_selectedId == 0) return;
                var p = new Product
                {
                    Id = _selectedId, Name = txtName.Text, Category = cmbCategory.Text,
                    CostPrice = decimal.TryParse(txtCost.Text, out var c) ? c : 0,
                    SellPrice = decimal.TryParse(txtSell.Text, out var sp) ? sp : 0,
                    Quantity = int.TryParse(txtQty.Text, out var q) ? q : 0,
                    ReorderLevel = int.TryParse(txtReorder.Text, out var r) ? r : 10
                };
                var (ok, err) = await _svc.UpdateAsync(p);
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
                var cats = await _svc.GetCategoriesAsync();
                cmbCategory.Items.Clear();
                cmbCategory.Items.Add(LanguageManager.Get("all_categories"));
                cats.ForEach(c => cmbCategory.Items.Add(c));
                cmbCategory.SelectedIndex = 0;
                await LoadAsync();

                // RBAC UI Enforcement
                btnAdd.Enabled = AuthService.IsManager;
                btnUpdate.Enabled = AuthService.IsManager;
                btnDelete.Enabled = AuthService.IsAdmin;
            };
        }

        private async System.Threading.Tasks.Task LoadAsync() { dgv.DataSource = await _svc.GetAllAsync(); }
        
        private async System.Threading.Tasks.Task PerformSearch()
        {
            if (!string.IsNullOrEmpty(txtSearch.Text))
                dgv.DataSource = await _svc.SearchAsync(txtSearch.Text);
            else if (cmbCategory.SelectedIndex > 0)
                dgv.DataSource = await _svc.GetByCategoryAsync(cmbCategory.Text);
            else
                await LoadAsync();
        }

        private void ClearInputs() { _selectedId = 0; txtName.Clear(); txtCost.Clear(); txtSell.Clear(); txtQty.Clear(); txtReorder.Clear(); }
    }
}
