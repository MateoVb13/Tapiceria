// Archivo: APITapiceria.Controllers/PagosController.cs
using APITapiceria.Data;
using APITapiceria.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;
using static APITapiceria.Models.CreatePaymentDto;

namespace APITapiceria.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PagosController : ControllerBase
    {
        private readonly TapiceriaContext _context;

        public PagosController(TapiceriaContext context)
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
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPagos()
        {
            try
            {
                var pagos = await ExecuteSelectProcedure(
                    "ObtenerPagos",
                    reader => new PaymentDto
                    {
                        IdPago = reader.GetInt32("IdPago"),
                        IdCita = reader.GetInt32("IdCita"),
                        TipoPago = reader.GetString("TipoPago"),
                        ValorPago = reader.IsDBNull("ValorPago") ? (decimal?)null : reader.GetDecimal("ValorPago"),
                        FechaCita = reader.GetDateTime("FechaCita"),
                        EstadoCita = reader.GetString("EstadoCita")
                    }
                );
                return Ok(pagos);
            }
            catch (Exception ex) {return StatusCode(500, "Error al obtener pagos."); }
        }

        // GET: api/pagos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetPago(int id)
        {
            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdPago", id) };
                var pagos = await ExecuteSelectProcedure(
                    "ObtenerPagoPorId",
                    reader => new PaymentDto
                    {
                        IdPago = reader.GetInt32("IdPago"),
                        IdCita = reader.GetInt32("IdCita"),
                        TipoPago = reader.GetString("TipoPago"),
                        ValorPago = reader.IsDBNull("ValorPago") ? (decimal?)null : reader.GetDecimal("ValorPago"),
                        FechaCita = reader.GetDateTime("FechaCita"),
                        EstadoCita = reader.GetString("EstadoCita")
                    },
                    parameters
                );
                var pago = pagos.FirstOrDefault();

                if (pago == null) return NotFound();
                return Ok(pago);
            }
            catch (Exception ex) {return StatusCode(500, "Error al obtener pago por ID."); }
        }

        // GET: api/pagos/cita/{citaId}
        [HttpGet("cita/{citaId}")]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPagosPorCita(int citaId)
        {
            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdCita", citaId) };
                var pagos = await ExecuteSelectProcedure(
                    "ObtenerPagosPorCita",
                     reader => new PaymentDto
                     {
                         IdPago = reader.GetInt32("IdPago"),
                         IdCita = reader.GetInt32("IdCita"),
                         TipoPago = reader.GetString("TipoPago"),
                         ValorPago = reader.IsDBNull("ValorPago") ? (decimal?)null : reader.GetDecimal("ValorPago"),
                         FechaCita = reader.GetDateTime("FechaCita"),
                         EstadoCita = reader.GetString("EstadoCita")
                     },
                    parameters
                );
                return Ok(pagos);
            }
            catch (Exception ex) {return StatusCode(500, "Error al obtener pagos por cita."); }
        }


        // POST: api/pagos
        [HttpPost]
        public async Task<ActionResult<PaymentDto>> PostPago(CreatePaymentDto pagoDto)
        {
            bool citaExiste = await _context.Citas.AnyAsync(c => c.IdCita == pagoDto.IdCita);
            if (!citaExiste) return BadRequest("El IdCita especificado no existe.");

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_IdCita", pagoDto.IdCita),
                    new MySqlParameter("@p_TipoPago", pagoDto.TipoPago),
                    new MySqlParameter("@p_ValorPago", pagoDto.ValorPago ?? (object)DBNull.Value)
                };

                object? result = await ExecuteScalarProcedure("InsertarPago", parameters);

                if (result != null && result != DBNull.Value)
                {
                    int nuevoIdPago = Convert.ToInt32(result);
                    var newPayment = (await ExecuteSelectProcedure(
                       "ObtenerPagoPorId",
                        reader => new PaymentDto
                        {
                            IdPago = reader.GetInt32("IdPago"),
                            IdCita = reader.GetInt32("IdCita"),
                            TipoPago = reader.GetString("TipoPago"),
                            ValorPago = reader.IsDBNull("ValorPago") ? (decimal?)null : reader.GetDecimal("ValorPago"),
                            FechaCita = reader.GetDateTime("FechaCita"),
                            EstadoCita = reader.GetString("EstadoCita")
                        },
                       new MySqlParameter("@p_IdPago", nuevoIdPago)
                   )).FirstOrDefault();

                    if (newPayment != null)
                    {
                        return CreatedAtAction(nameof(GetPago), new { id = nuevoIdPago }, newPayment);
                    }
                    else
                    {
                        return StatusCode(500, "Pago creado, pero error al recuperar detalles.");
                    }
                }
                else
                {
                    return StatusCode(500, "Error al crear el pago a través del procedimiento almacenado.");
                }
            }
            catch (MySqlException ex)
            {
                return StatusCode(500, $"Error de base de datos: {ex.Message}");
            }
            catch (Exception ex) {return StatusCode(500, "Error al crear pago."); }
        }

        // PUT: api/pagos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPago(int id, UpdatePaymentDto pagoDto)
        {
            if (id != pagoDto.IdPago) return BadRequest("El ID de la URL no coincide con el ID del pago.");

            bool pagoExiste = await _context.Pagos.AnyAsync(p => p.IdPago == id);
            if (!pagoExiste) return NotFound();

            bool citaExiste = await _context.Citas.AnyAsync(c => c.IdCita == pagoDto.IdCita);
            if (!citaExiste) return BadRequest("El IdCita especificado no existe.");

            try
            {
                var parameters = new MySqlParameter[]
                {
                    new MySqlParameter("@p_IdPago", id),
                    new MySqlParameter("@p_IdCita", pagoDto.IdCita),
                    new MySqlParameter("@p_TipoPago", pagoDto.TipoPago),
                    new MySqlParameter("@p_ValorPago", pagoDto.ValorPago ?? (object)DBNull.Value)
                };

                int filasAfectadas = await ExecuteNonQueryProcedure("ActualizarPago", parameters);

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
            catch (Exception ex) {return StatusCode(500, "Error al actualizar pago."); }
        }

        // DELETE: api/pagos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePago(int id)
        {
            bool pagoExiste = await _context.Pagos.AnyAsync(p => p.IdPago == id);
            if (!pagoExiste) return NotFound();

            try
            {
                var parameters = new MySqlParameter[] { new MySqlParameter("@p_IdPago", id) };

                int filasAfectadas = await ExecuteNonQueryProcedure("EliminarPago", parameters);

                if (filasAfectadas == 0)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex) {return StatusCode(500, "Error al eliminar pago."); }
        }
    }
}