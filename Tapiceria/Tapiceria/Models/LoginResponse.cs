
using Newtonsoft.Json; // Necesario para [JsonProperty]

namespace Tapiceria.Models
{
    public class LoginResponse
    {
        [JsonProperty("message")] // Mapea "message" del JSON a la propiedad C# Message
        public string Mensaje { get; set; }

        [JsonProperty("user")] // Mapea "user" del JSON a la propiedad C# User
        public UserDto Usuario { get; set; }
    }

    public class UserDto
    {
        [JsonProperty("idUsuario")] // Mapea "idUsuario" del JSON
        public int IdUsuario { get; set; }

        [JsonProperty("nombreUsuario")] // Mapea "nombreUsuario" del JSON
        public string NombreUsuario { get; set; }

        [JsonProperty("correo")] // Mapea "correo" del JSON
        public string Correo { get; set; }
    }
}