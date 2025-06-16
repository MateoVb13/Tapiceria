using APITapiceria.Data;
using APITapiceria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;
using System.Threading.Tasks;

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

            using (var command = connection.CreateCommand())
            {
                command.CommandText = procedureName;
                command.CommandType = CommandType.StoredProcedure;
                if (parameters != null) command.Parameters.AddRange(parameters);

                return await command.ExecuteScalarAsync();
            }

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
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsuarios()
        {
            try
            {
                var usuarios = await ExecuteSelectProcedure(
                    "ObtenerUsuarios",
                    reader => new UserDto
                    {
                        IdUsuario = reader.GetInt32("IdUsuario"),
                        NombreUsuario = reader.GetString("NombreUsuario"),
                        Correo = reader.GetString("Correo")
                    }
                );
                return Ok(usuarios);
            }
            catch (Exception ex) {return StatusCode(500, "Error al obtener usuarios."); }
        }

        // GET: api/usuarios/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUsuario(int id)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Where(u => u.IdUsuario == id)
                    .Select(u => new UserDto
                    {
                        IdUsuario = u.IdUsuario,
                        NombreUsuario = u.NombreUsuario,
                        Correo = u.Correo
                    })
                    .FirstOrDefaultAsync();

                if (usuario == null) return NotFound();
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error al obtener usuario por ID.");
            }
        }

        // POST: api/usuarios
        [HttpPost("registro")]
        public async Task<IActionResult> Registrar([FromBody] Usuarios usuario)
        {
            try
            {
                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"CALL InsertarUsuario({usuario.NombreUsuario}, {usuario.Correo}, {usuario.Contrasena})");

                Usuarios usuarioInsertado = null;

                var conn = _context.Database.GetDbConnection();
                await conn.OpenAsync();

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "CALL ObtenerUsuarioPorCorreo(@correo)";
                    command.CommandType = System.Data.CommandType.Text;

                    var correoParam = command.CreateParameter();
                    correoParam.ParameterName = "@correo";
                    correoParam.Value = usuario.Correo;
                    command.Parameters.Add(correoParam);

                    using var reader = await command.ExecuteReaderAsync();

                    if (await reader.ReadAsync())
                    {
                        usuarioInsertado = new Usuarios
                        {
                            IdUsuario = Convert.ToInt32(reader["IdUsuario"]),
                            NombreUsuario = reader["NombreUsuario"].ToString(),
                            Correo = reader["Correo"].ToString()
                        };
                    }
                }

                if (usuarioInsertado == null)
                    return StatusCode(500, new { mensaje = "No se pudo recuperar el usuario recién insertado" });

                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"CALL InsertarCliente({usuarioInsertado.IdUsuario}, {usuario.NombreUsuario}, '', '')");

                return Ok(new { mensaje = "Usuario registrado correctamente", usuario = usuarioInsertado });
            }
            catch (MySqlException ex)
            {
                if (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
                {
                    return Conflict("El correo o nombre de usuario ya existe.");
                }
                return StatusCode(500, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex) {return StatusCode(500, "Error al crear usuario."); }
        }

        // PUT: api/usuarios/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuarios usuario)
        {
            if (id != usuario.IdUsuario) return BadRequest("El ID de la URL no coincide con el ID del usuario.");

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_IdUsuario", id),
                    new MySqlParameter("@p_NombreUsuario", usuario.NombreUsuario),
                    new MySqlParameter("@p_Correo", usuario.Correo),
                    new MySqlParameter("@p_Contrasena", usuario.Contrasena)
                };

                int filasAfectadas = await ExecuteNonQueryProcedure("ActualizarUsuario", parameters);

                if (filasAfectadas == 0)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (MySqlException ex)
            {
                if (ex.ErrorCode == MySqlErrorCode.DuplicateKeyEntry)
                {
                    return Conflict("El correo o nombre de usuario ya existe.");
                }
                return StatusCode(500, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex) {return StatusCode(500, "Error al actualizar usuario."); }
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
                    return NotFound();
                }

                return NoContent();
            }
            catch (MySqlException ex)
            {

                return StatusCode(500, $"Error de base de datos al eliminar: {ex.Message}");
            }
            catch (Exception ex) {return StatusCode(500, "Error al eliminar usuario."); }
        }


        // POST: api/usuarios/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_Correo", request.Correo),
                    new MySqlParameter("@p_Contrasena", request.Contrasena)
                };

                var usuarios = await ExecuteSelectProcedure(
                    "ValidarLogin",
                     reader => new UserDto
                     {
                         IdUsuario = reader.GetInt32("IdUsuario"),
                         NombreUsuario = reader.GetString("NombreUsuario"),
                         Correo = reader.GetString("Correo")
                     },
                    parameters
                );

                var usuario = usuarios.FirstOrDefault();

                if (usuario == null)
                {
                    return Unauthorized("Credenciales inválidas.");
                }

                return Ok(new { Message = "Login exitoso", User = usuario});
            }
            catch (Exception ex) {return StatusCode(500, "Error durante el proceso de login."); }
        }
    }
}