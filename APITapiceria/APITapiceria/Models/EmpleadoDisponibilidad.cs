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
        public int IdEmpleado { get; set; } // Clave foránea a la tabla Empleados

        [Required]
        // Usamos INT para el día de la semana (1=Lunes, 7=Domingo)
        public int DiaSemana { get; set; }

        [Required]
        // TIME en BD se mapea a TimeSpan en C# por defecto en muchos proveedores EF Core
        public TimeSpan HoraInicio { get; set; }

        [Required]
        public TimeSpan HoraFin { get; set; }

        // Propiedad de navegación al Empleado asociado
        [ForeignKey("IdEmpleado")]
        public virtual Empleados Empleado { get; set; }
    }

    public class EmployeeAvailabilityDto // DTO para representar Disponibilidad de Empleado
    {
        public int IdEmpleadoDisponibilidad { get; set; }
        public int IdEmpleado { get; set; }
        public int DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
    }
}
