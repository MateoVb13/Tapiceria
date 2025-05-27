using System; // Necesario para DateTime
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APITapiceria.Models
{
    public class Citas
    {
        [Key]
        public int IdCita { get; set; }

        [Required]
        public int IdCliente { get; set; } // Clave foránea

        [Required]
        public int IdServicio { get; set; } // Clave foránea

        // Clave foránea a Empleados (es nullable en BD)
        public int? IdEmpleado { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        public string Estado { get; set; }

        public string? Notas { get; set; } // Nullable en BD

        // Propiedades de navegación
        [ForeignKey("IdCliente")]
        public virtual Clientes Cliente { get; set; }

        [ForeignKey("IdServicio")]
        public virtual Servicios Servicio { get; set; }

        // Propiedad de navegación al Empleado (nullable)
        [ForeignKey("IdEmpleado")]
        public virtual Empleados? Empleado { get; set; } // Propiedad de navegación nullable
    }

    /////////////////////////////////////////////////////////////////////////

    // DTO para representar una cita con detalles de Cliente, Servicio y Empleado
    public class CitaDto
    {
        public int IdCita { get; set; }
        public int IdCliente { get; set; }
        public string NombreCliente { get; set; } // Viene del join
        public int IdServicio { get; set; }
        public string NombreServicio { get; set; } // Viene del join
        public int DuracionEstimada { get; set; } // Viene del join con Servicios
        public decimal Precio { get; set; } // Viene del join con Servicios
        public int? IdEmpleado { get; set; } // Puede ser nulo
        public string? NombreEmpleado { get; set; } // Viene del join, puede ser nulo
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; }
        public string? Notas { get; set; } // Puede ser nulo
        // Asegúrate de que estas propiedades coincidan con las columnas/alias
        // que tus procedimientos almacenados SELECT devuelven.
    }

    // DTO para los datos de entrada al crear una cita
    public class CrearCitaDto
    {
        [Required(ErrorMessage = "El ID del cliente es obligatorio.")]
        public int IdCliente { get; set; }

        [Required(ErrorMessage = "El ID del servicio es obligatorio.")]
        public int IdServicio { get; set; }

        public int? IdEmpleado { get; set; } // Nullable (si se asigna al crear)

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        public DateTime FechaInicio { get; set; }

        [Required(ErrorMessage = "La fecha de fin es obligatoria.")]
        public DateTime FechaFin { get; set; }

        public string? Estado { get; set; } // Nullable (si no es obligatorio en la entrada)

        public string? Notas { get; set; } // Nullable
    }


    public class CitasUpdate
    {

        [Key]
        public int IdCita { get; set; }

        [Required]
        public int IdCliente { get; set; } // Clave foránea

        [Required]
        public int IdServicio { get; set; } // Clave foránea

        // Clave foránea a Empleados (es nullable en BD)
        public int? IdEmpleado { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        public string Estado { get; set; }

        public string? Notas { get; set; } // Nullable en BD

    


    }
}
