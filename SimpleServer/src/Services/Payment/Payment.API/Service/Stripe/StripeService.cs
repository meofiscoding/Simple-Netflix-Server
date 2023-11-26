using System;
using Payment.API.Entity;
using Stripe;
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

        public async Task<string> CheckOut(Subcription product, string userEmail)
        {
            try
            {
                // Get the base URL for the subscription
                //var request = _httpContextAccessor.HttpContext?.Request ?? throw new Exception("Could not get request to checkout");
                //var baseUrl = $"{request.Scheme}://{request.Host}";
                var baseUrl = "https://localhost:5000";
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
                            Price = "price_1OGGiEHlvWn5zVy0jA2kj4bC",
                            Quantity = 1
                        }
                    },
                    Mode = "subscription",
                    CustomerEmail = userEmail

                    // SubscriptionData = new SessionSubscriptionDataOptions
                    // {
                    //     BillingCycleAnchor = DateTime.UtcNow,
                    // },
                    // InvoiceCreation = new SessionInvoiceCreationOptions
                    // {
                    //     Enabled = true,
                    // },
                };

                var service = new SessionService();
                Session session = await service.CreateAsync(options);

                return session.Id;
            }
            catch (System.Exception ex)
            {
                _logger.LogError("error into Stripe Service on CheckOut() " + ex.Message);
                throw;
            }
        }

        // Get customer info by email
        public async Task<Customer> GetCustomerByEmail(string email)
        {
            try
            {
                var service = new CustomerService();
                var customer = await service.ListAsync(new CustomerListOptions
                {
                    Email = email,
                    Limit = 1,
                });

                return customer.FirstOrDefault() ?? throw new Exception("Customer not found!");
            }
            catch (System.Exception ex)
            {
                _logger.LogError("error into Stripe Service on GetCustomerByEmail() " + ex.Message);
                throw;
            }
        }
    }
}
