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
    string username = txtUsername.Text.Trim();

    if (!string.IsNullOrEmpty(messageContent))
    {
        // 1. Tạo gói tin theo cấu trúc chuẩn chung của nhóm
        NetworkPacket packet = new NetworkPacket
        {
            Type = "CHAT",
            Sender = username,
            Content = messageContent
        };

        // 2. Chuyển đối tượng gói tin sang mảng byte nhờ hàm bổ trợ vừa sửa ở bước 1
        byte[] dataToSend = packet.Serialize();

        // 3. Gọi hàm truyền dữ liệu của đối tượng _clientLogic xuống tầng mạng Socket
        // Bạn thay hàm SendPacketAsync bằng tên hàm gửi mảng byte thực tế trong file ChatClientLogic.cs của bạn nhé
        await _clientLogic.SendPacketAsync(dataToSend); 

        // 4. Xóa sạch ô nhập văn bản để sẵn sàng cho tin nhắn tiếp theo
        txtMessage.Text = "";
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