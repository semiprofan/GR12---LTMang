using System;
using System.Text.Json;

namespace ChatServer
{
    public class NetworkPacket
    {
        public string Type    { get; set; } = string.Empty;
        public string Sender  { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }
}
