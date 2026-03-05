using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BussinessErp.Helpers
{
    /// <summary>
    /// Theming constants, control styling, and reusable UI helpers.
    /// Premium Teal + Dark Gray enterprise theme.
    /// </summary>
    public static class UIHelper
    {
        // ===== COLORS =====
        public static readonly Color PrimaryTeal = Color.FromArgb(0, 150, 136);
        public static readonly Color PrimaryTealDark = Color.FromArgb(0, 121, 107);
        public static readonly Color PrimaryTealLight = Color.FromArgb(77, 182, 172);
        public static readonly Color AccentOrange = Color.FromArgb(255, 152, 0);
        public static readonly Color AccentRed = Color.FromArgb(244, 67, 54);
        public static readonly Color AccentGreen = Color.FromArgb(76, 175, 80);
        public static readonly Color AccentBlue = Color.FromArgb(33, 150, 243);

        public static readonly Color DarkBg = Color.FromArgb(33, 37, 41);
        public static readonly Color DarkBgLight = Color.FromArgb(52, 58, 64);
        public static readonly Color DarkSidebar = Color.FromArgb(40, 44, 52);
        public static readonly Color CardBg = Color.FromArgb(255, 255, 255);
        public static readonly Color CardBorder = Color.FromArgb(222, 226, 230);
        public static readonly Color TextDark = Color.FromArgb(33, 37, 41);
        public static readonly Color TextMuted = Color.FromArgb(134, 142, 150);
        public static readonly Color TextWhite = Color.FromArgb(248, 249, 250);
        public static readonly Color RowAlternate = Color.FromArgb(245, 248, 250);
        public static readonly Color HoverBg = Color.FromArgb(0, 150, 136, 40);

        // ===== FONTS =====
        public static readonly Font FontTitle = new Font("Segoe UI", 18F, FontStyle.Bold);
        public static readonly Font FontSubtitle = new Font("Segoe UI", 14F, FontStyle.Regular);
        public static readonly Font FontBody = new Font("Segoe UI", 10F, FontStyle.Regular);
        public static readonly Font FontBodyBold = new Font("Segoe UI", 10F, FontStyle.Bold);
        public static readonly Font FontSmall = new Font("Segoe UI", 9F, FontStyle.Regular);
        public static readonly Font FontKPI = new Font("Segoe UI", 24F, FontStyle.Bold);
        public static readonly Font FontKPILabel = new Font("Segoe UI", 10F, FontStyle.Regular);
        public static readonly Font FontSidebar = new Font("Segoe UI", 11F, FontStyle.Regular);
        public static readonly Font FontSidebarBold = new Font("Segoe UI", 11F, FontStyle.Bold);

        // ===== STYLE METHODS =====

        public static void StyleForm(Form form, string title = "")
        {
            form.BackColor = Color.FromArgb(243, 244, 246);
            form.Font = FontBody;
            if (!string.IsNullOrEmpty(title))
                form.Text = title;
        }

        public static void StyleButton(Button btn, Color? bgColor = null, Color? fgColor = null)
        {
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = bgColor ?? PrimaryTeal;
            btn.ForeColor = fgColor ?? TextWhite;
            btn.Font = FontBodyBold;
            btn.Cursor = Cursors.Hand;
            btn.Height = 38;
            btn.Padding = new Padding(12, 4, 12, 4);

            btn.MouseEnter += (s, e) => btn.BackColor = ControlPaint.Light(btn.BackColor, 0.15f);
            btn.MouseLeave += (s, e) => btn.BackColor = bgColor ?? PrimaryTeal;
        }

        public static void StyleDangerButton(Button btn)
        {
            StyleButton(btn, AccentRed, TextWhite);
            btn.MouseLeave += (s, e) => btn.BackColor = AccentRed;
        }

        public static void StyleTextBox(TextBox txt)
        {
            txt.Font = FontBody;
            txt.BorderStyle = BorderStyle.FixedSingle;
            txt.Height = 32;
            txt.Padding = new Padding(4);
        }

        public static void StyleComboBox(ComboBox cmb)
        {
            cmb.Font = FontBody;
            cmb.FlatStyle = FlatStyle.Flat;
            cmb.DropDownStyle = ComboBoxStyle.DropDownList;
            cmb.Height = 32;
        }

        public static void StyleDataGridView(DataGridView dgv)
        {
            EnableDoubleBuffered(dgv);
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.BackgroundColor = CardBg;
            dgv.BorderStyle = BorderStyle.None;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.GridColor = CardBorder;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.RowHeadersVisible = false;
            dgv.EnableHeadersVisualStyles = false;
            dgv.MultiSelect = false;

            dgv.DefaultCellStyle.Font = FontBody;
            dgv.DefaultCellStyle.ForeColor = TextDark;
            dgv.DefaultCellStyle.SelectionBackColor = PrimaryTealLight;
            dgv.DefaultCellStyle.SelectionForeColor = TextWhite;
            dgv.DefaultCellStyle.Padding = new Padding(6, 4, 6, 4);

            dgv.AlternatingRowsDefaultCellStyle.BackColor = RowAlternate;

            dgv.ColumnHeadersDefaultCellStyle.BackColor = DarkBg;
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = TextWhite;
            dgv.ColumnHeadersDefaultCellStyle.Font = FontBodyBold;
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 6, 6, 6);
            dgv.ColumnHeadersHeight = 42;

            dgv.RowTemplate.Height = 36;
        }

        /// <summary>
        /// Enables double buffering for a control using reflection to reduce flickering.
        /// </summary>
        public static void EnableDoubleBuffered(Control control)
        {
            try
            {
                var prop = typeof(Control).GetProperty("DoubleBuffered", 
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                prop?.SetValue(control, true, null);
            }
            catch { }
        }

        /// <summary>
        /// Throttles an action to be called only after a specified delay since the last call.
        /// </summary>
        public static System.Action Debounce(System.Action func, int milliseconds = 300)
        {
            Timer timer = new Timer();
            timer.Interval = milliseconds;
            timer.Tick += (s, e) => {
                timer.Stop();
                func();
            };

            return () => {
                timer.Stop();
                timer.Start();
            };
        }

        public static void StyleLabel(Label lbl, bool isTitle = false)
        {
            lbl.ForeColor = isTitle ? TextDark : TextMuted;
            lbl.Font = isTitle ? FontSubtitle : FontBody;
        }

        /// <summary>
        /// Creates a rounded-corner KPI card panel.
        /// </summary>
        public static Panel CreateKPICard(string label, string value, Color accentColor, int width = 220, int height = 110)
        {
            var panel = new Panel
            {
                Size = new Size(width, height),
                BackColor = CardBg,
                Margin = new Padding(8),
                Padding = new Padding(16),
                Tag = "kpi"
            };

            var lblValue = new Label
            {
                Text = value,
                Font = FontKPI,
                ForeColor = accentColor,
                AutoSize = false,
                Dock = DockStyle.Top,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblTitle = new Label
            {
                Text = label,
                Font = FontKPILabel,
                ForeColor = TextMuted,
                AutoSize = false,
                Dock = DockStyle.Bottom,
                Height = 30,
                TextAlign = ContentAlignment.BottomLeft
            };

            var accentBar = new Panel
            {
                BackColor = accentColor,
                Dock = DockStyle.Left,
                Width = 4
            };

            panel.Controls.Add(lblValue);
            panel.Controls.Add(lblTitle);
            panel.Controls.Add(accentBar);

            return panel;
        }

        /// <summary>
        /// Shows a styled message box.
        /// </summary>
        public static void ShowInfo(string message, string title = null)
        {
            MessageBox.Show(message, title ?? LanguageManager.Get("msg_info"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void ShowError(string message, string title = null)
        {
            MessageBox.Show(message, title ?? LanguageManager.Get("msg_error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowWarning(string message, string title = null)
        {
            MessageBox.Show(message, title ?? LanguageManager.Get("msg_warning"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static DialogResult ShowConfirm(string message, string title = null)
        {
            return MessageBox.Show(message, title ?? LanguageManager.Get("msg_confirm"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
    }
}
