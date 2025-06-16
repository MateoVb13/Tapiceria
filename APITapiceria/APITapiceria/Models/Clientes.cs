using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APITapiceria.Models
{
    public class Clientes
    {
        [Key]
        public int IdCliente { get; set; }

        public int? IdUsuario { get; set; }

        [Required]
        public string NombreCompleto { get; set; }

        public string? Contacto { get; set; }

        public string? Direccion { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuarios? Usuario { get; set; }
    }

    public class ClientDto
    {
        public int IdCliente { get; set; }
        public int? IdUsuario { get; set; }
        public string NombreCompleto { get; set; }
        public string? Contacto { get; set; }
        public string? Direccion { get; set; }
        public string? NombreUsuario { get; set; }
        public string? CorreoUsuario { get; set; }
    }
}
