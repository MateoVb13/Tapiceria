using System;

namespace Tapiceria.Models
{
    public class DisponibilidadHoraria
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int IdEmpleado { get; set; }
        public string NombreEmpleado { get; set; }

        // Propiedades auxiliares para la UI
        public string HoraInicioString => FechaInicio.ToString("hh:mm tt");
        public string HoraFinString => FechaFin.ToString("hh:mm tt");
        public string DuracionString 
        {
            get
            {
                var duracion = FechaFin - FechaInicio;
                return duracion.TotalMinutes switch
                {
                    < 60 => $"{duracion.TotalMinutes:0} min",
                    _ => $"{duracion.TotalHours:0.#} horas"
                };
            }
        }
    }
}