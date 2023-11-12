
using Payment.API.Enum;

namespace Payment.API.Models
{
    public class Subcription
    {
        public int Id { get; set; }
        public PlanTypeEnum Plan { get; set; } = new();
        public decimal Price { get; set; }
        public QualityEnum VideoQuality { get; set; } = new();
        public string Resolution { get; set; } = string.Empty;
        public List<Device> Devices { get; set; } = new();
    }
}
