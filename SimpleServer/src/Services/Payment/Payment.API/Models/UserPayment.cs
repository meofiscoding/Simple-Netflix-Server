using System;

namespace Payment.API.Models
{
    public class UserPayment
    {
        public int Id { get; set; }

        public Subcription Subcription { get; set; } = new Subcription();

        public string UserId { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // set expire date to 1 month from now
        public DateTime ExpireDate { get; set; } = DateTime.UtcNow.AddMonths(1);

        public decimal Amount { get; set; }
    }
}
