using System.Text.Json.Serialization;

namespace Click_Integration.Models.Complate
{
    public class CompleteResponse
    {
        [JsonPropertyName("click_trans_id")]
        public long ClickTransId { get; set; }  // CLICK tizimidagi to'lov ID

        [JsonPropertyName("merchant_trans_id")]
        public string MerchantTransId { get; set; } // Online do'kondagi buyurtma ID

        [JsonPropertyName("merchant_confirm_id")]
        public int? MerchantConfirmId { get; set; } // Billing tizimidagi to'lovni tasdiqlash ID (bo'lishi mumkin)

        [JsonPropertyName("error")]
        public int Error { get; set; }          // To'lov holati (0 – muvaffaqiyatli, xato bo'lsa xato kodi)

        [JsonPropertyName("error_note")]
        public string ErrorNote { get; set; }   // Xato kodi izohi
    }
}
