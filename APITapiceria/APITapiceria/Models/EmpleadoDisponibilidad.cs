using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APITapiceria.Models
{
    public class EmpleadoDisponibilidad
    {
        [Key]
        public int IdEmpleadoDisponibilidad { get; set; }

        [Required]
        public int IdEmpleado { get; set; }

        [Required]
        public int DiaSemana { get; set; }

        [Required]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        public TimeSpan HoraFin { get; set; }

        [ForeignKey("IdEmpleado")]
        public virtual Empleados Empleado { get; set; }
    }

    public class EmployeeAvailabilityDto
    {
        public int IdEmpleadoDisponibilidad { get; set; }
        public int IdEmpleado { get; set; }
        public int DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
    }
}
