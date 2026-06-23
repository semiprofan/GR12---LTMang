using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        TcpClient client;
        NetworkStream stream;

        public MainWindow()
        {
            InitializeComponent();
            Connect();
        }

        // ================= CONNECT SERVER =================
        void Connect()
        {
            try
            {
                client = new TcpClient();
                client.Connect("127.0.0.1", 5000);

                stream = client.GetStream();

                Thread t = new Thread(Receive);
                t.IsBackground = true;
                t.Start();
            }
            catch
            {
                MessageBox.Show("Không kết nối được Server!");
            }
        }

        // ================= NHẬN TIN NHẮN =================
        void Receive()
        {
            byte[] buffer = new byte[1024];

            while (true)
            {
                try
                {
                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    if (bytes <= 0) break;

                    string msg = Encoding.UTF8.GetString(buffer, 0, bytes);

                    Dispatcher.Invoke(() =>
                    {
                        lstMessages.Items.Add(msg);
                    });
                }
                catch
                {
                    break;
                }
            }
        }

        // ================= GỬI TIN NHẮN =================
        void SendMessage()
        {
            try
            {
                string msg = txtUsername.Text + ": " + txtMessage.Text;

                byte[] data = Encoding.UTF8.GetBytes(msg);
                stream.Write(data, 0, data.Length);

                lstMessages.Items.Add(msg);

                txtMessage.Clear();
            }
            catch
            {
                MessageBox.Show("Gửi thất bại!");
            }
        }

        // ================= ENTER =================
        private void txtMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        // ================= BUTTON SEND =================
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }
    }
}