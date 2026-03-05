using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BussinessErp.BLL;
using BussinessErp.Helpers;
using BussinessErp.Models;

namespace BussinessErp.UI.Forms
{
    public class frmChatbot : Form
    {
        private Panel pnlChatContainer;
        private TableLayoutPanel pnlInputArea;
        private RoundedPanel pnlInputContainer;
        private TextBox txtInput;
        private Button btnSend;
        private Button btnClear;
        private Button btnDarkMode;

        private GeminiService _geminiService;
        private List<ChatMessage> chatMessages = new List<ChatMessage>();
        private bool isDarkMode = false;

        // ===== Typing animation =====
        private Timer typingTimer;
        private int dotCount = 0;
        private Label lblTyping;

        public frmChatbot()
        {
            InitializeComponent();
            LanguageManager.ApplyRTL(this);
            _geminiService = new GeminiService("AIzaSyC5FnxQKQ54BGk0_wIw4OHuRKVpPG34no4");
            this.Load += FrmChatbot_Load;
        }

        private void FrmChatbot_Load(object sender, EventArgs e)
        {
            txtInput.Focus();
            LoadChatHistory();
        }

        private void InitializeComponent()
        {
            // === Chat Container ===
            pnlChatContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                Padding = new Padding(15),
                BackColor = Color.White
            };
            // Ensure child controls trigger scrollbar update
            pnlChatContainer.ControlAdded += (s, e) => pnlChatContainer.ScrollControlIntoView(e.Control);

