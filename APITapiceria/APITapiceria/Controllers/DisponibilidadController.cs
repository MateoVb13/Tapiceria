using APITapiceria.Data;
using APITapiceria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace APITapiceria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DisponibilidadController : ControllerBase
    {
        private readonly TapiceriaContext _context;

        public DisponibilidadController(TapiceriaContext context)
        {
            _context = context;
        }

        // GET: api/Disponibilidad?idServicio=1&fecha=2025-05-20
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetHorariosDisponibles(
            [FromQuery] int idServicio,
            [FromQuery] DateTime fecha)
        {
            try
            {
                if (idServicio <= 0)
                {
                    return BadRequest("El ID de servicio es obligatorio y debe ser un número válido");
                }

                var fechaSoloFecha = fecha.Date;

                var servicio = await _context.Servicios.FindAsync(idServicio);
                if (servicio == null)
                {
                    return NotFound("Servicio no encontrado");
                }

                int diaSemana = ((int)fechaSoloFecha.DayOfWeek == 0) ? 7 : (int)fechaSoloFecha.DayOfWeek;

                var horariosBase = await _context.HorariosBaseAgendamiento
                    .Where(hb => (hb.IdServicio == idServicio || hb.IdServicio == null) &&
                                hb.DiaSemana == diaSemana &&
                                hb.Activo == true)
                    .OrderBy(hb => hb.HoraInicio)
                    .ToListAsync();

                var disponibilidad = new List<object>();

                foreach (var horarioBase in horariosBase)
                {
                    var fechaHoraInicio = fechaSoloFecha.Add(horarioBase.HoraInicio);
                    var fechaHoraFin = fechaHoraInicio.AddMinutes(horarioBase.DuracionMinutos);

                    bool horarioOcupado = await _context.Citas
                        .AnyAsync(c => c.FechaInicio < fechaHoraFin && c.FechaFin > fechaHoraInicio);

                    if (!horarioOcupado)
                    {
                        var empleadoDisponible = await (from emp in _context.Empleados
                                                        join disp in _context.EmpleadoDisponibilidad
                                                        on emp.IdEmpleado equals disp.IdEmpleado
                                                        where disp.DiaSemana == diaSemana &&
                                                              disp.HoraInicio <= horarioBase.HoraInicio &&
                                                              disp.HoraFin >= horarioBase.HoraInicio.Add(TimeSpan.FromMinutes(horarioBase.DuracionMinutos))
                                                        select emp)
                                                       .FirstOrDefaultAsync();

                        if (empleadoDisponible != null)
                        {
                            disponibilidad.Add(new
                            {
                                FechaInicio = fechaHoraInicio,
                                FechaFin = fechaHoraFin,
                                IdEmpleado = empleadoDisponible.IdEmpleado,
                                NombreEmpleado = empleadoDisponible.NombreCompleto
                            });
                        }
                    }
                }

                return Ok(disponibilidad);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // GET: api/Disponibilidad/Horarios
        [HttpGet("Horarios")]
        public async Task<ActionResult<IEnumerable<HorarioBaseAgendamientoDto>>> GetHorariosBase(
            [FromQuery] int? diaSemana = null,
            [FromQuery] bool soloActivos = true)
        {
            try
            {
                var query = _context.HorariosBaseAgendamiento.AsQueryable();

                if (diaSemana.HasValue && diaSemana.Value > 0)
                {
                    query = query.Where(h => h.DiaSemana == diaSemana.Value);
                }

                if (soloActivos)
                {
                    query = query.Where(h => h.Activo == true);
                }

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
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/Disponibilidad/Horarios
        [HttpPost("Horarios")]
        public async Task<ActionResult> CrearHorarioBase([FromBody] HorarioBaseAgendamientoDto horario)
        {
            try
            {
                if (horario == null)
                {
                    return BadRequest("Datos de horario inválidos");
                }

                if (horario.DiaSemana < 1 || horario.DiaSemana > 7)
                {
                    return BadRequest("El día de la semana debe estar entre 1 y 7");
                }

                if (horario.IdServicio.HasValue && horario.IdServicio.Value > 0)
                {
                    var servicio = await _context.Servicios.FindAsync(horario.IdServicio.Value);
                    if (servicio == null)
                    {
                        return BadRequest("El servicio especificado no existe");
                    }
                }
                else
                {
                    horario.IdServicio = null;
                }

                var nuevoHorario = new HorarioBaseAgendamiento
                {
                    DiaSemana = horario.DiaSemana,
                    HoraInicio = horario.HoraInicio,
                    DuracionMinutos = horario.DuracionMinutos,
                    IdServicio = horario.IdServicio,
                    TipoDia = horario.TipoDia,
                    Activo = horario.Activo ?? true
                };

                _context.HorariosBaseAgendamiento.Add(nuevoHorario);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException?.Message.Contains("Duplicate entry") == true)
                    {
                        return Conflict("Ya existe un horario base con el mismo día, hora y servicio");
                    }
                    throw;
                }

                horario.IdHorarioBase = nuevoHorario.IdHorarioBase;

                return CreatedAtAction(nameof(GetHorariosBase), new { id = horario.IdHorarioBase }, horario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // PUT: api/Disponibilidad/Horarios/{id}
        [HttpPut("Horarios/{id}")]
        public async Task<ActionResult> ActualizarHorarioBase(int id, [FromBody] HorarioBaseAgendamientoDto horario)
        {
            try
            {
                if (horario == null)
                {
                    return BadRequest("Datos de horario inválidos");
                }

                var horarioExistente = await _context.HorariosBaseAgendamiento.FindAsync(id);
                if (horarioExistente == null)
                {
                    return NotFound("Horario base no encontrado");
                }

                if (horario.DiaSemana < 1 || horario.DiaSemana > 7)
                {
                    return BadRequest("El día de la semana debe estar entre 1 y 7");
                }

                if (horario.IdServicio.HasValue && horario.IdServicio.Value > 0)
                {
                    var servicio = await _context.Servicios.FindAsync(horario.IdServicio.Value);
                    if (servicio == null)
                    {
                        return BadRequest("El servicio especificado no existe");
                    }
                }
                else
                {
                    horario.IdServicio = null;
                }

                horarioExistente.DiaSemana = horario.DiaSemana;
                horarioExistente.HoraInicio = horario.HoraInicio;
                horarioExistente.DuracionMinutos = horario.DuracionMinutos;
                horarioExistente.IdServicio = horario.IdServicio;
                horarioExistente.TipoDia = horario.TipoDia;
                horarioExistente.Activo = horario.Activo ?? horarioExistente.Activo;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException?.Message.Contains("Duplicate entry") == true)
                    {
                        return Conflict("Ya existe un horario base con el mismo día, hora y servicio");
                    }
                    throw;
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // DELETE: api/Disponibilidad/Horarios/{id}
        [HttpDelete("Horarios/{id}")]
        public async Task<ActionResult> EliminarHorarioBase(int id)
        {
            try
            {
                var horario = await _context.HorariosBaseAgendamiento.FindAsync(id);
                if (horario == null)
                {
                    return NotFound("Horario base no encontrado");
                }

                _context.HorariosBaseAgendamiento.Remove(horario);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // POST: api/Disponibilidad/Horarios/Rango
        [HttpPost("Horarios/Rango")]
        public async Task<ActionResult> CrearHorariosBaseRango([FromBody] CrearHorarioBaseAgendamientoDto rango)
        {
            try
            {
                if (rango == null)
                {
                    return BadRequest("Datos de rango inválidos");
                }

                if (rango.DiaSemana < 1 || rango.DiaSemana > 7)
                {
                    return BadRequest("El día de la semana debe estar entre 1 y 7");
                }

                if (rango.HoraInicio >= rango.HoraFin)
                {
                    return BadRequest("La hora de inicio debe ser anterior a la hora de fin");
                }

                if (rango.DuracionSlotMinutos <= 0)
                {
                    return BadRequest("La duración del slot debe ser mayor a 0 minutos");
                }

                if (rango.IdServicio.HasValue && rango.IdServicio.Value > 0)
                {
                    var servicio = await _context.Servicios.FindAsync(rango.IdServicio.Value);
                    if (servicio == null)
                    {
                        return BadRequest("El servicio especificado no existe");
                    }
                }
                else
                {
                    rango.IdServicio = null;
                }

                var horariosCreados = 0;
                var horaActual = rango.HoraInicio;

                while (horaActual < rango.HoraFin)
                {
                    var existe = await _context.HorariosBaseAgendamiento
                        .AnyAsync(h => h.DiaSemana == rango.DiaSemana &&
                                      h.HoraInicio == horaActual &&
                                      h.IdServicio == rango.IdServicio);

                    if (!existe)
                    {
                        var nuevoHorario = new HorarioBaseAgendamiento
                        {
                            DiaSemana = rango.DiaSemana,
                            HoraInicio = horaActual,
                            DuracionMinutos = rango.DuracionSlotMinutos,
                            IdServicio = rango.IdServicio,
                            TipoDia = rango.TipoDia,
                            Activo = rango.Activo ?? true
                        };

                        _context.HorariosBaseAgendamiento.Add(nuevoHorario);
                        horariosCreados++;
                    }

                    horaActual = horaActual.Add(TimeSpan.FromMinutes(rango.DuracionSlotMinutos));
                }

                if (horariosCreados > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    Mensaje = $"Se crearon {horariosCreados} horarios base para el día {rango.DiaSemana}",
                    HorariosCreados = horariosCreados
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}