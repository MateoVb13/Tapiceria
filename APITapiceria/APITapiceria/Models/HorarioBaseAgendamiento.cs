using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APITapiceria.Models
{
    // Clase principal que mapea a la tabla horarios_base_agendamiento
    public class HorarioBaseAgendamiento
    {
        [Key]
        public int IdHorarioBase { get; set; }

        [Required]
        [Range(1, 7)]
        public int DiaSemana { get; set; } // 1=Lunes, 7=Domingo

        [Required]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int DuracionMinutos { get; set; }

        // Clave foránea opcional a un servicio específico
        public int? IdServicio { get; set; }

        public string TipoDia { get; set; }

        [Required]
        public bool Activo { get; set; } = true;

        // Propiedad de navegación
        [ForeignKey("IdServicio")]
        public virtual Servicios Servicio { get; set; }
    }

    // DTO para la creación y actualización de horarios base
    public class HorarioBaseDto
    {
        [Required]
        [Range(1, 7)]
        public int DiaSemana { get; set; } // 1=Lunes, 7=Domingo

        [Required]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int DuracionMinutos { get; set; }

        public int? IdServicio { get; set; }

        public string TipoDia { get; set; }

        public bool? Activo { get; set; }
    }

    // DTO para crear múltiples horarios en un rango
    public class HorarioBaseRangoDto
    {
        [Required]
        [Range(1, 7)]
        public int DiaSemana { get; set; } // 1=Lunes, 7=Domingo

        [Required]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        public TimeSpan HoraFin { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int DuracionSlotMinutos { get; set; }

        public int? IdServicio { get; set; }

        public string TipoDia { get; set; }

        public bool? Activo { get; set; } = true;
    }
}