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

        // Obtener todos los servicios disponibles
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
                    // Manejar error de API
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al obtener servicios: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                // Manejar excepciones de conexión
                throw new Exception($"Error de conexión: {ex.Message}", ex);
            }
        }

        // Obtener horarios disponibles para un servicio y fecha específicos
        public async Task<List<DisponibilidadHoraria>> GetHorariosDisponiblesAsync(int idServicio, DateTime fecha)
        {
            try
            {
                // Formatear la fecha como yyyy-MM-dd para la API
                string fechaFormateada = fecha.ToString("yyyy-MM-dd");

                // La ruta debe coincidir con tu API. Ajustar según sea necesario.
                var response = await _httpClient.GetAsync(
                    $"{_baseUrl}api/Disponibilidad?idServicio={idServicio}&fecha={fechaFormateada}");

                if (response.IsSuccessStatusCode)
                {
                    // La respuesta debería ser una lista de objetos con hora de inicio y fin
                    var horariosJson = await response.Content.ReadFromJsonAsync<List<DisponibilidadHoraria>>();
                    return horariosJson;
                }
                else
                {
                    // Manejar error de API
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al obtener horarios disponibles: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                // Manejar excepciones de conexión
                throw new Exception($"Error de conexión: {ex.Message}", ex);
            }
        }

        // Obtener las citas de un cliente
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
                    // Manejar error de API
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al obtener citas del cliente: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                // Manejar excepciones de conexión
                throw new Exception($"Error de conexión: {ex.Message}", ex);
            }
        }

        // Crear una nueva cita
        public async Task<CitaListItemDto> CreateCitaAsync(CitaCreacionDto citaDto)
        {
            try
            {
                // Convertir el DTO a JSON
                var content = JsonContent.Create(citaDto);

                var response = await _httpClient.PostAsync($"{_baseUrl}api/citas", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CitaListItemDto>();
                }
                else
                {
                    // Manejar error de API
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al crear cita: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                // Manejar excepciones de conexión
                throw new Exception($"Error de conexión: {ex.Message}", ex);
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

                    // Verificar si tiene alguna cita en estado "Pendiente"
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