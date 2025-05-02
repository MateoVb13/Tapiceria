// Archivo: APITapiceria.Controllers/ClientesController.cs
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
    public class ClientesController : ControllerBase
    {
        private readonly TapiceriaContext _context; // Necesario para GetDbConnection()

        // Considera inyectar un servicio que contenga los métodos auxiliares de ejecución de SPs
        // En este ejemplo, asumimos que los métodos Execute...Procedure están accesibles (ej: en una clase base)

        public ClientesController(TapiceriaContext context)
        {
            _context = context;
        }

        // Métodos auxiliares para ejecutar SPs (Ej: heredados de un BaseController)
        // private async Task<List<T>> ExecuteSelectProcedure<T>(...) { ... }
        // private async Task<object?> ExecuteScalarProcedure(...) { ... }
        // private async Task<int> ExecuteNonQueryProcedure(...) { ... }

        // --- Aquí irían los métodos auxiliares o la inyección del servicio que los contenga ---
        // Por simplicidad, copiamos aquí una versión básica si no usas herencia/servicio:
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
                    if (reader == null) throw new InvalidCastException("The DbDataReader could not be cast to MySqlDataReader.");
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
        // ENDPOINTS PARA CLIENTES - Llamando SPs
        // =============================================

        // GET: api/clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetClientes()
        {
            try
            {
                var clientes = await ExecuteSelectProcedure(
                    "ObtenerClientes",
                    reader => new ClientDto // Función de mapeo a ClientDto
                    {
                        IdCliente = reader.GetInt32("IdCliente"),
                        IdUsuario = reader.IsDBNull("IdUsuario") ? (int?)null : reader.GetInt32("IdUsuario"),
                        NombreCompleto = reader.GetString("NombreCompleto"),
                        Contacto = reader.IsDBNull("Contacto") ? null : reader.GetString("Contacto"),
                        Direccion = reader.IsDBNull("Direccion") ? null : reader.GetString("Direccion"),
                        NombreUsuario = reader.IsDBNull("NombreUsuario") ? null : reader.GetString("NombreUsuario"),
                        CorreoUsuario = reader.IsDBNull("Correo") ? null : reader.GetString("Correo") // Correo de usuario
                    }
                );
                return Ok(clientes);
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al obtener clientes."); }
        }

        // GET: api/clientes/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ClientDto>> GetCliente(int id)
        {
            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdCliente", id) };
                var clientes = await ExecuteSelectProcedure(
                    "ObtenerClientePorId",
                    reader => new ClientDto // Función de mapeo a ClientDto
                    {
                        IdCliente = reader.GetInt32("IdCliente"),
                        IdUsuario = reader.IsDBNull("IdUsuario") ? (int?)null : reader.GetInt32("IdUsuario"),
                        NombreCompleto = reader.GetString("NombreCompleto"),
                        Contacto = reader.IsDBNull("Contacto") ? null : reader.GetString("Contacto"),
                        Direccion = reader.IsDBNull("Direccion") ? null : reader.GetString("Direccion"),
                        NombreUsuario = reader.IsDBNull("NombreUsuario") ? null : reader.GetString("NombreUsuario"),
                        CorreoUsuario = reader.IsDBNull("Correo") ? null : reader.GetString("Correo")
                    },
                    parameters
                );
                var cliente = clientes.FirstOrDefault();

                if (cliente == null) return NotFound();
                return Ok(cliente);
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al obtener cliente por ID."); }
        }

        // POST: api/clientes
        [HttpPost]
        public async Task<ActionResult<ClientDto>> PostCliente(Clientes cliente) // Usa el modelo Clientes como entrada
        {
            // Opcional: Validar que IdUsuario (si no es null) exista en la tabla Usuarios
            if (cliente.IdUsuario.HasValue)
            {
                bool usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == cliente.IdUsuario.Value);
                if (!usuarioExiste) return BadRequest("El IdUsuario especificado no existe.");
            }

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_IdUsuario", cliente.IdUsuario ?? (object)DBNull.Value),
                    new MySqlParameter("@p_NombreCompleto", cliente.NombreCompleto),
                    new MySqlParameter("@p_Contacto", cliente.Contacto ?? (object)DBNull.Value),
                    new MySqlParameter("@p_Direccion", cliente.Direccion ?? (object)DBNull.Value)
                };

                object? result = await ExecuteScalarProcedure("InsertarCliente", parameters);

                if (result != null && result != DBNull.Value)
                {
                    int nuevoIdCliente = Convert.ToInt32(result);
                    // Obtener el cliente recién creado para devolverlo como DTO
                    var newClient = (await ExecuteSelectProcedure(
                       "ObtenerClientePorId",
                        reader => new ClientDto
                        {
                            IdCliente = reader.GetInt32("IdCliente"),
                            IdUsuario = reader.IsDBNull("IdUsuario") ? (int?)null : reader.GetInt32("IdUsuario"),
                            NombreCompleto = reader.GetString("NombreCompleto"),
                            Contacto = reader.IsDBNull("Contacto") ? null : reader.GetString("Contacto"),
                            Direccion = reader.IsDBNull("Direccion") ? null : reader.GetString("Direccion"),
                            NombreUsuario = reader.IsDBNull("NombreUsuario") ? null : reader.GetString("NombreUsuario"),
                            CorreoUsuario = reader.IsDBNull("Correo") ? null : reader.GetString("Correo")
                        },
                       new MySqlParameter("@p_IdCliente", nuevoIdCliente)
                   )).FirstOrDefault();

                    if (newClient != null)
                    {
                        return CreatedAtAction(nameof(GetCliente), new { id = nuevoIdCliente }, newClient);
                    }
                    else
                    {
                        return StatusCode(500, "Cliente creado, pero error al recuperar detalles.");
                    }
                }
                else
                {
                    return StatusCode(500, "Error al crear el cliente a través del procedimiento almacenado.");
                }
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al crear cliente."); }
        }

        // PUT: api/clientes/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCliente(int id, Clientes cliente) // Usa el modelo Clientes como entrada
        {
            if (id != cliente.IdCliente) return BadRequest("El ID de la URL no coincide con el ID del cliente.");

            // Opcional: Validar que IdUsuario (si no es null) exista en la tabla Usuarios
            if (cliente.IdUsuario.HasValue)
            {
                bool usuarioExiste = await _context.Usuarios.AnyAsync(u => u.IdUsuario == cliente.IdUsuario.Value);
                if (!usuarioExiste) return BadRequest("El IdUsuario especificado no existe.");
            }

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_IdCliente", id),
                    new MySqlParameter("@p_IdUsuario", cliente.IdUsuario ?? (object)DBNull.Value),
                    new MySqlParameter("@p_NombreCompleto", cliente.NombreCompleto),
                    new MySqlParameter("@p_Contacto", cliente.Contacto ?? (object)DBNull.Value),
                    new MySqlParameter("@p_Direccion", cliente.Direccion ?? (object)DBNull.Value)
                };

                int filasAfectadas = await ExecuteNonQueryProcedure("ActualizarCliente", parameters);

                if (filasAfectadas == 0) return NotFound(); // Cliente no encontrado/actualizado

                return NoContent(); // 204 No Content
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al actualizar cliente."); }
        }

        // DELETE: api/clientes/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdCliente", id) };

                int filasAfectadas = await ExecuteNonQueryProcedure("EliminarCliente", parameters);

                if (filasAfectadas == 0) return NotFound(); // Cliente no encontrado/eliminado

                return NoContent(); // 204 No Content
            }
            catch (MySqlException ex)
            {
                // Manejar errores de FK si existen citas vinculadas y no hay CASCADE/SET NULL
                // Log ex
                return StatusCode(500, $"Error de base de datos al eliminar: {ex.Message}. Verifique si hay citas vinculadas.");
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al eliminar cliente."); }
        }
    }
}