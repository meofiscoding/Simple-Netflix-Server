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
    [Route("api/[controller]")]
    [ApiController]
    public class SubcriptionController : ControllerBase
    {
        private readonly PaymentDBContext _context;

        public SubcriptionController(PaymentDBContext context)
        {
            _context = context;
        }

        // GET: api/Subcription
        [HttpGet]
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

        // GET: api/Subcription/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Subcription>> GetSubcription(int id)
        {
          if (_context.Subcriptions == null)
          {
              return NotFound();
          }
            var subcription = await _context.Subcriptions.FindAsync(id);

            if (subcription == null)
            {
                return NotFound();
            }

            return subcription;
        }

        // PUT: api/Subcription/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSubcription(int id, Subcription subcription)
        {
            if (id != subcription.Id)
            {
                return BadRequest();
            }

            _context.Entry(subcription).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SubcriptionExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Subcription
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Subcription>> PostSubcription(Subcription subcription)
        {
          if (_context.Subcriptions == null)
          {
              return Problem("Entity set 'PaymentDBContext.Subcriptions'  is null.");
          }
            _context.Subcriptions.Add(subcription);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSubcription", new { id = subcription.Id }, subcription);
        }

        // DELETE: api/Subcription/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubcription(int id)
        {
            if (_context.Subcriptions == null)
            {
                return NotFound();
            }
            var subcription = await _context.Subcriptions.FindAsync(id);
            if (subcription == null)
            {
                return NotFound();
            }

            _context.Subcriptions.Remove(subcription);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SubcriptionExists(int id)
        {
            return (_context.Subcriptions?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
