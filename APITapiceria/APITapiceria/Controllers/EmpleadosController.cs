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
        private readonly TapiceriaContext _context;

        public EmpleadosController(TapiceriaContext context)
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

                using (var reader = await command.ExecuteReaderAsync() as MySqlDataReader)
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
            using (var command = connection.CreateCommand()) {command.CommandText = procedureName; command.CommandType = CommandType.StoredProcedure; if (parameters != null) command.Parameters.AddRange(parameters); return await command.ExecuteScalarAsync(); }
        }
        private async Task<int> ExecuteNonQueryProcedure(string procedureName, params MySqlParameter[] parameters)
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();
            using (var command = connection.CreateCommand()) {command.CommandText = procedureName; command.CommandType = CommandType.StoredProcedure; if (parameters != null) command.Parameters.AddRange(parameters); return await command.ExecuteNonQueryAsync(); }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmpleados()
        {
            try
            {
                var empleados = await ExecuteSelectProcedure(
                    "ObtenerEmpleados",
                    reader => new EmployeeDto
                    {
                        IdEmpleado = reader.GetInt32("IdEmpleado"),
                        NombreCompleto = reader.GetString("NombreCompleto"),
                        Especialidad = reader.IsDBNull("Especialidad") ? null : reader.GetString("Especialidad"),
                        Contacto = reader.IsDBNull("Contacto") ? null : reader.GetString("Contacto")
                    }
                );
                return Ok(empleados);
            }
            catch (Exception ex) {return StatusCode(500, "Error al obtener empleados."); }
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
                    reader => new EmployeeDto
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
            catch (Exception ex) {return StatusCode(500, "Error al obtener empleado por ID."); }
        }


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
            catch (Exception ex) {return StatusCode(500, "Error al crear empleado."); }
        }

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

                if (filasAfectadas == 0) return NotFound();

                return NoContent();
            }
            catch (Exception ex) {return StatusCode(500, "Error al actualizar empleado."); }
        }

        // DELETE: api/empleados/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmpleado(int id)
        {
            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdEmpleado", id) };

                int filasAfectadas = await ExecuteNonQueryProcedure("EliminarEmpleado", parameters);

                if (filasAfectadas == 0) return NotFound();

                return NoContent();
            }
            catch (MySqlException ex)
            {
                return StatusCode(500, $"Error de base de datos al eliminar: {ex.Message}. Verifique si hay citas o disponibilidad vinculadas.");
            }
            catch (Exception ex) {return StatusCode(500, "Error al eliminar empleado."); }
        }
    }
}