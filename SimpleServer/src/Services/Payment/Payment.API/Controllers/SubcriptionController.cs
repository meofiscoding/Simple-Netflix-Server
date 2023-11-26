using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payment.API.Data;
using Payment.API.Entity;
using Payment.API.GrpcService;
using Payment.API.Mapper;
using Payment.API.Model;
using Payment.API.Service.Stripe;
using Stripe;
using Stripe.Checkout;

namespace Payment.API.Controllers
{
    [ApiController]
    public class SubcriptionController : ControllerBase
    {
        private readonly PaymentDBContext _context;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SubcriptionController> _logger;
        private readonly IStripeService _stripeService;
        private readonly PaymentGrpcService _paymentGrpcService;

        public SubcriptionController(PaymentDBContext context, IConfiguration config, IHttpContextAccessor httpContextAccessor, ILogger<SubcriptionController> logger, IStripeService stripeService, PaymentGrpcService paymentGrpcService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _stripeService = stripeService;
            _paymentGrpcService = paymentGrpcService;
            _config = config;
        }

        // GET: api/pricingPlans
        [HttpGet("api/pricingPlans")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IEnumerable<Subcriptions>>> GetSubcriptions()
        {
            if (_context.Subcriptions == null)
            {
                return NotFound();
            }
            // return a list of subcriptions
            return await _context.Subcriptions
                .ProjectTo<Subcriptions>(new MapperConfiguration(cfg => cfg.AddProfile<SubcriptionProfile>()))
                .ToListAsync();
        }

        // GET: api/pricingPlan/5
        [HttpGet("api/pricingPlan/{id}")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<SubcriptionsCheckOutModel>> GetSubcription(int id)
        {
            var subcription = await _context.Subcriptions.FindAsync(id) ?? throw new Exception("Plan not found");
            return new SubcriptionsCheckOutModel()
            {
                PlanType = subcription.Plan.ToString(),
                Price = subcription.Price
            };
        }

        // POST: api/subscription
        [HttpPost("api/subscription")]
        [Authorize(Roles = "User")]
        public async Task<Results<Ok<string>, BadRequest>> PostSubcription([FromBody] int planId)
        {
            try
            {
                var subcription = await _context.Subcriptions.FindAsync(planId) ?? throw new Exception("Plan not found");
                // get current userId
                var userId = _httpContextAccessor.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type.Contains("nameidentifier"))?.Value;
                var userEmail = _httpContextAccessor.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type.Contains("emailaddress"))?.Value;
                var sessionId = await _stripeService.CheckOut(subcription, userEmail);
                return TypedResults.Ok(sessionId);
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error when checkout due to: {ex.Message}");
                return TypedResults.BadRequest();
            }
        }

        //[HttpPost("api/create-payment-intent")]
        //[Authorize(Roles = "User")]
        //public async Task<IActionResult> CreatePaymentIntent([FromBody] int amount)
        //{
        //    var options = new PaymentIntentCreateOptions
        //    {
        //        Amount = amount * 100,
        //        Currency = "usd",
        //        PaymentMethodTypes = new List<string>
        //        {
        //            "card",
        //        }
        //    };

        //    var service = new PaymentIntentService();
        //    var paymentIntent = await service.CreateAsync(options);

        //    return Ok(paymentIntent);
        //}

        // Customer Portal
        [HttpPost]
        [Route("api/create-portal-session")]
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> CreatePortalSession([FromBody] string userEmail)
        {
            // Sets up a Billing Portal configuration
            var options = new Stripe.BillingPortal.ConfigurationCreateOptions
            {
                BusinessProfile = new Stripe.BillingPortal.ConfigurationBusinessProfileOptions
                {
                    Headline = "Simple Netflix partners with Stripe for simplified billing.",
                },
                Features = new Stripe.BillingPortal.ConfigurationFeaturesOptions
                {
                    InvoiceHistory = new Stripe.BillingPortal.ConfigurationFeaturesInvoiceHistoryOptions
                    {
                        Enabled = true,
                    },
                },
            };
            var service = new Stripe.BillingPortal.ConfigurationService();
            await service.CreateAsync(options);

            var customer = await _stripeService.GetCustomerByEmail(userEmail);
            var sessionServiceOptions = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = customer.Id,
                ReturnUrl = $"{_config["ClientUrl"]}/account",
            };
            var sessionService = new Stripe.BillingPortal.SessionService();
            var session = await sessionService.CreateAsync(sessionServiceOptions);

            return Ok(session);
        }

        // POST: subscription/success
        //[HttpPost("subscription/success")]
        //public async Task<IActionResult> CheckoutSuccess([FromBody] int planId)
        //{
        //    // get subcription
        //    Subcription Subcription = await _context.Subcriptions.FindAsync(planId);
        //    if (Subcription == null)
        //    {
        //        return NotFound();
        //    }

