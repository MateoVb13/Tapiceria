using APITapiceria.Data;
using APITapiceria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;

namespace APITapiceria.Controllers
{
    [Route("api/[controller]")] // La ruta base será /api/HorariosBaseAgendamiento
    [ApiController]
    public class HorariosBaseAgendamientoController : ControllerBase
    {
        private readonly TapiceriaContext _context;

        // Constructor para inyectar el contexto de la base de datos
        public HorariosBaseAgendamientoController(TapiceriaContext context)
        {
            _context = context;
        }

        // GET: api/HorariosBaseAgendamiento
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HorarioBaseAgendamientoDto>>> GetHorariosBase(
            [FromQuery] int diaSemana = 0,
            [FromQuery] bool soloActivos = false
        )
        {
            try
            {
                var query = _context.HorariosBaseAgendamiento.AsQueryable();

                // Aplicar filtros
                if (diaSemana > 0)
                {
                    query = query.Where(h => h.DiaSemana == diaSemana);
                }

                if (soloActivos)
                {
                    query = query.Where(h => h.Activo == true);
                }

                // Ordenar y mapear a DTO
                var horarios = await query
                    .OrderBy(h => h.DiaSemana)
                    .ThenBy(h => h.HoraInicio)
                    .Select(h => new HorarioBaseAgendamientoDto
                    {
                        IdHorarioBase = h.IdHorarioBase,
                        DiaSemana = h.DiaSemana,
                        HoraInicio = h.HoraInicio,
                        DuracionMinutos = h.DuracionMinutos,
                        IdServicio = h.IdServicio,
                        TipoDia = h.TipoDia,
                        Activo = h.Activo
                    })
                    .ToListAsync();

                return Ok(horarios);
            }
            catch (Exception ex)
            {
                // Log ex
                return StatusCode(500, "Error al obtener horarios base de agendamiento.");
            }
        }

        // GET: api/HorariosBaseAgendamiento/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<HorarioBaseAgendamientoDto>> GetHorarioBase(int id)
        {
            try
            {
                var horario = await _context.HorariosBaseAgendamiento
                    .Where(h => h.IdHorarioBase == id)
                    .Select(h => new HorarioBaseAgendamientoDto
                    {
                        IdHorarioBase = h.IdHorarioBase,
                        DiaSemana = h.DiaSemana,
                        HoraInicio = h.HoraInicio,
                        DuracionMinutos = h.DuracionMinutos,
                        IdServicio = h.IdServicio,
                        TipoDia = h.TipoDia,
                        Activo = h.Activo
                    })
                    .FirstOrDefaultAsync();

                if (horario == null) return NotFound();
                return Ok(horario);
            }
            catch (Exception ex)
            {
                // Log ex
                return StatusCode(500, "Error al obtener horario base por ID.");
            }
        }

        // POST: api/HorariosBaseAgendamiento
        [HttpPost]
        public async Task<ActionResult<HorarioBaseAgendamientoDto>> PostHorarioBase(CrearHorarioBaseAgendamientoDto horarioDto)
        {
            try
            {
                var nuevoHorario = new HorarioBaseAgendamiento
                {
                    DiaSemana = horarioDto.DiaSemana,
                    HoraInicio = horarioDto.HoraInicio,
                    DuracionMinutos = horarioDto.DuracionMinutos,
                    IdServicio = horarioDto.IdServicio, // Puede ser null
                    TipoDia = horarioDto.TipoDia,
                    Activo = horarioDto.Activo ?? true
                };

                _context.HorariosBaseAgendamiento.Add(nuevoHorario);
                await _context.SaveChangesAsync();

                // Mapear a DTO para la respuesta
                var horarioRespuesta = new HorarioBaseAgendamientoDto
                {
                    IdHorarioBase = nuevoHorario.IdHorarioBase,
                    DiaSemana = nuevoHorario.DiaSemana,
                    HoraInicio = nuevoHorario.HoraInicio,
                    DuracionMinutos = nuevoHorario.DuracionMinutos,
                    IdServicio = nuevoHorario.IdServicio,
                    TipoDia = nuevoHorario.TipoDia,
                    Activo = nuevoHorario.Activo
                };

                return CreatedAtAction(nameof(GetHorarioBase), new { id = nuevoHorario.IdHorarioBase }, horarioRespuesta);
            }
            catch (DbUpdateException ex)
            {
                // Verificar si es un error de duplicado (índice único)
                if (ex.InnerException?.Message.Contains("Duplicate entry") == true)
                {
                    return Conflict("Ya existe un horario base con el mismo día, hora y servicio.");
                }
                return StatusCode(500, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log ex
                return StatusCode(500, "Error al crear horario base de agendamiento.");
            }
        }

        // PUT: api/HorariosBaseAgendamiento/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHorarioBase(int id, HorarioBaseAgendamientoDto horarioDto)
        {
            if (id != horarioDto.IdHorarioBase)
                return BadRequest("El ID de la URL no coincide con el ID del horario base.");

            try
            {
                var horarioExistente = await _context.HorariosBaseAgendamiento.FindAsync(id);

                if (horarioExistente == null)
                {
                    return NotFound();
                }

                // Actualizar propiedades
                horarioExistente.DiaSemana = horarioDto.DiaSemana;
                horarioExistente.HoraInicio = horarioDto.HoraInicio;
                horarioExistente.DuracionMinutos = horarioDto.DuracionMinutos;
                horarioExistente.IdServicio = horarioDto.IdServicio;
                horarioExistente.TipoDia = horarioDto.TipoDia;
                horarioExistente.Activo = horarioDto.Activo ?? true;

                await _context.SaveChangesAsync();

                return NoContent(); // 204 No Content si es exitoso
            }
            catch (DbUpdateException ex)
            {
                // Verificar si es un error de duplicado
                if (ex.InnerException?.Message.Contains("Duplicate entry") == true)
                {
                    return Conflict("Ya existe un horario base con el mismo día, hora y servicio.");
                }
                return StatusCode(500, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log ex
                return StatusCode(500, "Error al actualizar horario base de agendamiento.");
            }
        }

        // DELETE: api/HorariosBaseAgendamiento/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHorarioBase(int id)
        {
            try
            {
                var horario = await _context.HorariosBaseAgendamiento.FindAsync(id);

                if (horario == null)
                {
                    return NotFound();
                }

                _context.HorariosBaseAgendamiento.Remove(horario);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al eliminar horario base de agendamiento.");
            }
        }
    }
}