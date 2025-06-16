using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tapiceria.Config;
using Tapiceria.Models;

namespace Tapiceria.Services
{
    public class CitasService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public CitasService()
        {
            _httpClient = new HttpClient();
            _baseUrl = ApiConfig.BaseUrl;
        }

        public async Task<List<Servicio>> GetServiciosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}api/servicios");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<Servicio>>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al obtener servicios: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error de conexión: {ex.Message}", ex);
            }
        }

        public async Task<List<DisponibilidadHoraria>> GetHorariosDisponiblesAsync(int idServicio, DateTime fecha)
        {
            try
            {
                string fechaFormateada = fecha.ToString("yyyy-MM-dd");

                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}api/Disponibilidad?idServicio={idServicio}&fecha={fechaFormateada}");

                if (response.IsSuccessStatusCode)
                {
                    var horariosJson = await response.Content.ReadFromJsonAsync<List<DisponibilidadHoraria>>();
                    return horariosJson;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al obtener horarios disponibles: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error de conexión: {ex.Message}", ex);
            }
        }

        public async Task<List<CitaListItemDto>> GetCitasByClienteIdAsync(int idCliente)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}api/citas/cliente/{idCliente}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<CitaListItemDto>>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al obtener citas del cliente: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error de conexión: {ex.Message}", ex);
            }
        }

        public async Task<CitaListItemDto> CreateCitaAsync(CitaCreacionDto citaDto)
        {
            try
            {
                var content = JsonContent.Create(citaDto);

                var response = await _httpClient.PostAsync($"{_baseUrl}api/citas", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CitaListItemDto>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al crear cita: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error de conexión: {ex.Message}", ex);
            }
        }

        public async Task<bool> CancelarCitaAsync(int idCita)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}api/citas/{idCita}");

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Error al cancelar cita: {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error de conexión al cancelar cita: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> TieneCitasPendientesAsync(int idCliente)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}api/citas/cliente/{idCliente}");

                if (response.IsSuccessStatusCode)
                {
                    var citas = await response.Content.ReadFromJsonAsync<List<CitaListItemDto>>();

                    return citas?.Any(c => c.Estado.Equals("Pendiente", StringComparison.OrdinalIgnoreCase)) ?? false;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al verificar citas pendientes: {ex.Message}", ex);
            }
        }
    }
}