        //    // get current userId
        //    var userId = _httpContextAccessor.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type.Contains("nameidentifier"))?.Value;
        //    if (userId == null)
        //    {
        //        return Unauthorized();
        //    }
        //    // Communicate with IdentityGrpcService to update user membership
        //    var response = await _paymentGrpcService.UpdateUserMembership(userId, true);

        //    // TODO: save user payment to database
        //    var userPayment = new UserPayment()
        //    {
        //        Subcription = Subcription,
        //        UserId = userId,
        //    };
        //    _context.UserPayments.Add(userPayment);
        //    await _context.SaveChangesAsync();

        //    return Ok();
        //}

        [HttpGet("subscription/success")]
        public async Task<Results<RedirectHttpResult, BadRequest>> CheckoutSuccess([FromQuery] string sessionId)
        {
            try
            {
                var service = new SessionService();
                SessionGetOptions options = new SessionGetOptions
                {
                    Expand = new List<string> { "line_items" }
                };
                var sessionInfo = service.Get(sessionId, options);
                var priceId = sessionInfo.LineItems.Data.FirstOrDefault()?.Price.Id
                    ?? throw new Exception("PriceId not found");

                var subcription = _context.Subcriptions.FirstOrDefault(x => x.StripePriceId == priceId)
                    ?? throw new Exception("Subscription not found");

                var userEmail = sessionInfo.CustomerEmail;

                // Communicate with IdentityGrpcService to update user membership
                var response = await _paymentGrpcService.UpdateUserMembership(userEmail, true);
                var userPayment = new UserPayment()
                {
                    Subcription = subcription,
                    UserId = response.UserId,
                };

                // TODO: Uncomment when need to serve explicit service based on user subscription
                // _context.UserPayments.Add(userPayment);
                // await _context.SaveChangesAsync();
                // return a redirect to the front end success page
                return TypedResults.Redirect($"{_config["ClientUrl"]}/subscription/success");
            }
            catch (Exception ex)
            {
                _logger.LogError("error into order Controller on route /canceled " + ex.Message);
                return TypedResults.BadRequest();
            }
        }

        [Route("stripe/webhook")]
        [HttpPost]
        public async Task<Results<RedirectHttpResult, BadRequest>> StripeWebHook()
        {
            // read webhook secret from configuration
            string endpointSecret = _config["Stripe:WebhookSecret"]
                ?? throw new Exception("Stripe:WebhookSecret not found");
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], endpointSecret);

                // Handle the event
                if (stripeEvent.Type == Events.CustomerSubscriptionUpdated)
                {
                    // If cancel_at_period_end is true, the subscription is canceled at the end of its billing period.
                    if (stripeEvent.Data.Object is Subscription subscription)
                    {
                        if (subscription.CancelAtPeriodEnd == true)
                        {
                            // Communicate with IdentityGrpcService to update user membership
                            var response = await _paymentGrpcService.UpdateUserMembership(subscription.Customer.Email, false);
                            // redirect to the front end cancel page
                            return TypedResults.Redirect($"{_config["ClientUrl"]}/subscription/cancel");
                        }
                    }
                }

                // If user update email information
                if (stripeEvent.Type == Events.CustomerUpdated)
                {
                    if (stripeEvent.Data.Object is Customer customer)
                    {
                        // Communicate with IdentityGrpcService to update user membership
                        var response = await _paymentGrpcService.UpdateUserEmail(customer.Email);
                    }
                }
                else
                {
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("error into order Controller on route /webhook " + ex.Message);
            }
            return TypedResults.BadRequest();
        }

        [HttpGet("subscription/cancel")]
        public async Task<Results<RedirectHttpResult, BadRequest>> CheckoutCanceled([FromQuery] string sessionId)
        {
            try
            {
                var service = new SessionService();
                SessionGetOptions options = new SessionGetOptions
                {
                    Expand = new List<string> { "line_items" }
                };
                var sessionInfo = service.Get(sessionId, options);
                var priceId = sessionInfo.LineItems.Data.FirstOrDefault()?.Price.Id
                    ?? throw new Exception("PriceId not found");

                var subcription = _context.Subcriptions.FirstOrDefault(x => x.StripePriceId == priceId)
                    ?? throw new Exception("Subscription not found");

                var userEmail = sessionInfo.CustomerEmail;

                // Communicate with IdentityGrpcService to update user membership
                var response = await _paymentGrpcService.UpdateUserMembership(userEmail, false);
                var userPayment = new UserPayment()
                {
                    Subcription = subcription,
                    UserId = response.UserId,
                };

                return TypedResults.Redirect($"{_config["ClientUrl"]}/payment/planform");
            }
            catch (Exception ex)
            {
                _logger.LogError("error into order Controller on route /canceled " + ex.Message);
                return TypedResults.BadRequest();
            }
        }
    }
}
