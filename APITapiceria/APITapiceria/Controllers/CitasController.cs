using APITapiceria.Data;
using APITapiceria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;

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

        private async Task<List<T>> ExecuteSelectProcedure<T>(string procedureName, Func<MySqlDataReader, T> mapFunction, params MySqlParameter[] parameters)
        {
            List<T> results = new List<T>();
            var connection = _context.Database.GetDbConnection();

            if (connection.State != ConnectionState.Open)
                await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = procedureName;
                command.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var mySqlReader = (MySqlDataReader)reader;

                    while (await mySqlReader.ReadAsync())
                    {
                        results.Add(mapFunction(mySqlReader));
                    }
                }
            }

            return results;
        }
        private async Task<object?> ExecuteScalarProcedure(string procedureName, params MySqlParameter[] parameters)
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();
            using (var command = connection.CreateCommand()) {command.CommandText = procedureName; command.CommandType = CommandType.StoredProcedure; if (parameters != null) command.Parameters.AddRange(parameters); return await command.ExecuteScalarAsync(); }
        }
        private async Task<int> ExecuteNonQueryProcedure(string procedureName, params MySqlParameter[] parameters)
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = procedureName;
                command.CommandType = CommandType.StoredProcedure;
                if (parameters != null) command.Parameters.AddRange(parameters);

                return await command.ExecuteNonQueryAsync();
            }

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CitaDto>>> GetCitas()
        {
            try
            {
                var citas = await ExecuteSelectProcedure(
                    "ObtenerCitas",
                    reader => new CitaDto
                    {
                        IdCita = reader.GetInt32("IdCita"),
                        IdCliente = reader.GetInt32("IdCliente"),
                        NombreCliente = reader.GetString("NombreCliente"),
                        IdServicio = reader.GetInt32("IdServicio"),
                        NombreServicio = reader.GetString("NombreServicio"),
                        DuracionEstimada = reader.GetInt32("DuracionEstimada"),
                        Precio = reader.GetDecimal("Precio"),
                        IdEmpleado = reader.IsDBNull("IdEmpleado") ? (int?)null : reader.GetInt32("IdEmpleado"),
                        NombreEmpleado = reader.IsDBNull("NombreEmpleado") ? null : reader.GetString("NombreEmpleado"),
                        FechaInicio = reader.GetDateTime("FechaInicio"),
                        FechaFin = reader.GetDateTime("FechaFin"),
                        Estado = reader.GetString("Estado"),
                        Notas = reader.IsDBNull("Notas") ? null : reader.GetString("Notas")
                    }
                );
                return Ok(citas);
            }
            catch (Exception ex) {return StatusCode(500, "Error al obtener las citas."); }
        }

        // GET: api/citas/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<CitaDto>> GetCita(int id)
        {
            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdCita", id) };
                var citas = await ExecuteSelectProcedure(
                    "ObtenerCitaPorId",
                    reader => new CitaDto
                    {
                        IdCita = reader.GetInt32("IdCita"),
                        IdCliente = reader.GetInt32("IdCliente"),
                        NombreCliente = reader.GetString("NombreCliente"),
                        IdServicio = reader.GetInt32("IdServicio"),
                        NombreServicio = reader.GetString("NombreServicio"),
                        DuracionEstimada = reader.GetInt32("DuracionEstimada"),
                        Precio = reader.GetDecimal("Precio"),
                        IdEmpleado = reader.IsDBNull("IdEmpleado") ? (int?)null : reader.GetInt32("IdEmpleado"),
                        NombreEmpleado = reader.IsDBNull("NombreEmpleado") ? null : reader.GetString("NombreEmpleado"),
                        FechaInicio = reader.GetDateTime("FechaInicio"),
                        FechaFin = reader.GetDateTime("FechaFin"),
                        Estado = reader.GetString("Estado"),
                        Notas = reader.IsDBNull("Notas") ? null : reader.GetString("Notas")
                    },
                    parameters
                );
                var cita = citas.FirstOrDefault();

                if (cita == null) return NotFound();
                return Ok(cita);
            }
            catch (Exception ex) {return StatusCode(500, "Error al obtener la cita por ID."); }
        }

        // GET: api/citas/cliente/{clientId}
        [HttpGet("cliente/{clientId}")]
        public async Task<ActionResult<IEnumerable<CitaDto>>> GetCitasPorCliente(int clientId)
        {
            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdCliente", clientId) };
                var citas = await ExecuteSelectProcedure(
                    "ObtenerCitasPorCliente",
                    reader => new CitaDto
                    {
                        IdCita = reader.GetInt32("IdCita"),
                        IdCliente = reader.GetInt32("IdCliente"),
                        NombreCliente = reader.GetString("NombreCliente"),
                        IdServicio = reader.GetInt32("IdServicio"),
                        NombreServicio = reader.GetString("NombreServicio"),
                        DuracionEstimada = reader.GetInt32("DuracionEstimada"),
                        Precio = reader.GetDecimal("Precio"),
                        IdEmpleado = reader.IsDBNull("IdEmpleado") ? (int?)null : reader.GetInt32("IdEmpleado"),
                        NombreEmpleado = reader.IsDBNull("NombreEmpleado") ? null : reader.GetString("NombreEmpleado"),
                        FechaInicio = reader.GetDateTime("FechaInicio"),
                        FechaFin = reader.GetDateTime("FechaFin"),
                        Estado = reader.GetString("Estado"),
                        Notas = reader.IsDBNull("Notas") ? null : reader.GetString("Notas")
                    },
                    parameters
                );
                return Ok(citas);
            }
            catch (Exception ex) {return StatusCode(500, "Error al obtener las citas del cliente."); }
        }

        // GET: api/citas/empleado/{employeeId}
        [HttpGet("empleado/{employeeId}")]
        public async Task<ActionResult<IEnumerable<CitaDto>>> GetCitasPorEmpleado(int employeeId)
        {
            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdEmpleado", employeeId) };
                var citas = await ExecuteSelectProcedure(
                    "ObtenerCitasPorEmpleado",
                    reader => new CitaDto
                    {
                        IdCita = reader.GetInt32("IdCita"),
                        IdCliente = reader.GetInt32("IdCliente"),
                        NombreCliente = reader.GetString("NombreCliente"),
                        IdServicio = reader.GetInt32("IdServicio"),
                        NombreServicio = reader.GetString("NombreServicio"),
                        DuracionEstimada = reader.GetInt32("DuracionEstimada"),
                        Precio = reader.GetDecimal("Precio"),
                        IdEmpleado = reader.GetInt32("IdEmpleado"),
                        NombreEmpleado = reader.GetString("NombreEmpleado"),
                        FechaInicio = reader.GetDateTime("FechaInicio"),
                        FechaFin = reader.GetDateTime("FechaFin"),
                        Estado = reader.GetString("Estado"),
                        Notas = reader.IsDBNull("Notas") ? null : reader.GetString("Notas")
                    },
                    parameters
                );
                return Ok(citas);
            }
            catch (Exception ex) {return StatusCode(500, "Error al obtener las citas del empleado."); }
        }


        // POST: api/citas
        [HttpPost]
        public async Task<ActionResult<CitaDto>> PostCita([FromBody] CrearCitaDto citaDto)
        {

            bool clienteExiste = await _context.Clientes.AnyAsync(c => c.IdCliente == citaDto.IdCliente);
            if (!clienteExiste) return BadRequest("El IdCliente especificado no existe.");

            bool servicioExiste = await _context.Servicios.AnyAsync(s => s.IdServicio == citaDto.IdServicio);
            if (!servicioExiste) return BadRequest("El IdServicio especificado no existe.");

            if (citaDto.IdEmpleado.HasValue)
            {
                bool empleadoExiste = await _context.Empleados.AnyAsync(e => e.IdEmpleado == citaDto.IdEmpleado.Value);
                if (!empleadoExiste) return BadRequest("El IdEmpleado especificado no existe.");

                var availabilityParams = new MySqlParameter[]
                {
                     new MySqlParameter("@p_IdEmpleado", citaDto.IdEmpleado.Value),
                     new MySqlParameter("@p_FechaInicio", citaDto.FechaInicio),
                     new MySqlParameter("@p_FechaFin", citaDto.FechaFin)
                };

                object? overlapResult = await ExecuteScalarProcedure("CheckEmpleadoOcupadoEnRango", availabilityParams);
                int citasSolapadas = Convert.ToInt32(overlapResult ?? 0);

                if (citasSolapadas > 0)
                {
                    return BadRequest("El empleado seleccionado ya está ocupado en el horario solicitado.");
                }


            }

            if (citaDto.FechaFin <= citaDto.FechaInicio) return BadRequest("La FechaFin debe ser posterior a la FechaInicio.");

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_IdCliente", citaDto.IdCliente),
                    new MySqlParameter("@p_IdServicio", citaDto.IdServicio),
                    new MySqlParameter("@p_IdEmpleado", citaDto.IdEmpleado ?? (object)DBNull.Value),
                    new MySqlParameter("@p_FechaInicio", citaDto.FechaInicio),
                    new MySqlParameter("@p_FechaFin", citaDto.FechaFin),
                    new MySqlParameter("@p_Estado", citaDto.Estado ?? "Pendiente"),
                    new MySqlParameter("@p_Notas", citaDto.Notas ?? (object)DBNull.Value)
                };

                object? result = await ExecuteScalarProcedure("InsertarCita", parameters);

                if (result != null && result != DBNull.Value)
                {
                    int nuevoIdCita = Convert.ToInt32(result);
                    var nuevaCitaCreada = (await ExecuteSelectProcedure(
                       "ObtenerCitaPorId",
                        reader => new CitaDto
                        {
                            IdCita = reader.GetInt32("IdCita"),
                            IdCliente = reader.GetInt32("IdCliente"),
                            NombreCliente = reader.GetString("NombreCliente"),
                            IdServicio = reader.GetInt32("IdServicio"),
                            NombreServicio = reader.GetString("NombreServicio"),
                            DuracionEstimada = reader.GetInt32("DuracionEstimada"),
                            Precio = reader.GetDecimal("Precio"),
                            IdEmpleado = reader.IsDBNull("IdEmpleado") ? (int?)null : reader.GetInt32("IdEmpleado"),
                            NombreEmpleado = reader.IsDBNull("NombreEmpleado") ? null : reader.GetString("NombreEmpleado"),
                            FechaInicio = reader.GetDateTime("FechaInicio"),
                            FechaFin = reader.GetDateTime("FechaFin"),
                            Estado = reader.GetString("Estado"),
                            Notas = reader.IsDBNull("Notas") ? null : reader.GetString("Notas")
                        },
                       new MySqlParameter("@p_IdCita", nuevoIdCita)
                   )).FirstOrDefault();

                    if (nuevaCitaCreada != null)
                    {
                        return CreatedAtAction(nameof(GetCita), new { id = nuevoIdCita }, nuevaCitaCreada);
                    }
                    else
                    {
                        return StatusCode(500, "Cita creada, pero error al recuperar detalles.");
                    }
                }
                else
                {
                    return StatusCode(500, "Error al crear la cita a través del procedimiento almacenado.");
                }
            }
            catch (MySqlException ex)
            {
                return StatusCode(500, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex) {return StatusCode(500, "Error al crear cita."); }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCita(int id, CitasUpdate cita)
        {
            if (id != cita.IdCita) return BadRequest("El ID de la URL no coincide con el ID de la cita.");

            bool citaExiste = await _context.Citas.AnyAsync(c => c.IdCita == id);
            if (!citaExiste) return NotFound();

            bool clienteExiste = await _context.Clientes.AnyAsync(c => c.IdCliente == cita.IdCliente);
            if (!clienteExiste) return BadRequest("El IdCliente especificado no existe.");

            bool servicioExiste = await _context.Servicios.AnyAsync(s => s.IdServicio == cita.IdServicio);
            if (!servicioExiste) return BadRequest("El IdServicio especificado no existe.");

            if (cita.IdEmpleado.HasValue)
            {
                bool empleadoExiste = await _context.Empleados.AnyAsync(e => e.IdEmpleado == cita.IdEmpleado.Value);
                if (!empleadoExiste) return BadRequest("El IdEmpleado especificado no existe.");
            }
            if (cita.FechaFin <= cita.FechaInicio) return BadRequest("La FechaFin debe ser posterior a la FechaInicio.");


            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_IdCita", id),
                    new MySqlParameter("@p_IdCliente", cita.IdCliente),
                    new MySqlParameter("@p_IdServicio", cita.IdServicio),
                    new MySqlParameter("@p_IdEmpleado", cita.IdEmpleado ?? (object)DBNull.Value),
                    new MySqlParameter("@p_FechaInicio", cita.FechaInicio),
                    new MySqlParameter("@p_FechaFin", cita.FechaFin),
                    new MySqlParameter("@p_Estado", cita.Estado),
                    new MySqlParameter("@p_Notas", cita.Notas ?? (object)DBNull.Value)
                };

                int filasAfectadas = await ExecuteNonQueryProcedure("ActualizarCita", parameters);

                if (filasAfectadas == 0)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (MySqlException ex)
            {
                return StatusCode(500, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex) {return StatusCode(500, "Error al actualizar cita."); }
        }

        // DELETE: api/citas/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCita(int id)
        {
            bool citaExiste = await _context.Citas.AnyAsync(c => c.IdCita == id);
            if (!citaExiste) return NotFound();


            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdCita", id) };

                int filasAfectadas = await ExecuteNonQueryProcedure("EliminarCita", parameters);

                if (filasAfectadas == 0)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (MySqlException ex)
            {
                return StatusCode(500, $"Error de base de datos al eliminar: {ex.Message}. Verifique si hay pagos vinculados.");
            }
            catch (Exception ex) {return StatusCode(500, "Error al eliminar cita."); }
        }
        // GET: api/Citas/AvailableSlotsForService?idServicio={idServicio}&fecha={fecha}
        [HttpGet("AvailableSlotsForService")]
        public async Task<ActionResult<IEnumerable<TimeSpan>>> GetAvailableSlotsForService(
            [FromQuery] int idServicio,
            [FromQuery] DateTime fecha
        )
        {
            if (idServicio <= 0)
            {
                return BadRequest("El ID del servicio no es válido.");
            }

            try
            {
                var fechaSoloFecha = fecha.Date;

                int diaSemana = (int)fechaSoloFecha.DayOfWeek;
                if (diaSemana == 0) diaSemana = 7;

                var horariosBase = await _context.HorariosBaseAgendamiento
                    .Where(hb => (hb.IdServicio == idServicio || hb.IdServicio == null) &&
                                hb.DiaSemana == diaSemana &&
                                hb.Activo == true)
                    .ToListAsync();

                var horariosDisponibles = new List<TimeSpan>();

                foreach (var horarioBase in horariosBase)
                {
                    var fechaHoraInicio = fechaSoloFecha.Add(horarioBase.HoraInicio);
                    var fechaHoraFin = fechaHoraInicio.AddMinutes(horarioBase.DuracionMinutos);

                    bool horarioOcupado = await _context.Citas
                        .AnyAsync(c => c.FechaInicio < fechaHoraFin && c.FechaFin > fechaHoraInicio);

                    if (!horarioOcupado)
                    {
                        bool hayEmpleadoDisponible = await _context.Empleados
                            .AnyAsync(e => _context.EmpleadoDisponibilidad
                                .Any(ed => ed.IdEmpleado == e.IdEmpleado &&
                                          ed.DiaSemana == diaSemana &&
                                          ed.HoraInicio <= horarioBase.HoraInicio &&
                                          ed.HoraFin >= TimeSpan.FromTicks(horarioBase.HoraInicio.Ticks + TimeSpan.FromMinutes(horarioBase.DuracionMinutos).Ticks)));

                        if (hayEmpleadoDisponible)
                        {
                            horariosDisponibles.Add(horarioBase.HoraInicio);
                        }
                    }
                }

                return Ok(horariosDisponibles.OrderBy(h => h));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno del servidor al calcular horarios disponibles: {ex.Message}");
            }
        }
    }
}