using APITapiceria.Data;
using APITapiceria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        // Otros endpoints: GET, PUT, DELETE, etc.

        // POST: api/Citas/agendar
        [HttpPost("agendar")]
        public async Task<IActionResult> AgendarCita([FromBody] Citas cita)
        {
            if (cita == null)
                return BadRequest("Datos de la cita son inválidos.");

            // Validación básica: La fecha de inicio debe ser menor que la de fin
            if (cita.FechaInicio >= cita.FechaFin)
            {
                return BadRequest("La fecha de inicio debe ser anterior a la fecha de fin.");
            }

            // Aquí se podrían agregar otras validaciones, como verificar que no haya citas conflictivas para el cliente o el servicio

            _context.Cita.Add(cita);
            await _context.SaveChangesAsync();

            // Retornar el objeto creado y su URL para consultarlo (usando GetCita como referencia, el cual debe existir)
            return CreatedAtAction(nameof(GetCita), new { id = cita.IdCita }, cita);
        }

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
    }
}
