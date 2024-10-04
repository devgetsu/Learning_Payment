using System.Text.Json.Serialization;

namespace Click_Integration.Models.Prepare
{
    public class PrepareResponse
    {
        [JsonPropertyName("click_trans_id")]
        public long ClickTransId { get; set; }  // CLICK tizimidagi to'lov ID raqami

        [JsonPropertyName("merchant_trans_id")]
        public string MerchantTransId { get; set; } // Online do'kondagi buyurtma ID

        [JsonPropertyName("merchant_prepare_id")]
        public int MerchantPrepareId { get; set; }  // Billing tizimidagi to'lov ID

        [JsonPropertyName("error")]
        public int Error { get; set; }          // To'lov holati (0 – muvaffaqiyatli, xato kodi bo'lsa, qaytadi)

        [JsonPropertyName("error_note")]
        public string ErrorNote { get; set; }   // Xato kodi izohi
    }

}
