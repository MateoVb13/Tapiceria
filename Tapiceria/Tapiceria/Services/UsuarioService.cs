using System.Net.Http.Json;
using System.Threading.Tasks;
using Tapiceria.Models;
using Tapiceria.Config; // Asegúrate de importar el namespace donde está ApiConfig

namespace Tapiceria.Services
{
    public class UsuarioService
    {
        private readonly HttpClient _httpClient;

        public UsuarioService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> LoginAsync(Usuarios usuario)
        {
            var response = await _httpClient.PostAsJsonAsync($"{ApiConfig.BaseUrl}/api/Usuarios/login", usuario);
            return response.IsSuccessStatusCode;
        }

    }
}
