using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Payment.API.Data;
using Payment.API.Entity;
using Payment.API.Mapper;
using Payment.API.Model;

namespace Payment.API.Controllers
{
    [Route("api/")]
    [ApiController]
    public class SubcriptionController : ControllerBase
    {
        private readonly PaymentDBContext _context;

        public SubcriptionController(PaymentDBContext context)
        {
            _context = context;
        }

        // GET: api/pricingPlans
        [HttpGet("pricingPlans")]
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

        // POST: api/subscription
        [HttpPost("subscription")]
        public async Task<ActionResult<Subcription>> PostSubcription([FromBody] int planId)
        {
            var subcription = await _context.Subcriptions.FindAsync(planId);

            return subcription ?? (ActionResult<Subcription>)NotFound();
        }
    }
}
