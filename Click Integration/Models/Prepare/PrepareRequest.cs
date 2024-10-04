using System.Text.Json.Serialization;

namespace Click_Integration.Models.Prepare
{
    public class PrepareRequest
    {
        [JsonPropertyName("click_trans_id")]
        public long ClickTransId { get; set; }  // CLICK tizimidagi tranzaksiya ID (to'lovga urinish)

        [JsonPropertyName("service_id")]
        public int ServiceId { get; set; }      // Xizmatning ID raqami

        [JsonPropertyName("click_paydoc_id")]
        public long ClickPaydocId { get; set; } // CLICK tizimidagi to'lov ID raqami (SMSda ko'rsatiladi)

        [JsonPropertyName("merchant_trans_id")]
        public string MerchantTransId { get; set; } // Online do'kondagi buyurtma ID yoki shaxsiy kabinet

        [JsonPropertyName("amount")]
        public decimal Amount { get; set; }     // To'lov summasi (so'mda)

        [JsonPropertyName("action")]
        public int Action { get; set; } = 0;    // Harakat turi, bu bosqichda 0

        [JsonPropertyName("error")]
        public int Error { get; set; }          // To'lov holati (0 – muvaffaqiyatli, xatolik bo'lsa xato kodi)

        [JsonPropertyName("error_note")]
        public string ErrorNote { get; set; }   // Xato kodi izohi

        [JsonPropertyName("sign_time")]
        public string SignTime { get; set; }    // To'lov vaqti (format: "YYYY-MM-DD HH:mm:ss")

        [JsonPropertyName("sign_string")]
        public string SignString { get; set; }  // MD5 hash orqali tasdiqlash (click_trans_id + service_id + SECRET_KEY + ...)
    }
}
