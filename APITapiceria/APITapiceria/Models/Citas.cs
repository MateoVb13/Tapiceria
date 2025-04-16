using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APITapiceria.Models
{
    public class Citas
    {
        [Key]
        public int IdCita { get; set; }

        [Required]
        public int IdCliente { get; set; }

        [Required]
        public int IdServicio { get; set; }

        public int? IdEmpleado { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        public string Estado { get; set; }

        public string Notas { get; set; }

        [ForeignKey("IdCliente")]
        public virtual Clientes Cliente { get; set; }

        [ForeignKey("IdServicio")]
        public virtual Servicios Servicio { get; set; }

        [ForeignKey("IdEmpleado")]
        public virtual Empleados Empleado { get; set; }
    }
}
