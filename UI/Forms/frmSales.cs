using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BussinessErp.BLL;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.UI.Forms
{
    public class frmSales : Form
    {
        private DataGridView dgvSales, dgvItems;
        private ComboBox cmbCustomer, cmbProduct;
        private TextBox txtQty, txtPrice;
        private Button btnAddItem, btnCompleteSale, btnPrintInvoice;
        private Label lblTotal;
        private List<SaleItem> _items = new List<SaleItem>();
        private SaleService _saleSvc = new SaleService();
        private InvoiceService _invoiceSvc = new InvoiceService();
        private CustomerService _custSvc = new CustomerService();
        private ProductService _prodSvc = new ProductService();

        public frmSales() { InitializeComponent(); }

        private void InitializeComponent()
        {
            this.Text = LanguageManager.Get("nav_sales");
            this.BackColor = Color.FromArgb(243, 244, 246);
            this.Padding = new Padding(20);

            LanguageManager.ApplyRTL(this);

            var lblTitle = new Label { Text = "💵 " + LanguageManager.Get("nav_sales"), Font = UIHelper.FontTitle, ForeColor = UIHelper.TextDark, Dock = DockStyle.Top, Height = 45 };

            // Sales history grid
            var panelHistory = new Panel { Dock = DockStyle.Top, Height = 200 };
            var lblHist = new Label { Text = LanguageManager.Get("sales_history"), Font = UIHelper.FontSubtitle, ForeColor = UIHelper.TextDark, Dock = DockStyle.Top, Height = 28 };
            dgvSales = new DataGridView { Dock = DockStyle.Fill };
            UIHelper.StyleDataGridView(dgvSales);
            panelHistory.Controls.Add(dgvSales);
            panelHistory.Controls.Add(lblHist);

            // New sale panel
            var panelNew = new Panel { Dock = DockStyle.Fill, BackColor = UIHelper.CardBg, Padding = new Padding(15) };
            var lblNew = new Label { Text = LanguageManager.Get("new_sale"), Font = UIHelper.FontSubtitle, ForeColor = UIHelper.PrimaryTeal, Dock = DockStyle.Top, Height = 30 };

            // Customer selection
            var panelSel = new Panel { Dock = DockStyle.Top, Height = 45 };
            var tableCust = new TableLayoutPanel { Dock = DockStyle.Left, Width = 300, ColumnCount = 2, RowCount = 1 };
            tableCust.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            tableCust.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            
            tableCust.Controls.Add(new Label { Text = LanguageManager.Get("lbl_customer"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 0, 0);
            cmbCustomer = new ComboBox { Dock = DockStyle.Top, Font = UIHelper.FontBody, DropDownStyle = ComboBoxStyle.DropDownList };
            tableCust.Controls.Add(cmbCustomer, 1, 0);
            panelSel.Controls.Add(tableCust);

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

            tableItem.Controls.Add(new Label { Text = LanguageManager.Get("lbl_price"), Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = UIHelper.FontBody }, 4, 0);
            txtPrice = new TextBox { Dock = DockStyle.Top, Font = UIHelper.FontBody };
            tableItem.Controls.Add(txtPrice, 5, 0);

            btnAddItem = new Button { Text = LanguageManager.Get("add_item"), Dock = DockStyle.Top, Height = 35 };
            UIHelper.StyleButton(btnAddItem, UIHelper.AccentBlue);
            tableItem.Controls.Add(btnAddItem, 6, 0);
            panelItem.Controls.Add(tableItem);

            // Items grid
            dgvItems = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false };
            UIHelper.StyleDataGridView(dgvItems);
            dgvItems.Columns.Add("Product", LanguageManager.Get("lbl_product").TrimEnd(':'));
            dgvItems.Columns.Add("Qty", LanguageManager.Get("lbl_qty").TrimEnd(':'));
            dgvItems.Columns.Add("Price", LanguageManager.Get("lbl_price").TrimEnd(':'));
            dgvItems.Columns.Add("Subtotal", LanguageManager.Get("total").TrimEnd(':'));

            // Total + Complete
            var panelTotal = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = UIHelper.CardBg };
            lblTotal = new Label { Text = LanguageManager.Get("total") + " $0.00", Font = new Font("Segoe UI", 14, FontStyle.Bold), ForeColor = UIHelper.PrimaryTeal, Dock = DockStyle.Left, AutoSize = false, Width = 300, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(15, 0, 0, 0) };
            btnCompleteSale = new Button { Text = LanguageManager.Get("complete_sale"), Dock = DockStyle.Right, Width = 180 };
            UIHelper.StyleButton(btnCompleteSale, UIHelper.AccentGreen);
            
            btnPrintInvoice = new Button { Text = LanguageManager.Get("print_invoice"), Dock = DockStyle.Right, Width = 150 };
            UIHelper.StyleButton(btnPrintInvoice, UIHelper.AccentBlue);

            panelTotal.Controls.Add(btnPrintInvoice);
            panelTotal.Controls.Add(btnCompleteSale);
            panelTotal.Controls.Add(lblTotal);

            panelNew.Controls.Add(dgvItems);
            panelNew.Controls.Add(panelTotal);
            panelNew.Controls.Add(panelItem);
            panelNew.Controls.Add(panelSel);
            panelNew.Controls.Add(lblNew);

            this.Controls.Add(panelNew);
            this.Controls.Add(panelHistory);
            this.Controls.Add(lblTitle);

            // Product selection auto-fills price
            cmbProduct.SelectedIndexChanged += (s, e) => {
                if (cmbProduct.SelectedItem is Product p)
                    txtPrice.Text = p.SellPrice.ToString("F2");
            };

            // Add item to cart
            btnAddItem.Click += (s, e) => {
                if (cmbProduct.SelectedItem is Product p && int.TryParse(txtQty.Text, out int qty) && decimal.TryParse(txtPrice.Text, out decimal price))
                {
                    _items.Add(new SaleItem { ProductId = p.Id, Quantity = qty, SellPrice = price });
                    dgvItems.Rows.Add(p.Name, qty, price.ToString("C"), (qty * price).ToString("C"));
                    UpdateTotal();
                }
            };

            // Complete sale
            btnCompleteSale.Click += async (s, e) => {
                if (_items.Count == 0) { UIHelper.ShowWarning(LanguageManager.Get("msg_add_items")); return; }
                int? custId = cmbCustomer.SelectedItem is Customer c ? (int?)c.Id : null;
                var (ok, saleId, err) = await _saleSvc.CreateSaleAsync(custId, _items);
                if (ok) {
                    UIHelper.ShowInfo(string.Format(LanguageManager.Get("msg_sale_complete"), saleId));
                    _items.Clear(); dgvItems.Rows.Clear(); UpdateTotal();
                    await LoadHistoryAsync();
                } else UIHelper.ShowError(err);
            };

            // Print invoice logic
            btnPrintInvoice.Click += async (s, e) => {
                Sale selectedSale = null;

                // Priority 1: Selected row in history
                if (dgvSales.SelectedRows.Count > 0)
                {
                    selectedSale = dgvSales.SelectedRows[0].DataBoundItem as Sale;
                }

                if (selectedSale != null)
                {
                    var invoice = await _invoiceSvc.GetSaleInvoiceAsync(selectedSale.Id);
                    if (invoice != null) PrintHelper.PrintInvoice(invoice);
                    else UIHelper.ShowError("Could not load sale details.");
                }
                else
                {
                    UIHelper.ShowWarning(LanguageManager.Get("msg_select_history"));
                }
            };

            this.Load += async (s, e) => {
                var customers = await _custSvc.GetAllAsync();
                cmbCustomer.DisplayMember = "Name";
                cmbCustomer.DataSource = customers;

                var products = await _prodSvc.GetAllAsync();
                cmbProduct.DisplayMember = "Name";
                cmbProduct.DataSource = products;

                await LoadHistoryAsync();
            };
        }

        private async System.Threading.Tasks.Task LoadHistoryAsync() { dgvSales.DataSource = await _saleSvc.GetAllAsync(); }
        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (var item in _items) total += item.Quantity * item.SellPrice;
            lblTotal.Text = $"{LanguageManager.Get("total")} {total:C}";
        }
    }
}
