using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatClient
{
    // Định nghĩa duy nhất cho gói tin mạng
    public class NetworkPacket
    {
        public string Type { get; set; } = string.Empty;       
        public string Sender { get; set; } = string.Empty;     
        public string Receiver { get; set; } = string.Empty;   
        public string Content { get; set; } = string.Empty;    
    }

    public class ChatClientLogic
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private StreamReader? _reader;
        private StreamWriter? _writer;
        private bool _isConnected;

        public event Action<NetworkPacket>? OnPacketReceived;

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
                _ = Task.Run(() => ListenFromServerAsync());

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Không thể kết nối Server: {ex.Message}");
                return false;
            }
        }

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