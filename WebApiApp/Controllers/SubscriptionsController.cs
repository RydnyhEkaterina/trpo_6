using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiApp.Models;

namespace WebApiApp.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public SubscriptionsController(AppDbContext db)
        {
            _db = db;
        }
        // GET /api/v1/Subscriptions?active=true&maxPrice=1000
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Subscription>>> GetAll(
            [FromQuery] bool? active,
            [FromQuery] decimal? maxPrice)
        {
            var query = _db.Subscriptions.AsQueryable();

            if (active.HasValue)
                query = query.Where(s => s.IsActive == active.Value);

            if (maxPrice.HasValue)
                query = query.Where(s => s.Price <= maxPrice.Value);

            return Ok(await query.ToListAsync()); // 200 OK
        }

        // GET /api/v1/Subscriptions/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Subscription>> GetById([FromRoute] int id)
        {
            var sub = await _db.Subscriptions.FindAsync(id);
            if (sub == null) return NotFound(); // 404 Not Found
            return Ok(sub); // 200 OK
        }

        // POST /api/v1/Subscriptions
        [HttpPost]
        public async Task<ActionResult<Subscription>> Create([FromBody] Subscription sub)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState); // 400 Bad Request

            _db.Subscriptions.Add(sub);
            await _db.SaveChangesAsync();

            // 201 Created + заголовок Location
            return CreatedAtAction(nameof(GetById), new { id = sub.Id }, sub);
        }

        // PUT /api/v1/Subscriptions/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] Subscription sub)
        {
            if (id != sub.Id) return BadRequest("ID не совпадает."); // 400 Bad Request

            _db.Entry(sub).State = EntityState.Modified;

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _db.Subscriptions.AnyAsync(e => e.Id == id)) return NotFound(); // 404
                throw;
            }

            return NoContent(); // 204 No Content
        }

        // PATCH /api/v1/Subscriptions/{id}
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch([FromRoute] int id, [FromBody] Subscription updated)
        {
            var sub = await _db.Subscriptions.FindAsync(id);
            if (sub == null) return NotFound(); // 404 Not Found

            // Частичное обновление
            if (!string.IsNullOrEmpty(updated.Name)) sub.Name = updated.Name;
            if (updated.Price != 0) sub.Price = updated.Price;
            if (updated.DurationDays != 0) sub.DurationDays = updated.DurationDays;
            if (!string.IsNullOrEmpty(updated.Description)) sub.Description = updated.Description;
            if (updated.IsActive != sub.IsActive) sub.IsActive = updated.IsActive;

            await _db.SaveChangesAsync();
            return NoContent(); // 204 No Content
        }

        // DELETE /api/v1/Subscriptions/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var sub = await _db.Subscriptions.FindAsync(id);
            if (sub == null) return NotFound(); // 404 Not Found

            _db.Subscriptions.Remove(sub);
            await _db.SaveChangesAsync();

            return NoContent(); // 204 No Content
        }
    }
}