using Microsoft.EntityFrameworkCore;
using Payment.API.Data;
using Payment.API.Enum;
using Payment.API.Entity;
using Polly;
using Stripe;

namespace Payment.API
{
    public static class SeedData
    {
        public static async Task InitializeDatabase(IApplicationBuilder app)
        {
            var service = new ProductService();
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
                    context.SaveChanges();
                }

                if (!context.Qualities.Any())
                {
                    context.Qualities.AddRange(
                        System.Enum.GetValues(typeof(QualityEnum)).Cast<QualityEnum>().Select(x => new Quality
                        {
                            Name = x.ToString()
                        }));
                    context.SaveChanges();
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
                                    Name = nameof(DeviceEnum.Mobile)
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
                    context.SaveChanges();
                }

                var subcriptions = context.Subcriptions.ToList();
                // check if there are any product on stripe
                var products = service.List(new ProductListOptions { Limit = 100 })
                    // select all available products
                    .Where(x => x.Active == true);

                if (!products.Any())
                {
                    foreach (var subcription in subcriptions)
                    {
                        var product = service.Create(new ProductCreateOptions
                        {
                            DefaultPriceData = new ProductDefaultPriceDataOptions
                            {
                                Currency = "usd",
                                UnitAmount = subcription.Price * 100,
                                Recurring = new ProductDefaultPriceDataRecurringOptions
                                {
                                    Interval = "month"
                                },
                            },
                            Name = subcription.Plan.ToString(),
                        });
                    }
                }

                // Fetch products from Stripe
                products = service.List(new ProductListOptions { Limit = 100 })
                    // select all available products
                    .Where(x => x.Active == true);
                // get all subcriptions to create product on stripe
                for (int i = 0; i < products.Count(); i++)
                {
                    var product = products.ElementAt(i);
                    if (i < subcriptions.Count)
                    {
                        subcriptions[i].StripeProductId = product.Id;
                        subcriptions[i].StripePriceId = product.DefaultPriceId;
                        context.Subcriptions.Update(subcriptions[i]);
                        context.SaveChanges();
                    }
                }

                return Task.CompletedTask;
            });
        }

        private static int GenerateRandomPrice(int multiplier)
        {
            Random random = new();
            int basePrice = random.Next(multiplier, 10);
            return basePrice * multiplier;
        }
    }
}
