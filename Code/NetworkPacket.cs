using System;

namespace ChatClient // Để namespace này giúp cả Client và Server đều nhận diện được
{
    public class NetworkPacket
    {
        // Các thuộc tính cơ bản
        public string Type { get; set; }     // Ví dụ: "CHAT", "LOGIN"
        public string Sender { get; set; }   // Tên người gửi
        public string Content { get; set; }  // Nội dung tin nhắn

        // --- THÀNH VIÊN 4 THÊM MỚI (TỐI ƯU HÓA) ---
        public DateTime Timestamp { get; set; } = DateTime.Now; // Lưu thời gian gửi tin
        public string MessageColor { get; set; } = "#FFFFFF";    // Màu sắc hiển thị mở rộng

        // Hàm tuần tự hóa: Chuyển đối tượng gói tin sang mảng byte nhờ JSON để gửi qua Socket
        public byte[] Serialize()
        {
            string jsonString = System.Text.Json.JsonSerializer.Serialize(this);
            return System.Text.Encoding.UTF8.GetBytes(jsonString);
        }

        // Hàm giải tuần tự hóa: Chuyển ngược mảng byte nhận được thành đối tượng gói tin
        public static NetworkPacket Deserialize(byte[] data)
        {
            string jsonString = System.Text.Encoding.UTF8.GetString(data);
            return System.Text.Json.JsonSerializer.Deserialize<NetworkPacket>(jsonString);
        }
    }
}