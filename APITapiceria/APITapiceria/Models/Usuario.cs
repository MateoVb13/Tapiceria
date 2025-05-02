using System.ComponentModel.DataAnnotations;

namespace APITapiceria.Models
{
    public class Usuarios
    {
        [Key] // Indica que IdUsuario es la clave primaria
        public int IdUsuario { get; set; }

        [Required] // Indica que este campo es obligatorio
        public string NombreUsuario { get; set; }

        [Required]
        public string Correo { get; set; } // UNIQUE en BD

        [Required]
        // Almacena el HASH seguro de la contraseña en la base de datos
        public string Contrasena { get; set; }
    }


    public class UserDto // DTO para representar datos de Usuario (sin contraseña)
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; }
        public string Correo { get; set; }
    }
}
