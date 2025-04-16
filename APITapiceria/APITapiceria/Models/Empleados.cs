using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APITapiceria.Models
{
    public class Empleados
    {
        [Key]
        public int IdEmpleado { get; set; }

        public int? IdUsuario { get; set; }

        [Required]
        public string NombreCompleto { get; set; }

        public string Especialidad { get; set; }

        public string Contacto { get; set; }

        public string HorarioDisponible { get; set; }

        [ForeignKey("IdUsuario")]
        public virtual Usuarios Usuario { get; set; }
    }
}
