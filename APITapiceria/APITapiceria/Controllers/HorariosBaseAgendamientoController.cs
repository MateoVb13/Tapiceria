using APITapiceria.Data;
using APITapiceria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;

namespace APITapiceria.Controllers
{
    [Route("api/[controller]")] // La ruta base será /api/HorariosBaseAgendamiento
    [ApiController]
    public class HorariosBaseAgendamientoController : ControllerBase
    {
        private readonly TapiceriaContext _context;

        // Constructor para inyectar el contexto de la base de datos
        public HorariosBaseAgendamientoController(TapiceriaContext context)
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
                    if (reader == null) throw new InvalidCastException("The DbDataReader could not be cast to MySqlDataReader.");
                    while (await reader.ReadAsync()) results.Add(mapFunction(reader));
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

        // GET: api/HorariosBaseAgendamiento
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HorarioBaseAgendamientoDto>>> GetHorariosBase(
            [FromQuery] int diaSemana = 0, 
            [FromQuery] bool soloActivos = false 
        )
        {
            try
            {
                var parameters = new MySqlParameter[] {
                    new MySqlParameter("@p_DiaSemana", diaSemana),
                    new MySqlParameter("@p_SoloActivos", soloActivos)
                };

                var horarios = await ExecuteSelectProcedure(
                    "ObtenerHorariosBaseAgendamiento", // Tu SP para obtener lista
                    reader => new HorarioBaseAgendamientoDto // Mapea a DTO
                    {
                        IdHorarioBase = reader.GetInt32("IdHorarioBase"),
                        DiaSemana = reader.GetInt32("DiaSemana"),
                        HoraInicio = reader.GetTimeSpan("HoraInicio"), // Lee como TimeSpan
                        DuracionMinutos = reader.GetInt32("DuracionMinutos"),
                        TipoDia = reader.IsDBNull("TipoDia") ? null : reader.GetString("TipoDia"),
                        Activo = reader.GetBoolean("Activo")
                    },
                    parameters
                );
                return Ok(horarios);
            }
            catch (Exception ex)
            {
                // Log ex
                return StatusCode(500, "Error al obtener horarios base de agendamiento.");
            }
        }

        // GET: api/HorariosBaseAgendamiento/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<HorarioBaseAgendamientoDto>> GetHorarioBase(int id)
        {
            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdHorarioBase", id) };
                var horarios = await ExecuteSelectProcedure(
                    "ObtenerHorarioBaseAgendamientoPorId", // Tu SP para obtener por ID
                    reader => new HorarioBaseAgendamientoDto // Mapea a DTO
                    {
                        IdHorarioBase = reader.GetInt32("IdHorarioBase"),
                        DiaSemana = reader.GetInt32("DiaSemana"),
                        HoraInicio = reader.GetTimeSpan("HoraInicio"), // Lee como TimeSpan
                        DuracionMinutos = reader.GetInt32("DuracionMinutos"),
                        TipoDia = reader.IsDBNull("TipoDia") ? null : reader.GetString("TipoDia"),
                        Activo = reader.GetBoolean("Activo")
                    },
                    parameters
                );
                var horario = horarios.FirstOrDefault();

                if (horario == null) return NotFound();
                return Ok(horario);
            }
            catch (Exception ex)
            {
                // Log ex
                return StatusCode(500, "Error al obtener horario base por ID.");
            }
        }

        // POST: api/HorariosBaseAgendamiento
        [HttpPost]
        public async Task<ActionResult<HorarioBaseAgendamientoDto>> PostHorarioBase(CrearHorarioBaseAgendamientoDto horarioDto) // Usa el DTO de creación
        {
 
            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_DiaSemana", horarioDto.DiaSemana),
                    new MySqlParameter("@p_HoraInicio", horarioDto.HoraInicio), // Pasa TimeSpan al SP
                    new MySqlParameter("@p_DuracionMinutos", horarioDto.DuracionMinutos),
                    new MySqlParameter("@p_TipoDia", horarioDto.TipoDia ?? (object)DBNull.Value), // Maneja nulos
                    new MySqlParameter("@p_Activo", horarioDto.Activo)
                };

                object? result = await ExecuteScalarProcedure("InsertarHorarioBaseAgendamiento", parameters); // SP devuelve nuevo ID

                if (result != null && result != DBNull.Value)
                {
                    int nuevoId = Convert.ToInt32(result); 
                    
                    var nuevoHorario = (await ExecuteSelectProcedure(
                       "ObtenerHorarioBaseAgendamientoPorId", // Reusa el SP de obtención por ID
                        reader => new HorarioBaseAgendamientoDto
                        {
                            IdHorarioBase = reader.GetInt32("IdHorarioBase"),
                            DiaSemana = reader.GetInt32("DiaSemana"),
                            HoraInicio = reader.GetTimeSpan("HoraInicio"),
                            DuracionMinutos = reader.GetInt32("DuracionMinutos"),
                            TipoDia = reader.IsDBNull("TipoDia") ? null : reader.GetString("TipoDia"),
                            Activo = reader.GetBoolean("Activo")
                        },
                        new MySqlParameter("@p_IdHorarioBase", nuevoId)
                    )).FirstOrDefault();

                    if (nuevoHorario != null)
                    {
                        return CreatedAtAction(nameof(GetHorarioBase), new { id = nuevoId }, nuevoHorario); // Retorna 201 Created
                    }
                    else
                    {
                        return StatusCode(500, "Horario base creado, pero error al recuperar detalles.");
                    }
                }
                else
                {
                    return StatusCode(500, "Error al crear el horario base a través del procedimiento almacenado.");
                }
            }
            catch (MySqlException ex)
            {

                return StatusCode(500, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log ex
                return StatusCode(500, "Error al crear horario base de agendamiento.");
            }
        }

        // PUT: api/HorariosBaseAgendamiento/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutHorarioBase(int id, HorarioBaseAgendamientoDto horarioDto) // Usa el DTO para actualizar
        {
            if (id != horarioDto.IdHorarioBase) return BadRequest("El ID de la URL no coincide con el ID del horario base.");

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_IdHorarioBase", id),
                    new MySqlParameter("@p_DiaSemana", horarioDto.DiaSemana),
                    new MySqlParameter("@p_HoraInicio", horarioDto.HoraInicio),
                    new MySqlParameter("@p_DuracionMinutos", horarioDto.DuracionMinutos),
                    new MySqlParameter("@p_TipoDia", horarioDto.TipoDia ?? (object)DBNull.Value),
                    new MySqlParameter("@p_Activo", horarioDto.Activo)
                };

                int filasAfectadas = await ExecuteNonQueryProcedure("ActualizarHorarioBaseAgendamiento", parameters);

                if (filasAfectadas == 0)
                {
  
                    return NotFound(); 
                }

                return NoContent(); // 204 No Content si es exitoso
            }
            catch (MySqlException ex)
            {
                // Log ex
                return StatusCode(500, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Log ex
                return StatusCode(500, "Error al actualizar horario base de agendamiento.");
            }
        }

        // DELETE: api/HorariosBaseAgendamiento/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHorarioBase(int id)
        {


            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdHorarioBase", id) };

                int filasAfectadas = await ExecuteNonQueryProcedure("EliminarHorarioBaseAgendamiento", parameters);

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
            catch (Exception ex)
            { 

                return StatusCode(500, "Error al eliminar horario base de agendamiento.");
            }
        }

    }
}