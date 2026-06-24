using System;
using System.Text.Json;

namespace ChatServer  // ← Đổi thành ChatServer để khớp với Program.cs
{
    public class NetworkPacket  // Bỏ [Serializable] và BinaryFormatter
    {
        public string Type    { get; set; } = string.Empty;
        public string Sender  { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}