using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        // ── Network ──────────────────────────────────────────────
        TcpClient _client;
        StreamReader _reader;
        StreamWriter _writer;

        // ── State ────────────────────────────────────────────────
        readonly Dictionary<string, bool> _members = new();   // username → isTyping flag (not used directly, just tracking presence)
        readonly DispatcherTimer _typingTimer = new();        // debounce "stop typing" notification
        bool _isTyping = false;

        // ── Emojis ───────────────────────────────────────────────
        static readonly string[] Emojis = {
            "😊","😂","😍","🥰","😎","😭","😅","🤔","😤","🥺",
            "👍","👏","🙏","💪","🤝","❤️","🔥","✨","🎉","💯",
            "😆","😜","🤣","😇","🙄","😏","🤩","😴","🤯","😱"
        };

        // ─────────────────────────────────────────────────────────
        public MainWindow()
        {
            InitializeComponent();
            BuildEmojiPicker();
            SetupTypingTimer();
            // Connect();
            txtLoginName.Focus();
        }

        // ======================== SETUP ========================

        void BuildEmojiPicker()
        {
            foreach (var emoji in Emojis)
            {
                var btn = new Button
                {
                    Content = emoji,
                    FontSize = 20,
                    Width = 36,
                    Height = 36,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Margin = new Thickness(2),
                    ToolTip = emoji
                };
                btn.Click += (s, e) =>
                {
                    txtMessage.Text += emoji;
                    txtMessage.CaretIndex = txtMessage.Text.Length;
                    txtMessage.Focus();
                };
                wrapEmoji.Children.Add(btn);
            }
        }

        void SetupTypingTimer()
        {
            _typingTimer.Interval = TimeSpan.FromSeconds(2);
            _typingTimer.Tick += (s, e) =>
            {
                _typingTimer.Stop();
                if (_isTyping)
                {
                    _isTyping = false;
                    SendJson(new { type = "typing", username = txtUsername.Text, isTyping = false });
                }
            };
        }

        // ======================== NETWORK ========================

        void Connect()
        {
            try
            {
                _client = new TcpClient();
                _client.Connect("127.0.0.1", 5000);

                var ns = _client.GetStream();
                _reader = new StreamReader(ns, System.Text.Encoding.UTF8);
                _writer = new StreamWriter(ns, System.Text.Encoding.UTF8) { AutoFlush = true };

                // Announce join
                SendJson(new { type = "join", username = txtUsername.Text });

                var t = new Thread(ReceiveLoop) { IsBackground = true };
                t.Start();
            }
            catch
            {
                MessageBox.Show("Không kết nối được Server!", "Lỗi kết nối",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // ======================== RECEIVE LOOP ========================

        void ReceiveLoop()
        {
            try
            {
                while (true)
                {
                    string line = _reader?.ReadLine();
                    if (line == null) break;

                    try
                    {
                        using var doc = JsonDocument.Parse(line);
                        var root = doc.RootElement;
                        string type = root.TryGetProperty("type", out var t) ? t.GetString() : "";

                        switch (type)
                        {
                            case "message":
                                HandleMessage(root);
                                break;

                            case "typing":
                                HandleTyping(root);
                                break;

                            case "join":
                                HandleJoin(root);
                                break;

                            case "leave":
                                HandleLeave(root);
                                break;

                            case "members":
                                HandleMemberList(root);
                                break;
                        }
                    }
                    catch { /* JSON parse error – skip */ }
                }
            }
            catch { /* connection closed */ }

            Dispatcher.Invoke(() =>
            {
                AddSystemMessage("⚠ Mất kết nối với server.");
            });
        }

        // ======================== HANDLE EVENTS ========================

        void HandleMessage(JsonElement root)
        {
            string user = root.TryGetProperty("username", out var u) ? u.GetString() : "?";
            string text = root.TryGetProperty("text", out var tx) ? tx.GetString() : "";
            string time = root.TryGetProperty("time", out var tm) ? tm.GetString()
                          : DateTime.Now.ToString("HH:mm");

            // Đưa toàn bộ phần đụng tới UI vào trong Invoke
            Dispatcher.Invoke(() =>
            {
                bool isSelf = user == txtUsername.Text;
                if (isSelf) return; // Chặn dội ngược tin nhắn

                AddChatBubble(user, text, time, isSelf);
            });
        }

        void HandleTyping(JsonElement root)
        {
            string user = root.TryGetProperty("username", out var u) ? u.GetString() : "";
            bool isTyping = root.TryGetProperty("isTyping", out var f) && f.GetBoolean();

            Dispatcher.Invoke(() =>
            {
                if (user == txtUsername.Text) return; // Bỏ qua nếu là chính mình

                if (isTyping)
                {
                    txtTypingIndicator.Text = $"✏ {user} đang soạn tin nhắn...";
                    txtTypingIndicator.Visibility = Visibility.Visible;
                }
                else
                {
                    txtTypingIndicator.Text = "";
                    txtTypingIndicator.Visibility = Visibility.Collapsed;
                }
            });
        }

        void HandleJoin(JsonElement root)
        {
            string user = root.TryGetProperty("username", out var u) ? u.GetString() : "?";
            Dispatcher.Invoke(() =>
            {
                _members[user] = false;
                RefreshMemberPanel();
                AddSystemMessage($"🟢 {user} đã tham gia phòng chat.");
            });
        }

        void HandleLeave(JsonElement root)
        {
            string user = root.TryGetProperty("username", out var u) ? u.GetString() : "?";
            Dispatcher.Invoke(() =>
            {
                _members.Remove(user);
                RefreshMemberPanel();
                AddSystemMessage($"🔴 {user} đã rời phòng chat.");
            });
        }

        void HandleMemberList(JsonElement root)
        {
            // Server can send full member list on join, e.g. {"type":"members","users":["A","B"]}
            Dispatcher.Invoke(() =>
            {
                _members.Clear();
                if (root.TryGetProperty("users", out var arr))
                    foreach (var item in arr.EnumerateArray())
                        _members[item.GetString()] = false;

                RefreshMemberPanel();
            });
        }

        // ======================== UI HELPERS ========================

        /// <summary>Adds a chat bubble (own = right-aligned blue, others = left-aligned grey).</summary>
        void AddChatBubble(string username, string text, string time, bool isSelf)
        {
            // Outer row
            var row = new Grid { Margin = new Thickness(0, 3, 0, 3) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = isSelf ? new GridLength(1, GridUnitType.Star) : GridLength.Auto });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = isSelf ? GridLength.Auto : new GridLength(1, GridUnitType.Star) });

            // Bubble container
            var bubbleCol = isSelf ? 1 : 0;
            var container = new StackPanel
            {
                Orientation = Orientation.Vertical,
                MaxWidth = 420,
                HorizontalAlignment = isSelf ? HorizontalAlignment.Right : HorizontalAlignment.Left
            };

            // Username + time header (only for others)
            if (!isSelf)
            {
                var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(6, 0, 0, 2) };
                header.Children.Add(new TextBlock
                {
                    Text = username,
                    Foreground = new SolidColorBrush(GetUserColor(username)),
                    FontWeight = FontWeights.SemiBold,
                    FontSize = 12
                });
                header.Children.Add(new TextBlock
                {
                    Text = "  " + time,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x6C, 0x70, 0x86)),
                    FontSize = 11,
                    VerticalAlignment = VerticalAlignment.Bottom
                });
                container.Children.Add(header);
            }

            // Bubble border
            var bubble = new Border
            {
                Background = new SolidColorBrush(isSelf
                    ? Color.FromRgb(0x89, 0xB4, 0xFA)   // blue for self
                    : Color.FromRgb(0x31, 0x32, 0x44)),  // grey for others
                CornerRadius = isSelf
                    ? new CornerRadius(16, 4, 16, 16)
                    : new CornerRadius(4, 16, 16, 16),
                Padding = new Thickness(12, 8, 12, 8)
            };

            var msgPanel = new StackPanel { Orientation = Orientation.Vertical };
            msgPanel.Children.Add(new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(isSelf
                    ? Color.FromRgb(0x1E, 0x1E, 0x2E)
                    : Color.FromRgb(0xCD, 0xD6, 0xF4)),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap
            });

            // Timestamp for own messages (shown inside bubble bottom-right)
            if (isSelf)
            {
                msgPanel.Children.Add(new TextBlock
                {
                    Text = time,
                    Foreground = new SolidColorBrush(Color.FromRgb(0x11, 0x11, 0x1B)),
                    FontSize = 10,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 2, 0, 0)
                });
            }

            bubble.Child = msgPanel;
            container.Children.Add(bubble);

            Grid.SetColumn(container, bubbleCol);
            row.Children.Add(container);

            var item = new ListBoxItem { Content = row, IsHitTestVisible = false };
            lstMessages.Items.Add(item);
            lstMessages.ScrollIntoView(item);
        }

        /// <summary>Adds a centered system message (join/leave/disconnect).</summary>
        void AddSystemMessage(string text)
        {
            var tb = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(Color.FromRgb(0x6C, 0x70, 0x86)),
                FontSize = 11,
                FontStyle = FontStyles.Italic,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 6, 0, 6),
                TextWrapping = TextWrapping.Wrap
            };
            var item = new ListBoxItem { Content = tb, IsHitTestVisible = false };
            lstMessages.Items.Add(item);
            lstMessages.ScrollIntoView(item);
        }

        /// <summary>Rebuilds the member sidebar list.</summary>
        void RefreshMemberPanel()
        {
            pnlMembers.Children.Clear();
            txtMemberCount.Text = $" {_members.Count}";
            txtOnlineStatus.Text = $"{_members.Count} trực tuyến";

            foreach (var kv in _members)
            {
                string name = kv.Key;
                bool isSelf = name == txtUsername.Text;

                var row = new Grid { Margin = new Thickness(0, 1, 0, 1) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // Avatar with dot
                var avatarGrid = new Grid { Width = 34, Height = 34, Margin = new Thickness(0, 0, 8, 0) };
                var circle = new Border
                {
                    Width = 30, Height = 30,
                    CornerRadius = new CornerRadius(15),
                    Background = new SolidColorBrush(GetUserColor(name)),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
                circle.Child = new TextBlock
                {
                    Text = name.Length > 0 ? name[0].ToString().ToUpper() : "?",
                    Foreground = Brushes.White,
                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                var dot = new Ellipse
                {
                    Width = 10, Height = 10,
                    Fill = new SolidColorBrush(Color.FromRgb(0xA6, 0xE3, 0xA1)),
                    Stroke = new SolidColorBrush(Color.FromRgb(0x18, 0x18, 0x25)),
                    StrokeThickness = 1.5,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom
                };
                avatarGrid.Children.Add(circle);
                avatarGrid.Children.Add(dot);
                Grid.SetColumn(avatarGrid, 0);

                var nameBlock = new TextBlock
                {
                    Text = isSelf ? $"{name} (Bạn)" : name,
                    Foreground = new SolidColorBrush(isSelf
                        ? Color.FromRgb(0x89, 0xB4, 0xFA)
                        : Color.FromRgb(0xCD, 0xD6, 0xF4)),
                    FontSize = 12,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };
                Grid.SetColumn(nameBlock, 1);

                row.Children.Add(avatarGrid);
                row.Children.Add(nameBlock);

                var wrapper = new Border
                {
                    Child = row,
                    Padding = new Thickness(8, 5, 8, 5),
                    CornerRadius = new CornerRadius(8),
                    Background = isSelf
                        ? new SolidColorBrush(Color.FromArgb(40, 0x89, 0xB4, 0xFA))
                        : Brushes.Transparent
                };
                pnlMembers.Children.Add(wrapper);
            }
        }

        // ======================== SEND ========================

        void SendMessage()
        {
            if (_writer == null) return;
            string text = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(text)) return;

            string time = DateTime.Now.ToString("HH:mm");
            string user = txtUsername.Text.Trim();
            if (string.IsNullOrEmpty(user)) user = "Ẩn danh";

            // Send to server
            SendJson(new { type = "message", username = user, text, time });

            // Show own bubble immediately (optimistic)
            AddChatBubble(user, text, time, isSelf: true);

            // Stop typing signal
            _isTyping = false;
            _typingTimer.Stop();
            SendJson(new { type = "typing", username = user, isTyping = false });

            txtMessage.Clear();
            txtMessage.Focus();
        }

        void SendJson(object payload)
        {
            try
            {
                string json = JsonSerializer.Serialize(payload);
                _writer?.WriteLine(json);
            }
            catch { /* ignore send errors */ }
        }

        // ======================== EVENT HANDLERS ========================

        private void btnSend_Click(object sender, RoutedEventArgs e) => SendMessage();

        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Keyboard.Modifiers == ModifierKeys.None)
            {
                e.Handled = true;
                SendMessage();
            }
        }

        private void txtMessage_TextChanged_Input(object sender, TextChangedEventArgs e)
        {
            if (_writer == null) return;
            if (!string.IsNullOrEmpty(txtMessage.Text))
            {
                if (!_isTyping)
                {
                    _isTyping = true;
                    SendJson(new { type = "typing", username = txtUsername.Text, isTyping = true });
                }
                _typingTimer.Stop();
                _typingTimer.Start();
            }
            else
            {
                if (_isTyping)
                {
                    _isTyping = false;
                    _typingTimer.Stop();
                    SendJson(new { type = "typing", username = txtUsername.Text, isTyping = false });
                }
            }
        }

        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            string name = txtUsername.Text;
            if (name.Length > 0)
                txtAvatarInitial.Text = name[0].ToString().ToUpper();
        }

        private void btnEmoji_Click(object sender, RoutedEventArgs e)
        {
            pnlEmojiPicker.Visibility = pnlEmojiPicker.Visibility == Visibility.Visible
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        // ======================== UTILITIES ========================

        /// <summary>Deterministic pastel color per username.</summary>
        static Color GetUserColor(string name)
        {
            // Cycle through a set of Catppuccin-style accent colors
            Color[] palette = {
                Color.FromRgb(0x89, 0xB4, 0xFA), // blue
                Color.FromRgb(0xA6, 0xE3, 0xA1), // green
                Color.FromRgb(0xF3, 0x8B, 0xA8), // red
                Color.FromRgb(0xFA, 0xB3, 0x87), // peach
                Color.FromRgb(0xF9, 0xE2, 0xAF), // yellow
                Color.FromRgb(0xCB, 0xA6, 0xF7), // mauve
                Color.FromRgb(0x89, 0xDC, 0xEB), // sky
                Color.FromRgb(0xA6, 0xD1, 0x89), // teal (approx)
            };
            int idx = Math.Abs(name.GetHashCode()) % palette.Length;
            return palette[idx];
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                SendJson(new { type = "leave", username = txtUsername.Text });
                _writer?.Close();
                _reader?.Close();
                _client?.Close();
            }
            catch { }
            base.OnClosed(e);
        }
        // ======================== LOGIN LOGIC ========================

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            JoinChat();
        }

        private void txtLoginName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                JoinChat();
            }
        }

        void JoinChat()
        {
            string name = txtLoginName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Vui lòng nhập tên của bạn!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Gán tên vào giao diện chính
            txtUsername.Text = name;
            
            // Ẩn màn hình đăng nhập đi
            pnlLoginOverlay.Visibility = Visibility.Collapsed;

            // BÂY GIỜ MỚI BẮT ĐẦU KẾT NỐI SERVER
            Connect();
        }
    }
}
