using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APITapiceria.Models
{
    public class Empleados
    {
        [Key]
        public int IdEmpleado { get; set; }

        [Required]
        public string NombreCompleto { get; set; }

        public string? Especialidad { get; set; } // Nullable en BD

        public string? Contacto { get; set; } // Nullable en BD

        public DateTime? HorarioDisponible { get; set; }
    }

    public class EmployeeDto // DTO para representar Empleado (sin info de Usuario que ya no tiene FK)
    {
        public int IdEmpleado { get; set; }
        public string NombreCompleto { get; set; }
        public string? Especialidad { get; set; } // Nullable
        public string? Contacto { get; set; } // Nullable
        // HorarioDisponible ya no está en la tabla principal, se obtiene de EmpleadoDisponibilidad
    }
}
