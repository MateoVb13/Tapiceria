using System;
using System.ComponentModel.DataAnnotations;

namespace APITapiceria.Models
{
    public class HorarioBaseAgendamientoDto
    {
        public int IdHorarioBase { get; set; }

        [Required]
        [Range(1, 7)]
        public int DiaSemana { get; set; }

        [Required]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int DuracionMinutos { get; set; }

        public int? IdServicio { get; set; }

        public string TipoDia { get; set; }

        public bool? Activo { get; set; }
    }

    public class CrearHorarioBaseAgendamientoDto
    {
        [Required]
        [Range(1, 7)]
        public int DiaSemana { get; set; }

        [Required]
        public TimeSpan HoraInicio { get; set; }

        [Required]
        public TimeSpan HoraFin { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int DuracionSlotMinutos { get; set; }

        public int DuracionMinutos => DuracionSlotMinutos;

        public int? IdServicio { get; set; }

        public string TipoDia { get; set; }

        public bool? Activo { get; set; } = true;
    }
}