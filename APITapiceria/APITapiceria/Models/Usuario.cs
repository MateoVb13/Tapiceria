using System.ComponentModel.DataAnnotations;

namespace APITapiceria.Models
{
    public class Usuarios
    {
        [Key]
        public int IdUsuario { get; set; }

        [Required]
        public string NombreUsuario { get; set; }

        [Required]
        public string Correo { get; set; }

        [Required]
        public string Contrasena { get; set; }
    }


    public class UserDto
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string Correo { get; set; }
    }
}
