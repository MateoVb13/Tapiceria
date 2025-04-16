using APITapiceria.Data;
using APITapiceria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace APITapiceria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagosController : ControllerBase
    {
        private readonly TapiceriaContext _context;
        public PagosController(TapiceriaContext context)
        {
            _context = context;
        }

        // GET: api/Pagos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pagos>>> GetPagos()
        {
            return await _context.Pagos
                .Include(p => p.Cita)
                .ToListAsync();
        }

        // GET: api/Pagos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Pagos>> GetPago(int id)
        {
            var pago = await _context.Pagos
                .Include(p => p.Cita)
                .FirstOrDefaultAsync(p => p.IdPago == id);

            if (pago == null)
                return NotFound();

            return pago;
        }

        // POST: api/Pagos
        [HttpPost]
        public async Task<ActionResult<Pagos>> PostPago(Pagos pago)
        {
            _context.Pagos.Add(pago);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPago), new { id = pago.IdPago }, pago);
        }

        // PUT: api/Pagos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPago(int id, Pagos pago)
        {
            if (id != pago.IdPago)
                return BadRequest();

            _context.Entry(pago).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Pagos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePago(int id)
        {
            var pago = await _context.Pagos.FindAsync(id);
            if (pago == null)
                return NotFound();

            _context.Pagos.Remove(pago);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
