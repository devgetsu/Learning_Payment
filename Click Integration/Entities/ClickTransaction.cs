namespace Click_Integration.Entities
{
    public class ClickTransaction
    {
        public int Id { get; set; }
        public long ClickTransId { get; set; }
        public string MerchantTransId { get; set; }
        public decimal Amount { get; set; }
        public DateTime SignTime { get; set; }
        public EOrderPaymentStatus Status { get; set; }
    }
}
