using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APITapiceria.Models
{
    public class HorarioBaseAgendamiento
    {
        [Key]
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

        [Required]
        public bool Activo { get; set; } = true;

        [ForeignKey("IdServicio")]
        public virtual Servicios Servicio { get; set; }
    }

}