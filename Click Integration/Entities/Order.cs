namespace Click_Integration.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public EOrderPaymentStatus PaymentStatus { get; set; }
        public DateTime CreatedDate { get; set; }

        // yana ko'p
        // maydonlar
        // bo'lishi mumkin

    }
}
