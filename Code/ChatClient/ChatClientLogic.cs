using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatClient
{
    // 1. Định nghĩa cấu trúc gói tin để trao đổi với Server (Khánh)
    public class NetworkPacket
    {
        public string Type { get; set; } = string.Empty;       // "LOGIN", "CHAT_11", "LOGOUT"
        public string Sender { get; set; } = string.Empty;     // Tên người gửi
        public string Receiver { get; set; } = string.Empty;   // Tên người nhận (nếu chat 1-1)
        public string Content { get; set; } = string.Empty;    // Nội dung tin nhắn
    }

    // 2. Lớp xử lý Logic mạng Máy khách của Đạt
    public class ChatClientLogic
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private bool _isConnected;

        // Thêm dấu ? để cho phép sự kiện này có thể null khi chưa có UI đăng ký nhận
        public event Action<NetworkPacket>? OnPacketReceived;

        // Hàm kết nối đến Server bất đồng bộ
        public async Task<bool> ConnectAsync(string ipAddress, int port)
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync(ipAddress, port);

                _stream = _client.GetStream();
                _reader = new StreamReader(_stream, Encoding.UTF8);
                _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };

                _isConnected = true;

                // Tạo một luồng chạy ngầm để liên tục lắng nghe phản hồi từ Server
                _ = Task.Run(() => ListenFromServerAsync());

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể kết nối Server: {ex.Message}");
                return false;
            }
        }

        // Hàm gửi tin nhắn/gói tin dạng JSON lên Server
        public async Task SendPacketAsync(NetworkPacket packet)
        {
            if (!_isConnected || _writer == null) return;

            try
            {
                string jsonString = JsonSerializer.Serialize(packet);
                await _writer.WriteLineAsync(jsonString);
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        // Luồng chạy ẩn thu thập dữ liệu thời gian thực từ Server
        private async Task ListenFromServerAsync()
        {
            try
            {
                while (_isConnected && _reader != null)
                {
                    string? jsonResponse = await _reader.ReadLineAsync();
                    if (jsonResponse == null)
                    {
                        Disconnect();
                        break;
                    }

                    var packet = JsonSerializer.Deserialize<NetworkPacket>(jsonResponse);
                    if (packet != null)
                    {
                        OnPacketReceived?.Invoke(packet);
                    }
                }
            }
            catch (Exception)
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            if (!_isConnected) return;
            _isConnected = false;
            _reader?.Close();
            _writer?.Close();
            _stream?.Close();
            _client?.Close();
        }
    }
}