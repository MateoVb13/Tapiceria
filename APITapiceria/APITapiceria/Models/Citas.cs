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

        public string? Notas { get; set; }

        [ForeignKey("IdCliente")]
        public virtual Clientes Cliente { get; set; }

        [ForeignKey("IdServicio")]
        public virtual Servicios Servicio { get; set; }

        [ForeignKey("IdEmpleado")]
        public virtual Empleados? Empleado { get; set; }
    }

    /////////////////////////////////////////////////////////////////////////

    public class CitaDto
    {
        public int IdCita { get; set; }
        public int IdCliente { get; set; }
        public string NombreCliente { get; set; }
        public int IdServicio { get; set; }
        public string NombreServicio { get; set; }
        public int DuracionEstimada { get; set; }
        public decimal Precio { get; set; }
        public int? IdEmpleado { get; set; }
        public string? NombreEmpleado { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; }
        public string? Notas { get; set; }
    }

    public class CrearCitaDto
    {
        [Required(ErrorMessage = "El ID del cliente es obligatorio.")]
        public int IdCliente { get; set; }

        [Required(ErrorMessage = "El ID del servicio es obligatorio.")]
        public int IdServicio { get; set; }

        public int? IdEmpleado { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        public DateTime FechaFin { get; set; }

        public string? Estado { get; set; }

        public string? Notas { get; set; }
    }


    public class CitasUpdate
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

        public string? Notas { get; set; }

    


    }
}
