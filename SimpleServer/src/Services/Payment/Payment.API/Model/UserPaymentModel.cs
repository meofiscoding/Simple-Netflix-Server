using System;

namespace Payment.API.Model
{
    public class UserPaymentModel
    {
        public string UserId { get; set; } = string.Empty;
        public int PricingPlanId { get; set; }
    }
}
