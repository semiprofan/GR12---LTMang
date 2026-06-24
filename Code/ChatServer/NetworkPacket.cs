using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace GR12___LTMang_main.Code // Thay đổi namespace này cho đúng với dự án của bạn nếu cần
{
    [Serializable] // Bắt buộc phải có thuộc tính này để ép kiểu mã hóa Binary
    public class NetworkPacket
    {
        public string Type { get; set; }       // "CONNECT", "CHAT", "DISCONNECT"
        public string Sender { get; set; }     // Tên người gửi tin nhắn
        public string Content { get; set; }    // Nội dung văn bản tin nhắn

        // --- HÀM BỔ TRỢ 1: Chuyển đổi từ Đối tượng sang mảng byte[] để gửi đi ---
        public byte[] Serialize()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, this);
                return ms.ToArray();
            }
        }

        // --- HÀM BỔ TRỢ 2: Chuyển đổi ngược từ mảng byte[] nhận được thành Đối tượng ---
        public static NetworkPacket Deserialize(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                BinaryFormatter bf = new BinaryFormatter();
                return (NetworkPacket)bf.Deserialize(ms);
            }
        }
    }
}