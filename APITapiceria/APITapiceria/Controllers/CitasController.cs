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
    public class CitasController : ControllerBase
    {
        private readonly TapiceriaContext _context;
        public CitasController(TapiceriaContext context)
        {
            _context = context;
        }

        // GET: api/Citas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Citas>>> GetCitas()
        {
            return await _context.Cita
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .Include(c => c.Empleado)
                .ToListAsync();
        }

        // GET: api/Citas/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Citas>> GetCita(int id)
        {
            var cita = await _context.Cita
                .Include(c => c.Cliente)
                .Include(c => c.Servicio)
                .Include(c => c.Empleado)
                .FirstOrDefaultAsync(c => c.IdCita == id);

            if (cita == null)
                return NotFound();

            return cita;
        }

        // POST: api/Citas
        [HttpPost]
        public async Task<ActionResult<Citas>> PostCita(Citas cita)
        {
            _context.Cita.Add(cita);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCita), new { id = cita.IdCita }, cita);
        }

        // PUT: api/Citas/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCita(int id, Citas cita)
        {
            if (id != cita.IdCita)
                return BadRequest();

            _context.Entry(cita).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Citas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCita(int id)
        {
            var cita = await _context.Cita.FindAsync(id);
            if (cita == null)
                return NotFound();

            _context.Cita.Remove(cita);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
