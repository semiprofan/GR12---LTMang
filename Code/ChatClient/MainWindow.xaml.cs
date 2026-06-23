using System;
using System.Windows;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        private ChatClientLogic _clientLogic = new ChatClientLogic();

        public MainWindow()
        {
            InitializeComponent();
            _clientLogic.OnPacketReceived += ClientLogic_OnPacketReceived;
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Kết nối tới Server (Hãy chắc chắn Server đã được chạy trước)
            bool isConnected = await _clientLogic.ConnectAsync("127.0.0.1", 5000);
            if (!isConnected)
            {
                MessageBox.Show("Kết nối thất bại. Hãy chắc chắn bạn đã bật Server ở Terminal!");
            }
        }

        // Xử lý khi nhận được tin nhắn từ Server truyền về

        // Xử lý sự kiện khi nhấn nút "Gửi tin"
        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            string messageContent = txtMessage.Text.Trim();
            
            // LẤY TÊN LINH HOẠT: Đọc trực tiếp từ ô txtUsername bạn vừa tạo ở file XAML
            string username = txtUsername.Text.Trim();

            // Kiểm tra nếu người dùng xóa sạch tên thì tự đặt là "AnDanh"
            if (string.IsNullOrEmpty(username))
            {
                username = "AnDanh";
            }

            // Kiểm tra nội dung tin nhắn trống
            if (string.IsNullOrEmpty(messageContent))
            {
                MessageBox.Show("Vui lòng nhập nội dung tin nhắn!");
                return;
            }

            try
            {
                // Đóng gói gói tin chứa Username linh hoạt thay vì gán chết cố định
                var chatPacket = new NetworkPacket
                {
                    Type = "CHAT",
                    Sender = username, 
                    Content = messageContent
                };

                // Gửi gói tin đi qua luồng TCP
                await _clientLogic.SendPacketAsync(chatPacket);
                
                // Xóa nội dung ở ô nhập tin nhắn để sẵn sàng gõ câu tiếp theo
                txtMessage.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi gửi tin: {ex.Message}");
            }
        }
        // 1. Thêm hàm xử lý khi nhấn phím Enter trên ô nhập tin nhắn
        private void txtMessage_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                // Tự động kích hoạt sự kiện nhấn nút Gửi tin
                btnSend_Click(this, new RoutedEventArgs());
            }
        }

        // 2. Bạn tìm đến hàm ClientLogic_OnPacketReceived sẵn có và chèn thêm dòng cuộn chữ này vào cuối:
        private void ClientLogic_OnPacketReceived(NetworkPacket packet)
        {
            Dispatcher.Invoke(() =>
            {
                if (packet.Type == "CHAT")
                {
                    lstMessages.Items.Add($"{packet.Sender}: {packet.Content}");
                    
                    // Giúp tự động cuộn xuống dòng tin nhắn cuối cùng vừa nhận
                    if (lstMessages.Items.Count > 0)
                    {
                        lstMessages.ScrollIntoView(lstMessages.Items[lstMessages.Items.Count - 1]);
                    }
                }
            });
        }
    }
}