namespace PropertyManagement.API.DTOs
{
    public class MarkPaidRequest
    {
        public string? TransactionReference { get; set; }
        public string? PaymentMethod { get; set; }
    }

    public class PaymentStatistics
    {
        public int TotalPayments { get; set; }
        public int PaidPayments { get; set; }
        public int PendingPayments { get; set; }
        public int OverduePayments { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal YearlyRevenue { get; set; }
        public decimal TotalOutstanding { get; set; }
        
        public decimal CollectionRate => TotalPayments > 0 ? (decimal)PaidPayments / TotalPayments * 100 : 0;
        public decimal OverdueRate => TotalPayments > 0 ? (decimal)OverduePayments / TotalPayments * 100 : 0;
    }
}
