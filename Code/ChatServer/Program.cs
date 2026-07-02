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
        private static readonly Dictionary<StreamWriter, string> _activeUsers = new Dictionary<StreamWriter, string>();
        
        private static readonly List<string> _messageHistory = new List<string>();

        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            var listener = new TcpListener(IPAddress.Any, 5000);
            listener.Start();
            Console.WriteLine("=== CHAT SERVER TCP ĐÃ KHỞI CHẠY TẠI PORT 5000 ===");

            try
            {
                while (true)
                {
                    TcpClient client = await listener.AcceptTcpClientAsync();
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
            string clientInfo = client.Client.RemoteEndPoint?.ToString() ?? "unknown endpoint";
            Console.WriteLine($"[+] Client kết nối mới từ: {clientInfo}");

            using NetworkStream stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.UTF8);
            using var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            lock (_activeUsers)
            {
                _activeUsers[writer] = "Ẩn danh";
            }

            try
            {
                while (true)
                {
                    string? jsonReceived = await reader.ReadLineAsync();
                    if (jsonReceived == null) break;

                    try
                    {
                        using var doc = JsonDocument.Parse(jsonReceived);
                        var root = doc.RootElement;
                        string msgType = GetStringOrDefault(root, "type", "UNKNOWN");

                        if (msgType == "join")
                        {
                            string username = GetStringOrDefault(root, "username", "Ẩn danh");
                            lock (_activeUsers)
                            {
                                _activeUsers[writer] = username;
                            }

                            List<string> historyCopy;
                            lock (_messageHistory)
                            {
                                historyCopy = new List<string>(_messageHistory);
                            }
                            foreach (var oldMsg in historyCopy)
                            {
                                await writer.WriteLineAsync(oldMsg);
                            }

                            await BroadcastMessageAsync(jsonReceived);

                            await BroadcastMemberListAsync();
                        }
                        else if (msgType == "message")
                        {
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
                            break;
                        }
                        else if (msgType == "typing")
                        {
                            await BroadcastMessageAsync(jsonReceived);
                        }
                    }
                    catch
                    {
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

        private static string GetStringOrDefault(JsonElement root, string propertyName, string defaultValue)
        {
            if (!root.TryGetProperty(propertyName, out var property))
                return defaultValue;

            return property.GetString() ?? defaultValue;
        }
    }
}
