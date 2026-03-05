using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BussinessErp.BLL;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.UI.Forms
{
    public class frmPurchases : Form
    {
        private DataGridView dgvPurchases, dgvItems;
        private ComboBox cmbSupplier, cmbProduct;
        private TextBox txtQty, txtCost;
        private Button btnAddItem, btnCompletePurchase, btnPrintInvoice;
        private Label lblTotal;
        private List<PurchaseItem> _items = new List<PurchaseItem>();
        private PurchaseService _purchSvc = new PurchaseService();
        private InvoiceService _invoiceSvc = new InvoiceService();
        private SupplierService _supSvc = new SupplierService();
        private ProductService _prodSvc = new ProductService();

        public frmPurchases() { InitializeComponent(); }

        private void InitializeComponent()
        {
            this.Text = LanguageManager.Get("nav_purchases");
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.Padding = new Padding(20);

            LanguageManager.ApplyRTL(this);

            var lblTitle = new Label { Text = "🛒 " + LanguageManager.Get("nav_purchases"), Font = UIHelper.FontTitle, ForeColor = UIHelper.TextDark, Dock = DockStyle.Top, Height = 45 };

            // Purchase history
            var panelHistory = new Panel { Dock = DockStyle.Top, Height = 200 };
            var lblHist = new Label { Text = LanguageManager.Get("purch_history"), Font = UIHelper.FontSubtitle, ForeColor = UIHelper.TextDark, Dock = DockStyle.Top, Height = 28 };
            dgvPurchases = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvPurchases);
            panelHistory.Controls.Add(dgvPurchases);
            panelHistory.Controls.Add(lblHist);

            // New purchase panel
            var panelNew = new Panel { Dock = DockStyle.Fill, BackColor = UIHelper.CardBg, Padding = new Padding(15) };
            var lblNew = new Label { Text = LanguageManager.Get("new_purch"), Font = UIHelper.FontSubtitle, ForeColor = UIHelper.PrimaryTeal, Dock = DockStyle.Top, Height = 30 };

            // Supplier selection
            var panelSel = new Panel { Dock = DockStyle.Top, Height = 45 };
            var tableSup = new TableLayoutPanel { Dock = DockStyle.Left, Width = 300, ColumnCount = 2, RowCount = 1 };
            tableSup.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            tableSup.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            
            tableSup.Controls.Add(new Label { Text = LanguageManager.Get("lbl_supplier"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 0, 0);
            cmbSupplier = new ComboBox { Dock = DockStyle.Top, Font = UIHelper.FontBody, DropDownStyle = ComboBoxStyle.DropDownList };
            tableSup.Controls.Add(cmbSupplier, 1, 0);
            panelSel.Controls.Add(tableSup);

            // Item entry
            var panelItem = new Panel { Dock = DockStyle.Top, Height = 45 };
            var tableItem = new TableLayoutPanel { Dock = DockStyle.Left, Width = 660, ColumnCount = 7, RowCount = 1 };
            tableItem.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            tableItem.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
            tableItem.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
            tableItem.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 70));
            tableItem.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
            tableItem.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
            tableItem.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));

            tableItem.Controls.Add(new Label { Text = LanguageManager.Get("lbl_product"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 0, 0);
            cmbProduct = new ComboBox { Dock = DockStyle.Top, Font = UIHelper.FontBody, DropDownStyle = ComboBoxStyle.DropDownList };
            tableItem.Controls.Add(cmbProduct, 1, 0);

            tableItem.Controls.Add(new Label { Text = LanguageManager.Get("lbl_qty"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 2, 0);
            txtQty = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody, Text = "1" };
            tableItem.Controls.Add(txtQty, 3, 0);

            tableItem.Controls.Add(new Label { Text = LanguageManager.Get("lbl_cost"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 4, 0);
            txtCost = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableItem.Controls.Add(txtCost, 5, 0);

            btnAddItem = new Button { Text = LanguageManager.Get("add_item"), Dock = DockStyle.Top, Height = 35 };
            UIHelper.StyleButton(btnAddItem, UIHelper.AccentBlue);
            tableItem.Controls.Add(btnAddItem, 6, 0);
            panelItem.Controls.Add(tableItem);

            dgvItems = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false };
            UIHelper.StyleDataGridView(dgvItems);
            dgvItems.Columns.Add("Product", LanguageManager.Get("lbl_product").TrimEnd(':'));
            dgvItems.Columns.Add("Qty", LanguageManager.Get("lbl_qty").TrimEnd(':'));
            dgvItems.Columns.Add("Cost", LanguageManager.Get("lbl_cost").TrimEnd(':'));
            dgvItems.Columns.Add("Subtotal", LanguageManager.Get("total").TrimEnd(':'));

            var panelTotal = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = UIHelper.CardBg };
            lblTotal = new Label { Text = LanguageManager.Get("total") + " $0.00", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = UIHelper.PrimaryTeal, Dock = DockStyle.Left, AutoSize = false, Width = 300, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(15, 0, 0, 0) };
            btnCompletePurchase = new Button { Text = LanguageManager.Get("complete_purchase"), Dock = DockStyle.Right, Width = 180 };
            UIHelper.StyleButton(btnCompletePurchase, UIHelper.AccentGreen);

            btnPrintInvoice = new Button { Text = LanguageManager.Get("print_invoice"), Dock = DockStyle.Right, Width = 150 };
            UIHelper.StyleButton(btnPrintInvoice, UIHelper.AccentBlue);

            panelTotal.Controls.Add(btnPrintInvoice);
            panelTotal.Controls.Add(btnCompletePurchase);
            panelTotal.Controls.Add(lblTotal);

            panelNew.Controls.Add(dgvItems);
            panelNew.Controls.Add(panelTotal);
            panelNew.Controls.Add(panelItem);
            panelNew.Controls.Add(panelSel);
            panelNew.Controls.Add(lblNew);

            this.Controls.Add(panelNew);
            this.Controls.Add(panelHistory);
            this.Controls.Add(lblTitle);

            cmbProduct.SelectedIndexChanged += (s, e) => {
                if (cmbProduct.SelectedItem is Product p)
                    txtCost.Text = p.CostPrice.ToString("F2");
            };

            btnAddItem.Click += (s, e) => {
                if (cmbProduct.SelectedItem is Product p && int.TryParse(txtQty.Text, out int qty) && decimal.TryParse(txtCost.Text, out decimal cost))
                {
                    _items.Add(new PurchaseItem { ProductId = p.Id, Quantity = qty, CostPrice = cost });
                    dgvItems.Rows.Add(p.Name, qty, cost.ToString("C"), (qty * cost).ToString("C"));
                    UpdateTotal();
                }
            };

            btnCompletePurchase.Click += async (s, e) => {
                if (_items.Count == 0) { UIHelper.ShowWarning(LanguageManager.Get("msg_add_items")); return; }
                int? supId = cmbSupplier.SelectedItem is Supplier sup ? (int?)sup.Id : null;
                var (ok, purchId, err) = await _purchSvc.CreatePurchaseAsync(supId, _items);
                if (ok) {
                    UIHelper.ShowInfo(string.Format(LanguageManager.Get("msg_purch_complete"), purchId));
                    _items.Clear(); dgvItems.Rows.Clear(); UpdateTotal();
                    await LoadHistoryAsync();
                } else UIHelper.ShowError(err);
            };

            // Print invoice logic
            btnPrintInvoice.Click += async (s, e) => {
                Purchase selectedPurchase = null;

                if (dgvPurchases.SelectedRows.Count > 0)
                {
                    selectedPurchase = dgvPurchases.SelectedRows[0].DataBoundItem as Purchase;
                }

                if (selectedPurchase != null)
                {
                    var invoice = await _invoiceSvc.GetPurchaseInvoiceAsync(selectedPurchase.Id);
                    if (invoice != null) PrintHelper.PrintInvoice(invoice);
                    else UIHelper.ShowError("Could not load purchase details.");
                }
                else
                {
                    UIHelper.ShowWarning(LanguageManager.Get("msg_select_history"));
                }
            };

            this.Load += async (s, e) => {
                var suppliers = await _supSvc.GetAllAsync();
                cmbSupplier.DisplayMember = "Name";
                cmbSupplier.DataSource = suppliers;
                var products = await _prodSvc.GetAllAsync();
                cmbProduct.DisplayMember = "Name";
                cmbProduct.DataSource = products;
                await LoadHistoryAsync();

                // RBAC UI Enforcement
                btnCompletePurchase.Enabled = AuthService.IsManager;
                // Note: Delete button is not in the original UI but if it were, it would be Admin-only.
            };
        }

        private async System.Threading.Tasks.Task LoadHistoryAsync() { dgvPurchases.DataSource = await _purchSvc.GetAllAsync(); }
        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (var item in _items) total += item.Quantity * item.CostPrice;
            lblTotal.Text = $"{LanguageManager.Get("total")} {total:C}";
        }
    }
}
