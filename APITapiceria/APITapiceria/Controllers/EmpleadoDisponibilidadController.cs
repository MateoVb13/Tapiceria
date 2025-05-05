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
    public class EmpleadoDisponibilidadController : ControllerBase
    {
        private readonly TapiceriaContext _context; // Necesario para GetDbConnection()

        public EmpleadoDisponibilidadController(TapiceriaContext context)
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
                    // Cast DbDataReader to MySqlDataReader
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
            using (var command = connection.CreateCommand()) { /* ... code ... */ command.CommandText = procedureName; command.CommandType = CommandType.StoredProcedure; if (parameters != null) command.Parameters.AddRange(parameters); return await command.ExecuteScalarAsync(); }
        }
        private async Task<int> ExecuteNonQueryProcedure(string procedureName, params MySqlParameter[] parameters)
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();
            using (var command = connection.CreateCommand()) { /* ... code ... */ command.CommandText = procedureName; command.CommandType = CommandType.StoredProcedure; if (parameters != null) command.Parameters.AddRange(parameters); return await command.ExecuteNonQueryAsync(); }
        }

        [HttpGet("empleado/{employeeId}")]
        public async Task<ActionResult<IEnumerable<EmployeeAvailabilityDto>>> GetDisponibilidadEmpleado(int employeeId)
        {
            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdEmpleado", employeeId) };
                var disponibilidad = await ExecuteSelectProcedure(
                    "ObtenerDisponibilidadEmpleado",
                    reader => new EmployeeAvailabilityDto // Función de mapeo a EmployeeAvailabilityDto
                    {
                        IdEmpleadoDisponibilidad = reader.GetInt32("IdEmpleadoDisponibilidad"),
                        IdEmpleado = reader.GetInt32("IdEmpleado"),
                        DiaSemana = reader.GetInt32("DiaSemana"),
                        HoraInicio = reader.GetTimeSpan("HoraInicio"), // Mapea TIME a TimeSpan
                        HoraFin = reader.GetTimeSpan("HoraFin") // Mapea TIME a TimeSpan
                    },
                    parameters
                );
                return Ok(disponibilidad);
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al obtener disponibilidad del empleado."); }
        }

        // POST: api/empleadodisponibilidad
        [HttpPost]
        public async Task<ActionResult<EmployeeAvailabilityDto>> PostDisponibilidadEmpleado(EmpleadoDisponibilidad disponibilidad) // Usa el modelo como entrada
        {

            bool empleadoExiste = await _context.Empleados.AnyAsync(e => e.IdEmpleado == disponibilidad.IdEmpleado);
            if (!empleadoExiste) return BadRequest("El IdEmpleado especificado no existe.");

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_IdEmpleado", disponibilidad.IdEmpleado),
                    new MySqlParameter("@p_DiaSemana", disponibilidad.DiaSemana),
                    new MySqlParameter("@p_HoraInicio", disponibilidad.HoraInicio), // Pasa TimeSpan a TIME en BD
                    new MySqlParameter("@p_HoraFin", disponibilidad.HoraFin) // Pasa TimeSpan a TIME en BD
                };

                object? result = await ExecuteScalarProcedure("InsertarDisponibilidadEmpleado", parameters);

                if (result != null && result != DBNull.Value)
                {
                    int nuevoIdDisponibilidad = Convert.ToInt32(result);
                    // Opcional: Obtener el bloque recién creado para devolverlo como DTO
                    var newBlock = (await ExecuteSelectProcedure(
                       "ObtenerDisponibilidadEmpleado", // Reusa el SP de obtención (necesitaría filtrar por IdEmpleadoDisponibilidad)
                        reader => new EmployeeAvailabilityDto
                        {
                            IdEmpleadoDisponibilidad = reader.GetInt32("IdEmpleadoDisponibilidad"),
                            IdEmpleado = reader.GetInt32("IdEmpleado"),
                            DiaSemana = reader.GetInt32("DiaSemana"),
                            HoraInicio = reader.GetTimeSpan("HoraInicio"),
                            HoraFin = reader.GetTimeSpan("HoraFin")
                        },
                       new MySqlParameter("@p_IdEmpleado", disponibilidad.IdEmpleado) // El SP actual solo filtra por IdEmpleado
                                                                                      // necesitarías un SP ObtenerDisponibilidadEmpleadoPorId si quieres uno solo
                   )).FirstOrDefault(b => b.IdEmpleadoDisponibilidad == nuevoIdDisponibilidad); // Filtramos en código C# temporalmente

                    if (newBlock != null)
                    {
                       return StatusCode(201, newBlock); // Retorna 201 con el objeto creado
                    }
                    else
                    {
                        return StatusCode(500, "Disponibilidad creada, pero error al recuperar detalles.");
                    }
                }
                else
                {
                    return StatusCode(500, "Error al crear la disponibilidad a través del procedimiento almacenado.");
                }
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al crear disponibilidad."); }
        }

     [HttpPut("{id}")]
        public async Task<IActionResult> PutDisponibilidadEmpleado(int id, EmpleadoDisponibilidad disponibilidad)
        {
            if (id != disponibilidad.IdEmpleadoDisponibilidad) return BadRequest("El ID de la URL no coincide con el ID de la disponibilidad.");

            // Opcional: Validar que IdEmpleado exista en la tabla Empleados
            bool empleadoExiste = await _context.Empleados.AnyAsync(e => e.IdEmpleado == disponibilidad.IdEmpleado);
            if (!empleadoExiste) return BadRequest("El IdEmpleado especificado no existe.");


            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_IdEmpleadoDisponibilidad", id),
                    new MySqlParameter("@p_IdEmpleado", disponibilidad.IdEmpleado),
                    new MySqlParameter("@p_DiaSemana", disponibilidad.DiaSemana),
                    new MySqlParameter("@p_HoraInicio", disponibilidad.HoraInicio),
                    new MySqlParameter("@p_HoraFin", disponibilidad.HoraFin)
                };

                int filasAfectadas = await ExecuteNonQueryProcedure("ActualizarDisponibilidadEmpleado", parameters);

                if (filasAfectadas == 0) return NotFound(); // Bloque no encontrado/actualizado

                return NoContent(); // 204 No Content
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al actualizar disponibilidad."); }
        }

        // DELETE: api/empleadodisponibilidad/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDisponibilidadEmpleado(int id)
        {
            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdEmpleadoDisponibilidad", id) };

                int filasAfectadas = await ExecuteNonQueryProcedure("EliminarDisponibilidadEmpleado", parameters);

                if (filasAfectadas == 0) return NotFound(); // Bloque no encontrado/eliminado

                return NoContent(); // 204 No Content
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al eliminar disponibilidad."); }
        }
    }
}