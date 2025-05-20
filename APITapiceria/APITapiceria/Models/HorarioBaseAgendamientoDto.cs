using System;
using System.ComponentModel.DataAnnotations;

namespace APITapiceria.Models
{
    // DTO para la creación y actualización de horarios base
    public class HorarioBaseAgendamientoDto
    {
        // Propiedad para el ID (solo para respuestas)
        public int IdHorarioBase { get; set; }

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
    public class CrearHorarioBaseAgendamientoDto
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

        // Agregamos DuracionMinutos como alias de DuracionSlotMinutos para mantener compatibilidad
        public int DuracionMinutos => DuracionSlotMinutos;

        public int? IdServicio { get; set; }

        public string TipoDia { get; set; }

        public bool? Activo { get; set; } = true;
    }
}