            // === Input Area ===
            pnlInputArea = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 120,
                ColumnCount = 4
            };
            pnlInputArea.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlInputArea.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            pnlInputArea.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));
            pnlInputArea.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80F));

            // === Rounded Panel for TextBox ===
            pnlInputContainer = new RoundedPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                Radius = 30
            };

            txtInput = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 16F, FontStyle.Regular),
                Dock = DockStyle.Fill,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            txtInput.KeyDown += TxtInput_KeyDown;
            pnlInputContainer.Controls.Add(txtInput);

            // === Buttons ===
            btnSend = new Button
            {
                Text = LanguageManager.Get("btn_send"),
                Height = 70,
                BackColor = Color.Teal,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.Click += BtnSend_Click;

            btnClear = new Button
            {
                Text = LanguageManager.Get("btn_clear"),
                Height = 70,
                BackColor = Color.DarkRed,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };
            btnClear.FlatAppearance.BorderSize = 0;
            btnClear.Click += BtnClear_Click;

            btnDarkMode = new Button
            {
                Text = LanguageManager.Get("btn_dark"),
                Height = 70,
                BackColor = Color.Black,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold)
            };
            btnDarkMode.FlatAppearance.BorderSize = 0;
            btnDarkMode.Click += BtnDarkMode_Click;

            pnlInputArea.Controls.Add(pnlInputContainer, 0, 0);
            pnlInputArea.Controls.Add(btnSend, 1, 0);
            pnlInputArea.Controls.Add(btnClear, 2, 0);
            pnlInputArea.Controls.Add(btnDarkMode, 3, 0);

            // === Form Settings ===
            this.Text = LanguageManager.Get("lbl_chatbot_title");
            this.ClientSize = new Size(1200, 700);
            this.Controls.Add(pnlChatContainer);
            this.Controls.Add(pnlInputArea);
        }

        // ===== Send / Enter Key =====
        private async void BtnSend_Click(object sender, EventArgs e) => await SendMessageAsync();
        private async void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                e.SuppressKeyPress = true;
                await SendMessageAsync();
            }
        }

        // ===== Send Message with Typing Animation =====
        private async Task SendMessageAsync()
        {
            string userPrompt = txtInput.Text.Trim();
            if (string.IsNullOrEmpty(userPrompt)) return;

            AddMessage(userPrompt, true);
            txtInput.Clear();
            btnSend.Enabled = false;

            ChatMessage loading = new ChatMessage
            {
                Text = LanguageManager.Get("msg_typing"),
                IsUser = false,
                TimeStamp = DateTime.Now
            };
            Panel loadingPanel = AddBubbleToUI(loading);

            lblTyping = loadingPanel.Controls[0] as Label;

            typingTimer = new Timer();
            typingTimer.Interval = 500; // كل نصف ثانية
            typingTimer.Tick += TypingTimer_Tick;
            typingTimer.Start();

            try
            {
                string response = await _geminiService.SendPromptAsync(userPrompt);
                typingTimer.Stop();
                pnlChatContainer.Controls.Remove(loadingPanel);

                AddMessage(response, false);
            }
            catch (Exception ex)
            {
                typingTimer.Stop();
                pnlChatContainer.Controls.Remove(loadingPanel);
                AddMessage(LanguageManager.Get("msg_error") + ": " + ex.Message, false);
            }
            finally
            {
                btnSend.Enabled = true;
                txtInput.Focus();
            }
        }

        private void TypingTimer_Tick(object sender, EventArgs e)
        {
            dotCount = (dotCount + 1) % 4; // 0 → 3 dots
            if (lblTyping != null)
                lblTyping.Text = LanguageManager.Get("msg_typing") + new string('.', dotCount);
        }

        // ===== Add Message =====
        private void AddMessage(string text, bool isUser)
        {
            var message = new ChatMessage
            {
                Text = text,
                IsUser = isUser,
                TimeStamp = DateTime.Now
            };

            chatMessages.Add(message);
            AddBubbleToUI(message);
            SaveChatHistory();
        }

        // ===== Add Bubble =====
        private Panel AddBubbleToUI(ChatMessage message)
        {
            RoundedPanel bubble = new RoundedPanel();
            bubble.BackColor = message.IsUser ? Color.Teal : Color.LightGray;
            bubble.Padding = new Padding(10);
            bubble.AutoSize = true;

            // === نص الرسالة ===
            Label lblMessage = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(600, 0),
                Font = new Font("Segoe UI", 14F, FontStyle.Regular),
                Padding = new Padding(10),
                ForeColor = message.IsUser ? Color.White : Color.Black,
                Text = message.Text + " "
            };
            bubble.Controls.Add(lblMessage);

            // === الوقت ===
            Label lblTime = new Label
            {
                Text = message.TimeStamp.ToString("hh:mm tt"),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                ForeColor = message.IsUser ? Color.LightCyan : Color.DimGray,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            bubble.Controls.Add(lblTime);

            // Calculate bubble dimensions manually because AutoSize might be late
            bubble.Size = new Size(
                Math.Max(lblMessage.PreferredWidth, lblTime.PreferredWidth) + 30,
                lblMessage.PreferredHeight + lblTime.PreferredHeight + 25
            );

            lblTime.Top = lblMessage.Bottom + 2;
            lblTime.Left = 10;

            // Positioning bubble in container
            int yOffset = 15;
            foreach (Control c in pnlChatContainer.Controls)
            {
                if (c.Bottom + 10 > yOffset) yOffset = c.Bottom + 10;
            }
            
            bubble.Top = yOffset;
            bubble.Left = message.IsUser ? pnlChatContainer.Width - bubble.Width - 45 : 15;

            // Copy button for AI
            if (!message.IsUser && message.Text != LanguageManager.Get("msg_typing"))
            {
                Button btnCopy = new Button
                {
                    Text = "", 
                    Size = new Size(30, 20),
                    BackColor = Color.FromArgb(230, 230, 230),
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand,
                    TabStop = false,
                    Top = 5,
                    Left = bubble.Width - 35,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                btnCopy.FlatAppearance.BorderSize = 0;

                ToolTip tip = new ToolTip();
                tip.SetToolTip(btnCopy, "Copy");

                btnCopy.Click += (s, e) =>
                {
                    Clipboard.SetText(lblMessage.Text.Trim());
                    tip.Show("Copied", btnCopy, btnCopy.Width / 2, btnCopy.Height / 2, 1000);
                };

                bubble.Controls.Add(btnCopy);
            }

            pnlChatContainer.Controls.Add(bubble);

            pnlChatContainer.ScrollControlIntoView(bubble);

            return bubble;
        }

        // ===== Save / Load Chat History (User-Specific) =====
        private Dictionary<string, List<ChatMessage>> GetHistoryDictionary()
        {
            string allHistoryJson = BussinessErp.Properties.Settings.Default.ChatHistory;
            if (string.IsNullOrEmpty(allHistoryJson))
                return new Dictionary<string, List<ChatMessage>>();

            try
            {
                // Try to load as Dictionary first (New format)
                var dict = JsonConvert.DeserializeObject<Dictionary<string, List<ChatMessage>>>(allHistoryJson);
                if (dict != null) return dict;
            }
            catch
            {
                // Fallback: If it's the old format (List), convert it to the new format
                try
                {
                    var legacyList = JsonConvert.DeserializeObject<List<ChatMessage>>(allHistoryJson);
                    if (legacyList != null)
                    {
                        var dict = new Dictionary<string, List<ChatMessage>>();
                        // Since old format didn't have users, we can't be sure who it belongs to.
                        // We'll assign it to the current user if they are logged in, or Guest.
                        var owner = AuthService.CurrentUser?.Username ?? "Guest";
                        dict[owner] = legacyList;
                        return dict;
                    }
                }
                catch { }
            }

            return new Dictionary<string, List<ChatMessage>>();
        }

        private void SaveChatHistory()
        {
            try {
                var currentUser = AuthService.CurrentUser?.Username ?? "Guest";
                var historyDict = GetHistoryDictionary();

                historyDict[currentUser] = chatMessages;

                BussinessErp.Properties.Settings.Default.ChatHistory = JsonConvert.SerializeObject(historyDict);
                BussinessErp.Properties.Settings.Default.Save();
            } catch { }
        }

        private void LoadChatHistory()
        {
            try {
                var currentUser = AuthService.CurrentUser?.Username ?? "Guest";
                var historyDict = GetHistoryDictionary();

                if (historyDict.ContainsKey(currentUser))
                {
                    chatMessages = historyDict[currentUser] ?? new List<ChatMessage>();
                }
                else
                {
                    chatMessages = new List<ChatMessage>();
                }

                foreach (var msg in chatMessages)
                    AddBubbleToUI(msg);
            } catch { }
        }

        // ===== Buttons Actions =====
        private void BtnClear_Click(object sender, EventArgs e)
        {
            var currentUser = AuthService.CurrentUser?.Username ?? "Guest";
            chatMessages.Clear();
            pnlChatContainer.Controls.Clear();

            try {
                var historyDict = GetHistoryDictionary();
                if (historyDict.ContainsKey(currentUser))
                {
                    historyDict.Remove(currentUser);
                    BussinessErp.Properties.Settings.Default.ChatHistory = JsonConvert.SerializeObject(historyDict);
                    BussinessErp.Properties.Settings.Default.Save();
                }
            } catch { }
        }

        private void BtnDarkMode_Click(object sender, EventArgs e)
        {
            isDarkMode = !isDarkMode;
            if (isDarkMode)
            {
                this.BackColor = Color.FromArgb(30, 30, 30);
                pnlChatContainer.BackColor = Color.FromArgb(45, 45, 45);
            }
            else
            {
                this.BackColor = Color.FromArgb(245, 247, 250);
                pnlChatContainer.BackColor = Color.White;
            }
        }

        // REMOVED private class ChatMessage as it is now in Models namespace

        public class RoundedPanel : Panel
        {
            public int Radius { get; set; } = 20;

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                GraphicsPath path = new GraphicsPath();
                int radius = this.Radius;
                if (Width <= 0 || Height <= 0) return;
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(Width - radius, Height - radius, radius, radius, 0, 90);
                path.AddArc(0, Height - radius, radius, radius, 90, 90);
                path.CloseFigure();

                this.Region = new Region(path);
            }
        }
    }
}
