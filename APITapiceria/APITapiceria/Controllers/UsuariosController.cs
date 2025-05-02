using APITapiceria.Data;
using APITapiceria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector; // Asegúrate de tener instalado el paquete NuGet MySqlConnector
using System.Data; // Necesario para CommandType
using System.Threading.Tasks; // Necesario para Task

namespace APITapiceria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly TapiceriaContext _context;

        public UsuariosController(TapiceriaContext context)
        {
            _context = context;
        }

        // Auxiliar para ejecutar SPs SELECT que devuelven un conjunto de resultados
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
                    if (reader == null)
                    {
                        throw new InvalidCastException("The DbDataReader could not be cast to MySqlDataReader.");
                    }

                    while (await reader.ReadAsync())
                    {
                        results.Add(mapFunction(reader)); // Usa la función de mapeo proporcionada
                    }
                }
            }
            // La conexión se gestiona por el ciclo de vida del DbContext (si es Scoped)
            return results;
        }

        // Auxiliar para ejecutar SPs de acción (INSERT, UPDATE, DELETE) que devuelven un valor escalar (ej: LAST_INSERT_ID)
        private async Task<object?> ExecuteScalarProcedure(string procedureName, params MySqlParameter[] parameters)
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = procedureName;
                command.CommandType = CommandType.StoredProcedure;
                if (parameters != null) command.Parameters.AddRange(parameters);

                return await command.ExecuteScalarAsync(); // Devuelve un solo valor
            }
            // La conexión se gestiona por el ciclo de vida del DbContext
        }

        // Auxiliar para ejecutar SPs de acción (UPDATE, DELETE) que devuelven el número de filas afectadas (ExecuteNonQuery)
        private async Task<int> ExecuteNonQueryProcedure(string procedureName, params MySqlParameter[] parameters)
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State != ConnectionState.Open) await connection.OpenAsync();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = procedureName;
                command.CommandType = CommandType.StoredProcedure;
                if (parameters != null) command.Parameters.AddRange(parameters);

                return await command.ExecuteNonQueryAsync(); // Devuelve el número de filas afectadas
            }
            // La conexión se gestiona por el ciclo de vida del DbContext
        }

        // =============================================
        // ENDPOINTS PARA USUARIOS - Llamando SPs
        // =============================================

        // GET: api/usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsuarios()
        {
            try
            {
                var usuarios = await ExecuteSelectProcedure(
                    "ObtenerUsuarios",
                    reader => new UserDto // Función de mapeo a UserDto
                    {
                        IdUsuario = reader.GetInt32("IdUsuario"),
                        NombreUsuario = reader.GetString("NombreUsuario"),
                        Correo = reader.GetString("Correo")
                    }
                );
                return Ok(usuarios);
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al obtener usuarios."); }
        }

        // GET: api/usuarios/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUsuario(int id)
        {
            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdUsuario", id) };
                var usuarios = await ExecuteSelectProcedure(
                    "ObtenerUsuarioPorId",
                    reader => new UserDto // Función de mapeo a UserDto
                    {
                        IdUsuario = reader.GetInt32("IdUsuario"),
                        NombreUsuario = reader.GetString("NombreUsuario"),
                        Correo = reader.GetString("Correo")
                    },
                    parameters
                );
                var usuario = usuarios.FirstOrDefault();

                if (usuario == null) return NotFound();
                return Ok(usuario);
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al obtener usuario por ID."); }
        }

        // POST: api/usuarios
        [HttpPost]
        public async Task<ActionResult<UserDto>> PostUsuario(Usuarios usuario) // Usa el modelo Usuarios como entrada (puede validar con atributos)
        {
            // **NOTA DE SEGURIDAD:** Aquí DEBES hashear la contraseña de usuario.Contrasena
            // antes de pasársela al procedimiento almacenado InsertarUsuario.
            // El código para hashear la contraseña va aquí.
            // string hashedPassword = TuHelperDeHashing.HashPassword(usuario.Contrasena);

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_NombreUsuario", usuario.NombreUsuario),
                    new MySqlParameter("@p_Correo", usuario.Correo),
                    new MySqlParameter("@p_Contrasena", usuario.Contrasena) // <-- Pasa el HASHED password
                };

                // El SP devuelve el nuevo ID como escalar
                object? result = await ExecuteScalarProcedure("InsertarUsuario", parameters);

                if (result != null && result != DBNull.Value)
                {
                    int nuevoIdUsuario = Convert.ToInt32(result);
                    // Opcional: Obtener el usuario recién creado para devolverlo (sin contraseña)
                    var newUser = (await ExecuteSelectProcedure(
                       "ObtenerUsuarioPorId",
                       reader => new UserDto
                       {
                           IdUsuario = reader.GetInt32("IdUsuario"),
                           NombreUsuario = reader.GetString("NombreUsuario"),
                           Correo = reader.GetString("Correo")
                       },
                       new MySqlParameter("@p_IdUsuario", nuevoIdUsuario)
                   )).FirstOrDefault();

                    if (newUser != null)
                    {
                        return CreatedAtAction(nameof(GetUsuario), new { id = nuevoIdUsuario }, newUser);
                    }
                    else
                    {
                        return StatusCode(500, "Usuario creado, pero error al recuperar detalles.");
                    }

                }
                else
                {
                    return StatusCode(500, "Error al crear el usuario a través del procedimiento almacenado.");
                }

            }
            catch (MySqlException ex)
            {
                // Manejar duplicados si Correo ya existe
                if (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry) // Verifica el código de error específico para duplicados
                {
                    return Conflict("El correo o nombre de usuario ya existe."); // 409 Conflict
                }
                // Log ex
                return StatusCode(500, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al crear usuario."); }
        }

        // PUT: api/usuarios/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuarios usuario) // Usa el modelo Usuarios como entrada
        {
            if (id != usuario.IdUsuario) return BadRequest("El ID de la URL no coincide con el ID del usuario.");

            // **NOTA DE SEGURIDAD:** Si la contraseña se envía en el PUT, también DEBES hashearla aquí.
            // Considera si PUT debería permitir cambiar la contraseña o tener un endpoint aparte para eso.

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_IdUsuario", id),
                    new MySqlParameter("@p_NombreUsuario", usuario.NombreUsuario),
                    new MySqlParameter("@p_Correo", usuario.Correo),
                    new MySqlParameter("@p_Contrasena", usuario.Contrasena) // <-- Pasa el HASHED password si se actualiza
                };

                int filasAfectadas = await ExecuteNonQueryProcedure("ActualizarUsuario", parameters);

                if (filasAfectadas == 0)
                {
                    // Si no se afectaron filas, puede ser que el usuario con ese ID no exista
                    return NotFound();
                }

                return NoContent(); // 204 No Content si es exitoso
            }
            catch (MySqlException ex)
            {
                // Manejar duplicados si se intenta actualizar a un correo o nombre de usuario existente
                if (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
                {
                    return Conflict("El correo o nombre de usuario ya existe.");
                }
                // Log ex
                return StatusCode(500, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al actualizar usuario."); }
        }

        // DELETE: api/usuarios/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdUsuario", id) };

                int filasAfectadas = await ExecuteNonQueryProcedure("EliminarUsuario", parameters);

                if (filasAfectadas == 0)
                {
                    // Si no se afectaron filas, el usuario con ese ID no existía
                    return NotFound();
                }

                return NoContent(); // 204 No Content si es exitoso
            }
            catch (MySqlException ex)
            {
                // Manejar errores si hay FKs que impiden la eliminación (si no usas ON DELETE CASCADE/SET NULL)
                // Log ex
                return StatusCode(500, $"Error de base de datos al eliminar: {ex.Message}");
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error al eliminar usuario."); }
        }


        // POST: api/usuarios/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // **NOTA DE SEGURIDAD:** Aquí DEBES hashear la contraseña de request.Contrasena
            // antes de pasársela al procedimiento almacenado ValidarLogin.
            // El código para hashear la contraseña va aquí.
            // string hashedPassword = TuHelperDeHashing.HashPassword(request.Contrasena);

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_Correo", request.Correo),
                    new MySqlParameter("@p_Contrasena", request.Contrasena) // <-- Pasa el HASHED password
                };

                // ValidarLogin devuelve las columnas del usuario si coincide
                var usuarios = await ExecuteSelectProcedure(
                    "ValidarLogin",
                     reader => new UserDto // Mapea a UserDto (sin contraseña)
                     {
                         IdUsuario = reader.GetInt32("IdUsuario"),
                         NombreUsuario = reader.GetString("NombreUsuario"),
                         Correo = reader.GetString("Correo")
                     },
                    parameters
                );

                var usuario = usuarios.FirstOrDefault(); // Esperamos como máximo un resultado

                if (usuario == null)
                {
                    // No se encontró usuario con ese correo/contraseña
                    return Unauthorized("Credenciales inválidas."); // 401 Unauthorized
                }

                // Retornar los datos del usuario logueado (sin contraseña) y quizás el token
                return Ok(new { Message = "Login exitoso", User = usuario /*, Token = "tu_token_jwt"*/ });
            }
            catch (Exception ex) { /* Log ex */ return StatusCode(500, "Error durante el proceso de login."); }
        }
    }
}