
using Payment.API.Enum;

namespace Payment.API.Entity
{
    public class Subcription
    {
        public int Id { get; set; }
        public string StripeProductId { get; set; } = string.Empty;
        public string StripePriceId { get; set; } = string.Empty;
        public PlanTypeEnum Plan { get; set; } = new();
        public int Price { get; set; }
        public QualityEnum VideoQuality { get; set; } = new();
        public string Resolution { get; set; } = string.Empty;
        public List<Device> Devices { get; set; } = new();
    }
}
