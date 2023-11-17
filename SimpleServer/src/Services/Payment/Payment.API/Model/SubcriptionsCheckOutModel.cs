using System;

namespace Payment.API.Model
{
    public class SubcriptionsCheckOutModel
    {
        public string PlanType { get; set; } = string.Empty;

        public int Price { get; set; }
    }
}
