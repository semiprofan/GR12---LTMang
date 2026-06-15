using System;
using System.Windows;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        // Khởi tạo lớp logic mạng của Đạt
        private ChatClientLogic _clientLogic = new ChatClientLogic();

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Đang thử kết nối thử nghiệm đến Server của Khánh...");

            // Giả lập kết nối đến IP máy Localhost ở Port 5000
            bool isConnected = await _clientLogic.ConnectAsync("127.0.0.1", 5000);

            if (isConnected)
            {
                MessageBox.Show("Kết nối mạng TCP thành công rực rỡ!");
                
                var loginPacket = new NetworkPacket
                {
                    Type = "LOGIN",
                    Sender = "DatDao",
                    Content = "MatKhau123"
                };
                await _clientLogic.SendPacketAsync(loginPacket);
            }
            else
            {
                MessageBox.Show("Kết nối thất bại. Do hiện tại chưa bật Server của Khánh.");
            }
        }
    }
} // Hãy chắc chắn có dấu ngoặc này để đóng class