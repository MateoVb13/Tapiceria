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

        private async Task<List<T>> ExecuteSelectProcedure<T>(string procedureName, Func<MySqlDataReader, T> mapFunction, params MySqlParameter[] parameters)
        {
            List<T> results = new List<T>();
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();
            using (var command = connection.CreateCommand())
            {
                command.CommandText = procedureName;
                command.CommandType = CommandType.StoredProcedure;
                if (parameters != null) command.Parameters.AddRange(parameters);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    // Convertir DbDataReader a MySqlDataReader de forma segura
                    if (reader is MySqlDataReader mySqlReader)
                    {
                        while (await mySqlReader.ReadAsync())
                        {
                            results.Add(mapFunction(mySqlReader));
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("No se pudo convertir DbDataReader a MySqlDataReader");
                    }
                }
            }
            return results;
        }

        // GET: api/Disponibilidad?idServicio=1&fecha=2025-05-20
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetHorariosDisponibles(
            [FromQuery] int idServicio,
            [FromQuery] DateTime fecha)
        {
            try
            {
                // Validar datos de entrada
                if (idServicio <= 0)
                {
                    return BadRequest("El ID de servicio es obligatorio y debe ser un número válido");
                }

                // Asegurarse de que la fecha es válida y sin componente de tiempo
                var fechaSoloFecha = fecha.Date;

                // Verificar que el servicio existe
                var servicio = await _context.Servicios.FindAsync(idServicio);
                if (servicio == null)
                {
                    return NotFound("Servicio no encontrado");
                }

                // Parámetros para el procedimiento almacenado
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("p_IdServicio", idServicio),
                    new MySqlParameter("p_Fecha", fechaSoloFecha)
                };

                // Usar el procedimiento almacenado para obtener la disponibilidad completa
                var disponibilidad = await ExecuteSelectProcedure(
                    "ObtenerDisponibilidadCompleta",
                    reader => new
                    {
                        FechaInicio = reader.GetDateTime("FechaInicio"),
                        FechaFin = reader.GetDateTime("FechaFin"),
                        IdEmpleado = reader.GetInt32("IdEmpleado"),
                        NombreEmpleado = reader.GetString("NombreEmpleado")
                    },
                    parameters
                );

                // Si no hay resultados, devolver lista vacía
                if (!disponibilidad.Any())
                {
                    return Ok(new List<object>());
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
                string query = @"
                    SELECT 
                        IdHorarioBase, 
                        DiaSemana,
                        HoraInicio,
                        DuracionMinutos,
                        IdServicio,
                        TipoDia,
                        Activo
                    FROM horarios_base_agendamiento
                    WHERE (@DiaSemana IS NULL OR DiaSemana = @DiaSemana)
                      AND (@SoloActivos = 0 OR Activo = 1)
                    ORDER BY DiaSemana, HoraInicio";

                var parameters = new[]
                {
                    new MySqlParameter("@DiaSemana", diaSemana.HasValue ? diaSemana.Value : DBNull.Value),
                    new MySqlParameter("@SoloActivos", soloActivos)
                };

                var horarios = new List<HorarioBaseAgendamientoDto>();
                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open) await connection.OpenAsync();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = query;
                    command.Parameters.AddRange(parameters);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Convertir TIME a TimeSpan manualmente
                            TimeSpan horaInicio = reader.IsDBNull(2) ? TimeSpan.Zero :
                                TimeSpan.Parse(reader.GetValue(2).ToString());

                            horarios.Add(new HorarioBaseAgendamientoDto
                            {
                                IdHorarioBase = reader.GetInt32(0),
                                DiaSemana = reader.GetInt32(1),
                                HoraInicio = horaInicio,
                                DuracionMinutos = reader.GetInt32(3),
                                IdServicio = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                                TipoDia = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Activo = reader.IsDBNull(6) ? true : reader.GetBoolean(6)
                            });
                        }
                    }
                }

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
                // Validar entrada
                if (horario == null)
                {
                    return BadRequest("Datos de horario inválidos");
                }

                // Validar día de la semana
                if (horario.DiaSemana < 1 || horario.DiaSemana > 7)
                {
                    return BadRequest("El día de la semana debe estar entre 1 y 7");
                }

                // Validar ID de servicio si se proporcionó
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
                    // Si es null o 0, asignar explícitamente null
                    horario.IdServicio = null;
                }

                // Crear el horario base
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
                    // Verificar si es un error de duplicado
                    if (ex.InnerException?.Message.Contains("Duplicate entry") == true)
                    {
                        return Conflict("Ya existe un horario base con el mismo día, hora y servicio");
                    }
                    throw;
                }

                // Asignar el ID generado al DTO para devolver
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
                // Validar entrada
                if (horario == null)
                {
                    return BadRequest("Datos de horario inválidos");
                }

                // Buscar el horario existente
                var horarioExistente = await _context.HorariosBaseAgendamiento.FindAsync(id);
                if (horarioExistente == null)
                {
                    return NotFound("Horario base no encontrado");
                }

                // Validar día de la semana
                if (horario.DiaSemana < 1 || horario.DiaSemana > 7)
                {
                    return BadRequest("El día de la semana debe estar entre 1 y 7");
                }

                // Validar ID de servicio si se proporcionó
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
                    // Si es null o 0, asignar explícitamente null
                    horario.IdServicio = null;
                }

                // Actualizar propiedades
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
                    // Verificar si es un error de duplicado
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
                // Validar entrada
                if (rango == null)
                {
                    return BadRequest("Datos de rango inválidos");
                }

                // Validar día de la semana
                if (rango.DiaSemana < 1 || rango.DiaSemana > 7)
                {
                    return BadRequest("El día de la semana debe estar entre 1 y 7");
                }

                // Validar horas
                if (rango.HoraInicio >= rango.HoraFin)
                {
                    return BadRequest("La hora de inicio debe ser anterior a la hora de fin");
                }

                // Validar duración
                if (rango.DuracionSlotMinutos <= 0)
                {
                    return BadRequest("La duración del slot debe ser mayor a 0 minutos");
                }

                // Validar ID de servicio si se proporcionó
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
                    // Si es null o 0, asignar explícitamente null
                    rango.IdServicio = null;
                }

                // Parámetros para el procedimiento almacenado
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("p_DiaSemana", rango.DiaSemana),
                    new MySqlParameter("p_HoraInicio", rango.HoraInicio),
                    new MySqlParameter("p_HoraFin", rango.HoraFin),
                    new MySqlParameter("p_DuracionSlotMinutos", rango.DuracionSlotMinutos),
                    new MySqlParameter("p_IdServicio", rango.IdServicio.HasValue ? (object)rango.IdServicio.Value : DBNull.Value),
                    new MySqlParameter("p_TipoDia", rango.TipoDia ?? (object)DBNull.Value),
                    new MySqlParameter("p_Activo", rango.Activo ?? true)
                };

                // Llamar al procedimiento almacenado
                var resultado = await ExecuteSelectProcedure(
                    "CrearHorariosBaseRango",
                    reader => reader.GetString("Resultado"),
                    parameters
                );

                return Ok(new { Mensaje = resultado.FirstOrDefault() });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}