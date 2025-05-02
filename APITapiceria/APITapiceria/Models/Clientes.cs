using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APITapiceria.Models
{
    public class Clientes
    {
        [Key]
        public int IdCliente { get; set; }

        // La clave foránea a la tabla Usuarios (puede ser NULL según tu BD)
        public int? IdUsuario { get; set; }

        [Required]
        public string NombreCompleto { get; set; }

        public string? Contacto { get; set; } // Nullable en BD mapea a string?

        public string? Direccion { get; set; } // Nullable en BD mapea a string?

        // Propiedad de navegación para acceder al Usuario asociado (nullable)
        [ForeignKey("IdUsuario")] // Especifica la columna de la clave foránea
        public virtual Usuarios? Usuario { get; set; } // Propiedad de navegación también nullable y 'virtual'
    }

    public class ClientDto // DTO para representar Cliente con datos de Usuario
    {
        public int IdCliente { get; set; }
        public int? IdUsuario { get; set; } // Puede ser nulo
        public string NombreCompleto { get; set; }
        public string? Contacto { get; set; } // Nullable
        public string? Direccion { get; set; } // Nullable
        // Datos del usuario relacionado (si existe) - vienen del JOIN en el SP
        public string? NombreUsuario { get; set; } // Nullable
        public string? CorreoUsuario { get; set; } // Nullable (renombrado para evitar conflicto con Correo del cliente si existiera)
    }
}
