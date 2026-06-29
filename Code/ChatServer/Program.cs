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
        
        // Dùng Dictionary để map StreamWriter với Username của Client đó
        private static Dictionary<StreamWriter, string> _activeUsers = new Dictionary<StreamWriter, string>();
        
        // TÍNH NĂNG MỚI: List lưu trữ lịch sử tin nhắn
        private static List<string> _messageHistory = new List<string>();

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

            // Tạm đăng ký Client mới với tên "Ẩn danh"
            lock (_activeUsers)
            {
                _activeUsers[writer] = "Ẩn danh";
            }

            try
            {
                while (true)
                {
                    string jsonReceived = await reader.ReadLineAsync();
                    if (jsonReceived == null) break;

                    try
                    {
                        using var doc = JsonDocument.Parse(jsonReceived);
                        var root = doc.RootElement;
                        string msgType = root.TryGetProperty("type", out var t) ? t.GetString() : "UNKNOWN";

                        if (msgType == "join")
                        {
                            string username = root.TryGetProperty("username", out var u) ? u.GetString() : "Ẩn danh";
                            lock (_activeUsers)
                            {
                                _activeUsers[writer] = username;
                            }

                            // 1. ĐỒNG BỘ LỊCH SỬ: Gửi toàn bộ tin nhắn cũ cho riêng người mới vào
                            List<string> historyCopy;
                            lock (_messageHistory)
                            {
                                historyCopy = new List<string>(_messageHistory);
                            }
                            foreach (var oldMsg in historyCopy)
                            {
                                await writer.WriteLineAsync(oldMsg);
                            }

                            // 2. Báo cho phòng chat có người mới join
                            await BroadcastMessageAsync(jsonReceived);

                            // 3. Cập nhật lại số lượng và danh sách thanh Sidebar
                            await BroadcastMemberListAsync();
                        }
                        else if (msgType == "message")
                        {
                            // LƯU LỊCH SỬ: Add tin nhắn vào List (Giới hạn 50 tin nhắn để nhẹ RAM)
                            lock (_messageHistory)
                            {
                                _messageHistory.Add(jsonReceived);
                                if (_messageHistory.Count > 50)
                                    _messageHistory.RemoveAt(0);
                            }
                            await BroadcastMessageAsync(jsonReceived);
                        }
                        else if (msgType == "leave")
                        {
                            break; // Thoát vòng lặp để xuống khối finally xử lý
                        }
                        else if (msgType == "typing")
                        {
                            await BroadcastMessageAsync(jsonReceived);
                        }
                    }
                    catch
                    {
                        // Bỏ qua nếu lỗi parse JSON
                    }
                }
            }
            catch {}
            finally
            {
                string username = "Ẩn danh";
                lock (_activeUsers)
                {
                    if (_activeUsers.TryGetValue(writer, out var name))
                    {
                        username = name;
                    }
                    _activeUsers.Remove(writer);
                }
                
                client.Close();
                Console.WriteLine($"[-] Client {clientInfo} ({username}) đã rời phòng chat.");

                // Thông báo có người rời đi và cập nhật lại Sidebar
                var leavePayload = JsonSerializer.Serialize(new { type = "leave", username = username });
                await BroadcastMessageAsync(leavePayload);
                await BroadcastMemberListAsync();
            }
        }

        private static async Task BroadcastMessageAsync(string jsonMessage)
        {
            List<StreamWriter> targets;
            lock (_activeUsers)
            {
                targets = new List<StreamWriter>(_activeUsers.Keys);
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

        private static async Task BroadcastMemberListAsync()
        {
            string jsonList;
            List<StreamWriter> targets;

            lock (_activeUsers)
            {
                var usernames = new List<string>(_activeUsers.Values);
                var payload = new { type = "members", users = usernames };
                jsonList = JsonSerializer.Serialize(payload);
                targets = new List<StreamWriter>(_activeUsers.Keys);
            }

            foreach (var writer in targets)
            {
                try
                {
                    await writer.WriteLineAsync(jsonList);
                }
                catch {}
            }
        }
    }
}