using System;

namespace Application.ViewModel.Response
{
    public class PaymentResponse
    {
        public Guid Id { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? PaymentTime { get; set; }
        public decimal? AmountPaid { get; set; }
        public string? TransactionCode { get; set; }
        public string? Status { get; set; }
        public Guid? OrderId { get; set; }
        public Guid? SubscriptionId { get; set; }
        public Guid? userId { get; set; }
        public DateTime CreateDate { get; set; }
        
    }
} 