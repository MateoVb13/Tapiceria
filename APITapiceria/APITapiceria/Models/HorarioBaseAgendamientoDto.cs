using System;
using Newtonsoft.Json; 
namespace APITapiceria.Models
{

    public class HorarioBaseAgendamientoDto
    {
        [JsonProperty("idHorarioBase")] 
        public int IdHorarioBase { get; set; }

        [JsonProperty("diaSemana")] 
        public int DiaSemana { get; set; } 

        [JsonProperty("horaInicio")] 
        public TimeSpan HoraInicio { get; set; }

        [JsonProperty("duracionMinutos")] 
        public int DuracionMinutos { get; set; }

        [JsonProperty("tipoDia")] 
        public string? TipoDia { get; set; } 

        [JsonProperty("activo")] 
        public bool Activo { get; set; }
    }

    public class CrearHorarioBaseAgendamientoDto
    {
        [JsonProperty("diaSemana")]
        public int DiaSemana { get; set; }

        [JsonProperty("horaInicio")]
        public TimeSpan HoraInicio { get; set; }

        [JsonProperty("duracionMinutos")]
        public int DuracionMinutos { get; set; }

        [JsonProperty("tipoDia")]
        public string? TipoDia { get; set; }

        [JsonProperty("activo")]
        public bool Activo { get; set; } = true;
    }
}