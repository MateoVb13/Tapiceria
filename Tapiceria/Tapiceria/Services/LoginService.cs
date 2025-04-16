using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Tapiceria.Models;
using Tapiceria.Config;

namespace Tapiceria.Services
{
    public class LoginService
    {
        private readonly HttpClient _httpClient;

        public LoginService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<bool> IniciarSesion(LoginRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"{ApiConfig.BaseUrl}/api/Usuarios/login", request);
            return response.IsSuccessStatusCode;
        }
    }
}
