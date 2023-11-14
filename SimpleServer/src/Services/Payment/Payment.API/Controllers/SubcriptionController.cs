using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payment.API.Data;
using Payment.API.Entity;
using Payment.API.Mapper;
using Payment.API.Model;
using Payment.API.Service.Stripe;
using Stripe.Checkout;

namespace Payment.API.Controllers
{
    [Route("api/")]
    [ApiController]
    [Authorize]
    public class SubcriptionController : ControllerBase
    {
        private readonly PaymentDBContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SubcriptionController> _logger;
        private readonly IStripeService _stripeService;
        private readonly string _frontendSuccessUrl;
        private readonly string _frontendCanceledUrl;

        public SubcriptionController(PaymentDBContext context, IHttpContextAccessor httpContextAccessor, ILogger<SubcriptionController> logger, IStripeService stripeService, IConfiguration configuration)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _stripeService = stripeService;
            var request = _httpContextAccessor.HttpContext!.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            _frontendSuccessUrl = baseUrl + "/orders/success";
            _frontendCanceledUrl = baseUrl + "/orders/canceled";
        }

        // GET: api/pricingPlans
        [HttpGet("pricingPlans")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult<IEnumerable<Subcriptions>>> GetSubcriptions()
        {
            if (_context.Subcriptions == null)
            {
                return NotFound();
            }
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

            // return a list of subcriptions
            return await _context.Subcriptions
                .ProjectTo<Subcriptions>(new MapperConfiguration(cfg => cfg.AddProfile<SubcriptionProfile>()))
                .ToListAsync();
        }

        // POST: api/subscription
        [HttpPost("subscription")]
        [Authorize(Roles = "User")]
        public async Task<Results<Ok<string>, BadRequest>> PostSubcription([FromBody] int planId)
        {
            var subcription = await _context.Subcriptions.FindAsync(planId) ?? throw new Exception("Plan not found");

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

        // POST: api/subscription/success
        /// <summary>
        /// this API is going to be hit when order is placed successfully @Stripe
        /// </summary>
        /// <returns>A redirect to the front end success page</returns>
        [HttpGet("subscription/success")]
        [Authorize(Roles = "User")]
        public async Task<Results<RedirectHttpResult, BadRequest>> CheckoutSuccess([FromQuery] string sessionId)
        {
            try
            {
                var sessionService = new SessionService();
                var session = sessionService.Get(sessionId);


                //var total = session.AmountTotal.Value; <- total from Stripe side also
                //var customerEmail = session.CustomerDetails.Email;
                // TODO: save user payment to database
                // CustomerModel customer = new CustomerModel(session.Id, session.CustomerDetails.Name, session.CustomerDetails.Email, session.CustomerDetails.Phone);

                // Save the customer details to your database.
                // await _customerData.InsertCustomerInDb(customer);

                return TypedResults.Redirect(_frontendSuccessUrl, true, true);
            }
            catch (Exception ex)
            {
                _logger.LogError("error into order Controller on route /success " + ex.Message);
                return TypedResults.BadRequest();
            }
        }

        /// <summary>
        /// this API is going to be hit when order is a failure
        /// </summary>
        /// <returns>A redirect to the front end success page</returns>
        [HttpGet("canceled")]
        [Authorize(Roles = "User")]
        public async Task<Results<RedirectHttpResult, BadRequest>> CheckoutCanceled([FromQuery] string sessionId)
        {
            try
            {
                // Insert here failure data in data base
                return TypedResults.Redirect(_frontendCanceledUrl, true, true);
            }
            catch (Exception ex)
            {
                _logger.LogError("error into order Controller on route /canceled " + ex.Message);
                return TypedResults.BadRequest();
            }
        }
    }
}
