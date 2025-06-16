// Archivo: APITapiceria.Controllers/ServiciosController.cs
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
    public class ServiciosController : ControllerBase
    {
        private readonly TapiceriaContext _context;

        public ServiciosController(TapiceriaContext context)
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
            using (var command = connection.CreateCommand()) {command.CommandText = procedureName; command.CommandType = CommandType.StoredProcedure; if (parameters != null) command.Parameters.AddRange(parameters); return await command.ExecuteNonQueryAsync(); }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ServiceDto>>> GetServicios()
        {
            try
            {
                var servicios = await ExecuteSelectProcedure(
                    "ObtenerServicios",
                    reader => new ServiceDto
                    {
                        IdServicio = reader.GetInt32("IdServicio"),
                        Descripcion = reader.GetString("Descripcion"),
                        DuracionEstimada = reader.GetInt32("DuracionEstimada"),
                        Precio = reader.GetDecimal("Precio"),
                        Categoria = reader.IsDBNull("Categoria") ? null : reader.GetString("Categoria")
                    }
                );
                return Ok(servicios);
            }
            catch (Exception ex) {return StatusCode(500, "Error al obtener servicios."); }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceDto>> GetServicio(int id)
        {
            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdServicio", id) };
                var servicios = await ExecuteSelectProcedure(
                    "ObtenerServicioPorId",
                    reader => new ServiceDto
                    {
                        IdServicio = reader.GetInt32("IdServicio"),
                        Descripcion = reader.GetString("Descripcion"),
                        DuracionEstimada = reader.GetInt32("DuracionEstimada"),
                        Precio = reader.GetDecimal("Precio"),
                        Categoria = reader.IsDBNull("Categoria") ? null : reader.GetString("Categoria")
                    },
                    parameters
                );
                var servicio = servicios.FirstOrDefault();

                if (servicio == null) return NotFound();
                return Ok(servicio);
            }
            catch (Exception ex) {return StatusCode(500, "Error al obtener servicio por ID."); }
        }

        [HttpPost]
        public async Task<ActionResult<ServiceDto>> PostServicio(Servicios servicio)
        {
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_Descripcion", servicio.Descripcion),
                    new MySqlParameter("@p_DuracionEstimada", servicio.DuracionEstimada),
                    new MySqlParameter("@p_Precio", servicio.Precio),
                    new MySqlParameter("@p_Categoria", servicio.Categoria ?? (object)DBNull.Value)
                };

                object? result = await ExecuteScalarProcedure("InsertarServicio", parameters);

                if (result != null && result != DBNull.Value)
                {
                    int nuevoIdServicio = Convert.ToInt32(result);
                    var newService = (await ExecuteSelectProcedure(
                       "ObtenerServicioPorId",
                        reader => new ServiceDto
                        {
                            IdServicio = reader.GetInt32("IdServicio"),
                            Descripcion = reader.GetString("Descripcion"),
                            DuracionEstimada = reader.GetInt32("DuracionEstimada"),
                            Precio = reader.GetDecimal("Precio"),
                            Categoria = reader.IsDBNull("Categoria") ? null : reader.GetString("Categoria")
                        },
                       new MySqlParameter("@p_IdServicio", nuevoIdServicio)
                   )).FirstOrDefault();

                    if (newService != null)
                    {
                        return CreatedAtAction(nameof(GetServicio), new { id = nuevoIdServicio }, newService);
                    }
                    else
                    {
                        return StatusCode(500, "Servicio creado, pero error al recuperar detalles.");
                    }
                }
                else
                {
                    return StatusCode(500, "Error al crear el servicio a través del procedimiento almacenado.");
                }
            }
            catch (Exception ex) {return StatusCode(500, "Error al crear servicio."); }
        }

        // PUT: api/servicios/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutServicio(int id, Servicios servicio)
        {
            if (id != servicio.IdServicio) return BadRequest("El ID de la URL no coincide con el ID del servicio.");

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_IdServicio", id),
                    new MySqlParameter("@p_Descripcion", servicio.Descripcion),
                    new MySqlParameter("@p_DuracionEstimada", servicio.DuracionEstimada),
                    new MySqlParameter("@p_Precio", servicio.Precio),
                    new MySqlParameter("@p_Categoria", servicio.Categoria ?? (object)DBNull.Value)
                };

                int filasAfectadas = await ExecuteNonQueryProcedure("ActualizarServicio", parameters);

                if (filasAfectadas == 0) return NotFound();

                return NoContent();
            }
            catch (Exception ex) {return StatusCode(500, "Error al actualizar servicio."); }
        }

        // DELETE: api/servicios/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServicio(int id)
        {
            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdServicio", id) };

                int filasAfectadas = await ExecuteNonQueryProcedure("EliminarServicio", parameters);

                if (filasAfectadas == 0) return NotFound();

                return NoContent();
            }
            catch (MySqlException ex)
            {

                return StatusCode(500, $"Error de base de datos al eliminar: {ex.Message}. Verifique si hay citas vinculadas.");
            }
            catch (Exception ex) {return StatusCode(500, "Error al eliminar servicio."); }
        }
    }
}