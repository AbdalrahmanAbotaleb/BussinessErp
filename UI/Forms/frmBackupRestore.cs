using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BussinessErp.Helpers;

namespace BussinessErp.UI.Forms
{
    public class frmBackupRestore : Form
    {
        private Panel cardBackup, cardRestore;
        private Label lblTitle, lblBackupPath, lblRestoreFile;
        private TextBox txtBackupPath, txtRestoreFile;
        private Button btnBrowseBackup, btnBrowseRestore, btnBackupNow, btnRestoreNow;

        public frmBackupRestore()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            UIHelper.StyleForm(this, LanguageManager.Get("backup_restore_title"));
            this.Padding = new Padding(30);

            lblTitle = new Label
            {
                Text = LanguageManager.Get("backup_restore_title"),
                Font = UIHelper.FontTitle,
                ForeColor = UIHelper.PrimaryTeal,
                Dock = DockStyle.Top,
                Height = 60
            };

            // ===== BACKUP CARD =====
            cardBackup = CreateCard(LanguageManager.Get("nav_backup_restore"));
            cardBackup.Location = new Point(30, 90);

            lblBackupPath = new Label { Text = LanguageManager.Get("lbl_backup_path"), Location = new Point(20, 40), AutoSize = true };
            txtBackupPath = new TextBox { Location = new Point(20, 65), Width = 400, ReadOnly = true };
            UIHelper.StyleTextBox(txtBackupPath);

            btnBrowseBackup = new Button { Text = "...", Location = new Point(425, 64), Width = 40 };
            UIHelper.StyleButton(btnBrowseBackup, UIHelper.DarkBgLight);

            btnBackupNow = new Button { Text = LanguageManager.Get("btn_backup_now"), Location = new Point(20, 110), Width = 150 };
            UIHelper.StyleButton(btnBackupNow);

            cardBackup.Controls.AddRange(new Control[] { lblBackupPath, txtBackupPath, btnBrowseBackup, btnBackupNow });

            // ===== RESTORE CARD =====
            cardRestore = CreateCard(LanguageManager.Get("btn_restore_now"));
            cardRestore.Location = new Point(30, 280);

            lblRestoreFile = new Label { Text = LanguageManager.Get("lbl_restore_file"), Location = new Point(20, 40), AutoSize = true };
            txtRestoreFile = new TextBox { Location = new Point(20, 65), Width = 400, ReadOnly = true };
            UIHelper.StyleTextBox(txtRestoreFile);

            btnBrowseRestore = new Button { Text = "...", Location = new Point(425, 64), Width = 40 };
            UIHelper.StyleButton(btnBrowseRestore, UIHelper.DarkBgLight);

            btnRestoreNow = new Button { Text = LanguageManager.Get("btn_restore_now"), Location = new Point(20, 110), Width = 150 };
            UIHelper.StyleButton(btnRestoreNow, UIHelper.AccentOrange);

            cardRestore.Controls.AddRange(new Control[] { lblRestoreFile, txtRestoreFile, btnBrowseRestore, btnRestoreNow });

            this.Controls.Add(cardRestore);
            this.Controls.Add(cardBackup);
            this.Controls.Add(lblTitle);

            // ===== EVENTS =====
            btnBrowseBackup.Click += (s, e) =>
            {
                using (var fbd = new FolderBrowserDialog())
                {
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        string fileName = $"BussinessDB_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak";
                        txtBackupPath.Text = Path.Combine(fbd.SelectedPath, fileName);
                    }
                }
            };

            btnBrowseRestore.Click += (s, e) =>
            {
                using (var ofd = new OpenFileDialog { Filter = "Backup Files (*.bak)|*.bak" })
                {
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        txtRestoreFile.Text = ofd.FileName;
                    }
                }
            };

            btnBackupNow.Click += async (s, e) =>
            {
                if (string.IsNullOrEmpty(txtBackupPath.Text))
                {
                    UIHelper.ShowWarning(LanguageManager.Get("msg_select_path"));
                    return;
                }

                try
                {
                    btnBackupNow.Enabled = false;
                    btnBackupNow.Text = LanguageManager.Get("processing");
                    
                    await DatabaseHelper.BackupDatabaseAsync(txtBackupPath.Text);
                    
                    UIHelper.ShowInfo(string.Format(LanguageManager.Get("msg_backup_success"), txtBackupPath.Text));
                }
                catch (Exception ex)
                {
                    UIHelper.ShowError(ex.Message);
                }
                finally
                {
                    btnBackupNow.Enabled = true;
                    btnBackupNow.Text = LanguageManager.Get("btn_backup_now");
                }
            };

            btnRestoreNow.Click += async (s, e) =>
            {
                if (string.IsNullOrEmpty(txtRestoreFile.Text))
                {
                    UIHelper.ShowWarning(LanguageManager.Get("msg_select_file"));
                    return;
                }

                if (UIHelper.ShowConfirm(LanguageManager.Get("msg_restore_confirm")) != DialogResult.Yes) return;

                try
                {
                    btnRestoreNow.Enabled = false;
                    btnRestoreNow.Text = LanguageManager.Get("processing");

                    await DatabaseHelper.RestoreDatabaseAsync(txtRestoreFile.Text);

                    UIHelper.ShowInfo(LanguageManager.Get("msg_restore_success"));
                    
                    // Restart Application
                    Application.Restart();
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    UIHelper.ShowError(ex.Message);
                }
                finally
                {
                    btnRestoreNow.Enabled = true;
                    btnRestoreNow.Text = LanguageManager.Get("btn_restore_now");
                }
            };

            LanguageManager.ApplyRTL(this);
        }

        private Panel CreateCard(string title)
        {
            var p = new Panel
            {
                Size = new Size(500, 170),
                BackColor = UIHelper.CardBg,
                BorderStyle = BorderStyle.FixedSingle
            };

            var lbl = new Label
            {
                Text = title,
                Font = UIHelper.FontBodyBold,
                ForeColor = UIHelper.TextDark,
                Location = new Point(15, 10),
                AutoSize = true
            };
            p.Controls.Add(lbl);

            return p;
        }
    }
}
