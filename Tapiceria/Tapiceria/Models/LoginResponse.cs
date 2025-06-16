using Newtonsoft.Json;

namespace Tapiceria.Models
{
    public class LoginResponse
    {
        [JsonProperty("message")]
        public string Mensaje { get; set; }

        [JsonProperty("user")]
        public UserDto Usuario { get; set; }
    }

    public class UserDto
    {
        [JsonProperty("idUsuario")]
        public int IdUsuario { get; set; }

        [JsonProperty("nombreUsuario")]
        public string NombreUsuario { get; set; }

        [JsonProperty("correo")]
        public string Correo { get; set; }

    }
}