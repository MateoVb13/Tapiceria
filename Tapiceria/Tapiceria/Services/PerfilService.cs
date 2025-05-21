using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Tapiceria.Config;
using Tapiceria.Models;

namespace Tapiceria.Services
{
    public class PerfilService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public PerfilService()
        {
            _httpClient = new HttpClient();
            _baseUrl = ApiConfig.BaseUrl;

            // Configurar timeout y headers
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            // Para depuración
            System.Diagnostics.Debug.WriteLine($"🌐 PerfilService configurado con URL base: {_baseUrl}");
        }

        // Obtener información del cliente por ID de usuario
        public async Task<Cliente> GetClienteByUsuarioIdAsync(int idUsuario)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}api/clientes/PorUsuario/{idUsuario}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Cliente>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Cliente no encontrado para este usuario
                    return null;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al obtener datos del cliente: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error de conexión: {ex.Message}", ex);
            }
        }

        // Actualizar información del cliente
        public async Task<bool> UpdateClienteAsync(Cliente cliente)
        {
            try
            {
                var clienteJson = JsonConvert.SerializeObject(cliente);
                var content = new StringContent(clienteJson, System.Text.Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"{_baseUrl}api/clientes/{cliente.IdCliente}", content);

                // Simplificado: cualquier código que no sea de error se considera éxito
                bool esExitoso = !response.StatusCode.ToString().StartsWith("4") &&
                                !response.StatusCode.ToString().StartsWith("5");

                System.Diagnostics.Debug.WriteLine($"Código: {(int)response.StatusCode}, Exitoso: {esExitoso}");

                return esExitoso;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        // Obtener citas completadas del cliente
        public async Task<List<CitaListItemDto>> GetCitasCompletadasAsync(int idCliente)
        {
            try
            {
                // Obtenemos todas las citas del cliente
                var citasService = new CitasService();
                var todasLasCitas = await citasService.GetCitasByClienteIdAsync(idCliente);

                // Filtramos solo las completadas
                return todasLasCitas.Where(c => c.Estado.Equals("Completada", StringComparison.OrdinalIgnoreCase)).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener citas completadas: {ex.Message}", ex);
            }
        }

        // Validar si el cliente tiene datos completos
        public bool TieneDatosCompletos(Cliente cliente)
        {
            return cliente != null &&
                   !string.IsNullOrWhiteSpace(cliente.Contacto) &&
                   !string.IsNullOrWhiteSpace(cliente.Direccion);
        }
        // Agregar este método a la clase ClienteService o usar PerfilService
        public static async Task<Cliente> GetClienteActual()
        {
            try
            {
                // Recuperar el usuario de las preferencias
                string userJson = Preferences.Get("usuario_actual", string.Empty);
                if (string.IsNullOrEmpty(userJson))
                {
                    return null;
                }

                var usuario = JsonConvert.DeserializeObject<UserDto>(userJson);

                // Usar el servicio de perfil para obtener los datos del cliente
                var perfilService = new PerfilService();
                return await perfilService.GetClienteByUsuarioIdAsync(usuario.IdUsuario);
            }
            catch
            {
                return null;
            }
        }
        // Obtener información del cliente por ID
        public async Task<Cliente> GetClienteByIdAsync(int idCliente)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}api/clientes/{idCliente}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<Cliente>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Cliente no encontrado
                    return null;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Error al obtener datos del cliente: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en GetClienteByIdAsync: {ex.Message}");
                throw new Exception($"Error de conexión: {ex.Message}", ex);
            }
        }
    }
}