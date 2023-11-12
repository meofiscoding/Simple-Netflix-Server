using Microsoft.EntityFrameworkCore;
using Payment.API.Data;
using Payment.API.Enum;
using Payment.API.Models;
using Polly;

namespace Payment.API
{
    public static class SeedData
    {
        public static async Task InitializeDatabase(IApplicationBuilder app)
        {
            var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()?.CreateScope() ?? throw new Exception("Could not create scope");
            var context = serviceScope.ServiceProvider.GetRequiredService<PaymentDBContext>();
            var retry = Policy
                // handle posgresql exception
                .Handle<Exception>()
                .WaitAndRetryAsync(new TimeSpan[]
                {
                        TimeSpan.FromSeconds(3),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(8),
                });
            await retry.ExecuteAsync(() =>
            {
                context.Database.Migrate();
                if (!context.PricingPlans.Any())
                {
                    // add all element of enum to database
                    context.PricingPlans.AddRange(
                        System.Enum.GetValues(typeof(PlanTypeEnum)).Cast<PlanTypeEnum>().Select(x => new PlanType
                        {
                            Name = x.ToString()
                        }));
                }

                if (!context.Qualities.Any())
                {
                    context.Qualities.AddRange(
                        System.Enum.GetValues(typeof(QualityEnum)).Cast<QualityEnum>().Select(x => new Quality
                        {
                            Name = x.ToString()
                        }));
                }

                if (!context.Subcriptions.Any())
                {
                    context.Subcriptions.AddRange(
                        new Subcription
                        {
                            Plan = PlanTypeEnum.Mobile,
                            Price = GenerateRandomPrice((int)PlanTypeEnum.Mobile),
                            VideoQuality = QualityEnum.Good,
                            Resolution = Consts.RESOLUTION_480P,
                            Devices = new List<Device>
                            {
                                new Device
                                {
                                    Name = nameof(DeviceEnum.Phone)
                                }
                            }
                        },
                        new Subcription
                        {
                            Plan = PlanTypeEnum.Basic,
                            Price = GenerateRandomPrice((int)PlanTypeEnum.Basic),
                            VideoQuality = QualityEnum.Good,
                            Resolution = Consts.RESOLUTION_720P,
                            Devices = System.Enum.GetValues(typeof(DeviceEnum))
                                .Cast<DeviceEnum>()
                                .Select(x => new Device
                                {
                                    Name = x.ToString()
                                }).ToList()
                        },
                        new Subcription
                        {
                            Plan = PlanTypeEnum.Standard,
                            Price = GenerateRandomPrice((int)PlanTypeEnum.Standard),
                            VideoQuality = QualityEnum.Better,
                            Resolution = Consts.RESOLUTION_1080P,
                            Devices = System.Enum.GetValues(typeof(DeviceEnum))
                                .Cast<DeviceEnum>()
                                .Select(x => new Device
                                {
                                    Name = x.ToString()
                                }).ToList()
                        },
                        new Subcription
                        {
                            Plan = PlanTypeEnum.Premium,
                            Price = GenerateRandomPrice((int)PlanTypeEnum.Premium),
                            VideoQuality = QualityEnum.Best,
                            Resolution = Consts.RESOLUTION_4K,
                            Devices = System.Enum.GetValues(typeof(DeviceEnum))
                                .Cast<DeviceEnum>()
                                .Select(x => new Device
                                {
                                    Name = x.ToString()
                                }).ToList()
                        });
                }
                context.SaveChanges();
                return Task.CompletedTask;
            });
        }

        private static decimal GenerateRandomPrice(int multiplier)
        {
            Random random = new();
            int basePrice = random.Next(1, 10);
            return basePrice * multiplier;
        }
    }
}
