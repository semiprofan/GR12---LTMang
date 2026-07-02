using System;

namespace ChatClient
{
    public class NetworkPacket
    {
        public string Type { get; set; }
        public string Sender { get; set; }
        public string Content { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string MessageColor { get; set; } = "#FFFFFF";

        public byte[] Serialize()
        {
            string jsonString = System.Text.Json.JsonSerializer.Serialize(this);
            return System.Text.Encoding.UTF8.GetBytes(jsonString);
        }

        public static NetworkPacket Deserialize(byte[] data)
        {
            string jsonString = System.Text.Encoding.UTF8.GetString(data);
            return System.Text.Json.JsonSerializer.Deserialize<NetworkPacket>(jsonString);
        }
    }
}
