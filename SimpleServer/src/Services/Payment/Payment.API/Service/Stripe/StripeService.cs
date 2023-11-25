using System;
using Payment.API.Entity;
using Stripe.Checkout;

namespace Payment.API.Service.Stripe
{
    public class StripeService : IStripeService
    {
        private readonly ILogger<StripeService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public StripeService(ILogger<StripeService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> CheckOut(Subcription product)
        {
            try
            {
                // Get the base URL for the subscription
                var request = _httpContextAccessor.HttpContext?.Request ?? throw new Exception("Could not get request to checkout");
                var baseUrl = $"{request.Scheme}://{request.Host}";

                var options = new SessionCreateOptions
                {
                    // Stripe calls these user defined endpoints
                    SuccessUrl = $"{baseUrl}/subscription/success?sessionId={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = $"{baseUrl}/subscription/canceled",


                    PaymentMethodTypes = new List<string>
                    {
                        "card"
                    },
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Price = product.StripePriceId,
                            //PriceData = new SessionLineItemPriceDataOptions
                            //{
                            //    UnitAmount = product.Price * 100,
                            //    Currency = "usd",
                            //    ProductData = new SessionLineItemPriceDataProductDataOptions
                            //    {
                            //        Name = $"Subsciption Plan: {product.Plan}",
                            //        Description = $"Monthly price with Resolution: {product.Resolution}\n Quality:{product.VideoQuality}"
                            //    }
                            //},
                            Quantity = 1
                        }
                    },
                    Mode = "subscription",
                    SubscriptionData = new SessionSubscriptionDataOptions
                    {
                        BillingCycleAnchor = DateTime.UtcNow,
                    },
                    InvoiceCreation = new SessionInvoiceCreationOptions
                    {
                        Enabled = true,
                    },
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                return session.Id;
            }
            catch (System.Exception ex)
            {
                _logger.LogError("error into Stripe Service on CheckOut() " + ex.Message);
                throw;
            }
        }
    }
}
