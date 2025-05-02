// Archivo: APITapiceria.Controllers/EmpleadosController.cs
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
    public class EmpleadosController : ControllerBase
    {
        private readonly TapiceriaContext _context; // Necesario para GetDbConnection()

        // Considera inyectar un servicio que contenga los métodos auxiliares de ejecución de SPs
        // En este ejemplo, asumimos que los métodos Execute...Procedure están accesibles (ej: en una clase base)

        public EmpleadosController(TapiceriaContext context)
        {
            _context = context;
        }

        // --- Aquí irían los métodos auxiliares o la inyección del servicio que los contenga ---
        // Copiamos aquí una versión básica si no usas herencia/servicio:
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

                using (var reader = await command.ExecuteReaderAsync() as MySqlDataReader) // Cast to MySqlDataReader
                {
                    if (reader == null) throw new InvalidCastException("Failed to cast DbDataReader to MySqlDataReader.");
                    while (await reader.ReadAsync())
                    {
                        results.Add(mapFunction(reader));
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
        // --- Fin Métodos Auxiliares (Refactorizar en producción) ---


        // =============================================
        // ENDPOINTS PARA EMPLEADOS - Llamando SPs
        // =============================================

        // GET: api/empleados
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmpleados()
        {
            try
            {
                var empleados = await ExecuteSelectProcedure(
                    "ObtenerEmpleados",
                    reader => new EmployeeDto // Función de mapeo a EmployeeDto
                    {
                        IdEmpleado = reader.GetInt32("IdEmpleado"),
                        NombreCompleto = reader.GetString("NombreCompleto"),
                        Especialidad = reader.IsDBNull("Especialidad") ? null : reader.GetString("Especialidad"),
                        Contacto = reader.IsDBNull("Contacto") ? null : reader.GetString("Contacto")
                        // HorarioDisponible ya no está en este SP de obtención básica
                    }
                );
                return Ok(empleados);
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al obtener empleados."); }
        }

        // GET: api/empleados/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> GetEmpleado(int id)
        {
            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdEmpleado", id) };
                var empleados = await ExecuteSelectProcedure(
                    "ObtenerEmpleadoPorId",
                    reader => new EmployeeDto // Función de mapeo a EmployeeDto
                    {
                        IdEmpleado = reader.GetInt32("IdEmpleado"),
                        NombreCompleto = reader.GetString("NombreCompleto"),
                        Especialidad = reader.IsDBNull("Especialidad") ? null : reader.GetString("Especialidad"),
                        Contacto = reader.IsDBNull("Contacto") ? null : reader.GetString("Contacto")
                    },
                    parameters
                );
                var empleado = empleados.FirstOrDefault();

                if (empleado == null) return NotFound();
                return Ok(empleado);
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al obtener empleado por ID."); }
        }

        // POST: api/empleados
        // Usa el modelo Empleados como entrada (sin IdUsuario, sin HorarioDisponible)
        [HttpPost]
        public async Task<ActionResult<EmployeeDto>> PostEmpleado(Empleados empleado)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_NombreCompleto", empleado.NombreCompleto),
                    new MySqlParameter("@p_Especialidad", empleado.Especialidad ?? (object)DBNull.Value),
                    new MySqlParameter("@p_Contacto", empleado.Contacto ?? (object)DBNull.Value)
                };

                object? result = await ExecuteScalarProcedure("InsertarEmpleado", parameters);

                if (result != null && result != DBNull.Value)
                {
                    int nuevoIdEmpleado = Convert.ToInt32(result);
                    // Obtener el empleado recién creado para devolverlo como DTO
                    var newEmployee = (await ExecuteSelectProcedure(
                       "ObtenerEmpleadoPorId",
                        reader => new EmployeeDto
                        {
                            IdEmpleado = reader.GetInt32("IdEmpleado"),
                            NombreCompleto = reader.GetString("NombreCompleto"),
                            Especialidad = reader.IsDBNull("Especialidad") ? null : reader.GetString("Especialidad"),
                            Contacto = reader.IsDBNull("Contacto") ? null : reader.GetString("Contacto")
                        },
                       new MySqlParameter("@p_IdEmpleado", nuevoIdEmpleado)
                   )).FirstOrDefault();

                    if (newEmployee != null)
                    {
                        return CreatedAtAction(nameof(GetEmpleado), new { id = nuevoIdEmpleado }, newEmployee);
                    }
                    else
                    {
                        return StatusCode(500, "Empleado creado, pero error al recuperar detalles.");
                    }
                }
                else
                {
                    return StatusCode(500, "Error al crear el empleado a través del procedimiento almacenado.");
                }
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al crear empleado."); }
        }

        // PUT: api/empleados/{id}
        // Usa el modelo Empleados como entrada
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEmpleado(int id, Empleados empleado)
        {
            if (id != empleado.IdEmpleado) return BadRequest("El ID de la URL no coincide con el ID del empleado.");

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_IdEmpleado", id),
                    new MySqlParameter("@p_NombreCompleto", empleado.NombreCompleto),
                    new MySqlParameter("@p_Especialidad", empleado.Especialidad ?? (object)DBNull.Value),
                    new MySqlParameter("@p_Contacto", empleado.Contacto ?? (object)DBNull.Value)
                };

                int filasAfectadas = await ExecuteNonQueryProcedure("ActualizarEmpleado", parameters);

                if (filasAfectadas == 0) return NotFound(); // Empleado no encontrado/actualizado

                return NoContent(); // 204 No Content
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al actualizar empleado."); }
        }

        // DELETE: api/empleados/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmpleado(int id)
        {
            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdEmpleado", id) };

                int filasAfectadas = await ExecuteNonQueryProcedure("EliminarEmpleado", parameters);

                if (filasAfectadas == 0) return NotFound(); // Empleado no encontrado/eliminado

                return NoContent(); // 204 No Content
            }
            catch (MySqlException ex)
            {
                // Manejar errores de FK si existen citas o disponibilidad vinculadas y no hay CASCADE/SET NULL
                // Log ex
                return StatusCode(500, $"Error de base de datos al eliminar: {ex.Message}. Verifique si hay citas o disponibilidad vinculadas.");
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al eliminar empleado."); }
        }
    }
}