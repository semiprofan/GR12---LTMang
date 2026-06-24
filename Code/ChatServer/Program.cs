using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatServer
{
    class Program
    {
        private static TcpListener _listener;
        private static List<StreamWriter> _clientWriters = new List<StreamWriter>();

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            _listener = new TcpListener(IPAddress.Any, 5000);
            _listener.Start();
            Console.WriteLine("=== CHAT SERVER TCP ĐÃ KHỞI CHẠY TẠI PORT 5000 ===");

            try
            {
                while (true)
                {
                    TcpClient client = await _listener.AcceptTcpClientAsync();
                    _ = Task.Run(() => HandleClientAsync(client));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi Server: {ex.Message}");
            }
        }

        private static async Task HandleClientAsync(TcpClient client)
        {
            string clientInfo = client.Client.RemoteEndPoint.ToString();
            Console.WriteLine($"[+] Client kết nối mới từ: {clientInfo}");

            using NetworkStream stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            lock (_clientWriters)
            {
                _clientWriters.Add(writer);
            }

            try
            {
                while (true)
                {
                    string jsonReceived = await reader.ReadLineAsync();
                    if (jsonReceived == null) break;

                    try
                    {
                        // Parse JSON động bằng JsonDocument để không bị lỗi lệch kiểu dữ liệu (NetworkPacket)
                        using var doc = JsonDocument.Parse(jsonReceived);
                        var root = doc.RootElement;
                        string msgType = root.TryGetProperty("type", out var t) ? t.GetString() : "UNKNOWN";
                        
                        Console.WriteLine($"[Nhận] {jsonReceived}");

                        // Kiểm tra nếu là các gói tin hợp lệ từ Client thì tiến hành Broadcast
                        if (msgType == "message" || msgType == "join" || msgType == "leave" || msgType == "typing")
                        {
                            await BroadcastMessageAsync(jsonReceived);
                        }
                    }
                    catch 
                    { 
                        // Bỏ qua nếu gói tin nhận được bị lỗi định dạng JSON
                    }
                }
            }
            catch {}
            finally
            {
                lock (_clientWriters)
                {
                    _clientWriters.Remove(writer);
                }
                client.Close();
                Console.WriteLine($"[-] Client {clientInfo} đã rời phòng chat.");
            }
        }

        private static async Task BroadcastMessageAsync(string jsonMessage)
        {
            List<StreamWriter> targets;
            lock (_clientWriters)
            {
                targets = new List<StreamWriter>(_clientWriters);
            }

            foreach (var writer in targets)
            {
                try
                {
                    await writer.WriteLineAsync(jsonMessage);
                }
                catch {}
            }
        }
    }
}