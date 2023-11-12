using System;

namespace Payment.API.Model
{
    public class Subcriptions
    {
        public int Id { get; set; }
        public string PlanType { get; set; } = string.Empty;

        public string Price { get; set; } = string.Empty;

        public string VideoQuality { get; set; } = string.Empty;

        public string Resolution { get; set; } = string.Empty;

        public List<string> Devices { get; set; } = new();
    }
}
