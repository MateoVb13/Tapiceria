using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Tapiceria.Config;
using Tapiceria.Models;
using Newtonsoft.Json;

namespace Tapiceria.Services
{
    public class PagosService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public PagosService()
        {
            _httpClient = new HttpClient();
            _baseUrl = ApiConfig.BaseUrl;
        }

        // Obtener pagos pendientes del cliente
        public async Task<List<PagoDto>> GetPagosPendientesAsync(int idCliente)
        {
            try
            {
                // Primero obtenemos todas las citas completadas del cliente
                var citasResponse = await _httpClient.GetAsync($"{_baseUrl}api/citas/cliente/{idCliente}");

                if (!citasResponse.IsSuccessStatusCode)
                    return new List<PagoDto>();

                var citasJson = await citasResponse.Content.ReadAsStringAsync();
                var citas = JsonConvert.DeserializeObject<List<CitaListItemDto>>(citasJson);

                if (citas == null) return new List<PagoDto>();

                var citasCompletadas = citas.Where(c => c.Estado.Equals("Completada", StringComparison.OrdinalIgnoreCase)).ToList();
                var pagosPendientes = new List<PagoDto>();

                foreach (var cita in citasCompletadas)
                {
                    // Verificar si ya existe un pago para esta cita
                    var pagosResponse = await _httpClient.GetAsync($"{_baseUrl}api/pagos/cita/{cita.IdCita}");

                    if (pagosResponse.IsSuccessStatusCode)
                    {
                        var pagosJson = await pagosResponse.Content.ReadAsStringAsync();
                        var pagosExistentes = JsonConvert.DeserializeObject<List<PaymentDto>>(pagosJson);

                        // Si no hay pagos registrados para esta cita, es un pago pendiente
                        if (pagosExistentes == null || pagosExistentes.Count == 0)
                        {
                            // Obtener el precio del servicio
                            var precio = await ObtenerPrecioServicio(cita.IdCita);

                            pagosPendientes.Add(new PagoDto
                            {
                                IdCita = cita.IdCita,
                                NombreServicio = cita.NombreServicio,
                                FechaCita = cita.FechaInicio,
                                Estado = "Pendiente",
                                MontoAPagar = precio
                            });
                        }
                    }
                }

                return pagosPendientes;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en GetPagosPendientesAsync: {ex.Message}");
                throw new Exception($"Error al obtener pagos pendientes: {ex.Message}", ex);
            }
        }

        // Procesar pago simulado
        public async Task<bool> ProcesarPagoSimuladoAsync(int idCita, string tipoPago, decimal monto)
        {
            try
            {
                var nuevoPago = new CreatePaymentDto
                {
                    IdCita = idCita,
                    TipoPago = tipoPago,
                    ValorPago = monto
                };

                var json = JsonConvert.SerializeObject(nuevoPago);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_baseUrl}api/pagos", content);

                var statusCode = (int)response.StatusCode;
                return statusCode >= 200 && statusCode < 300;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error procesando pago: {ex.Message}");
                return false;
            }
        }

        // Obtener historial de pagos del cliente
        public async Task<List<PaymentDto>> GetHistorialPagosAsync(int idCliente)
        {
            try
            {
                // Obtener todas las citas del cliente
                var citasResponse = await _httpClient.GetAsync($"{_baseUrl}api/citas/cliente/{idCliente}");

                if (!citasResponse.IsSuccessStatusCode)
                    return new List<PaymentDto>();

                var citasJson = await citasResponse.Content.ReadAsStringAsync();
                var citas = JsonConvert.DeserializeObject<List<CitaListItemDto>>(citasJson);

                if (citas == null) return new List<PaymentDto>();

                var historialPagos = new List<PaymentDto>();

                foreach (var cita in citas)
                {
                    var pagosResponse = await _httpClient.GetAsync($"{_baseUrl}api/pagos/cita/{cita.IdCita}");

                    if (pagosResponse.IsSuccessStatusCode)
                    {
                        var pagosJson = await pagosResponse.Content.ReadAsStringAsync();
                        var pagos = JsonConvert.DeserializeObject<List<PaymentDto>>(pagosJson);

                        if (pagos != null)
                        {
                            historialPagos.AddRange(pagos);
                        }
                    }
                }

                return historialPagos.OrderByDescending(p => p.FechaCita).ToList();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error en GetHistorialPagosAsync: {ex.Message}");
                throw new Exception($"Error al obtener historial de pagos: {ex.Message}", ex);
            }
        }

        private async Task<decimal> ObtenerPrecioServicio(int idCita)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}api/citas/{idCita}");

                if (response.IsSuccessStatusCode)
                {
                    var citaJson = await response.Content.ReadAsStringAsync();
                    var cita = JsonConvert.DeserializeObject<CitaDto>(citaJson);
                    return cita?.Precio ?? 0;
                }

                return 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error obteniendo precio: {ex.Message}");
                return 0;
            }
        }
    }
}