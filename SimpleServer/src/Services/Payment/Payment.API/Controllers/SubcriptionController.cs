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
    [Authorize(Roles = "User")]
    public class SubcriptionController : ControllerBase
    {
        private readonly PaymentDBContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SubcriptionController> _logger;
        private readonly IStripeService _stripeService;
        private readonly PaymentGrpcService _paymentGrpcService;

        public SubcriptionController(PaymentDBContext context, IHttpContextAccessor httpContextAccessor, ILogger<SubcriptionController> logger, IStripeService stripeService, PaymentGrpcService paymentGrpcService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _stripeService = stripeService;
            _paymentGrpcService = paymentGrpcService;
        }

        // GET: api/pricingPlans
        [HttpGet("api/pricingPlans")]
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
        public async Task<Results<Ok<string>, BadRequest>> PostSubcription([FromBody] UserPaymentModel model)
        {
            var subcription = await _context.Subcriptions.FindAsync(model.PricingPlanId) ?? throw new Exception("Plan not found");
            // store userId and pricingPlanId temporarily to use in checkout success
            try
            {
                var sessionId = await _stripeService.CheckOut(subcription);
                return TypedResults.Ok(sessionId);
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error when checkout due to: {ex.Message}");
                return TypedResults.BadRequest();
            }
        }

        [HttpPost("api/create-payment-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] int amount)
        {
            var options = new PaymentIntentCreateOptions
            {
                Amount = amount * 100,
                Currency = "usd",
                PaymentMethodTypes = new List<string>
                {
                    "card",
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            return Ok(paymentIntent);
        }

        // POST: subscription/success
        [HttpPost("subscription/success")]
        public async Task<IActionResult> CheckoutSuccess([FromBody] int planId)
        {
            // get subcription
            Subcription Subcription = await _context.Subcriptions.FindAsync(planId);
            if (Subcription == null)
            {
                return NotFound();
            }

            // get current userId
            var userId = _httpContextAccessor.HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type.Contains("nameidentifier"))?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            // Communicate with IdentityGrpcService to update user membership
            var response = await _paymentGrpcService.UpdateUserMembership(userId, true);

            // TODO: save user payment to database
            var userPayment = new UserPayment()
            {
                Subcription = Subcription,
                UserId = userId,
            };
            _context.UserPayments.Add(userPayment);
            await _context.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// this API is going to be hit when order is a failure
        /// </summary>
        /// <returns>A redirect to the front end success page</returns>
        // [HttpGet("subscription/canceled")]
        // public async Task<Results<RedirectHttpResult, BadRequest>> CheckoutCanceled([FromQuery] string sessionId)
        // {
        //     try
        //     {
        //         // Insert here failure data in data base

        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError("error into order Controller on route /canceled " + ex.Message);
        //         return TypedResults.BadRequest();
        //     }
        // }
    }
}
