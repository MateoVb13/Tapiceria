using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tapiceria.Models
{
    public class DisponibilidadHoraria
    {
        [JsonProperty("idEmpleado")]
        public int IdEmpleado { get; set; }

        [JsonProperty("nombreEmpleado")]
        public string NombreEmpleado { get; set; }

        [JsonProperty("fechaInicio")]
        public DateTime FechaInicio { get; set; }

        [JsonProperty("fechaFin")]
        public DateTime FechaFin { get; set; }

        // Propiedades derivadas para fácil visualización
        public string HoraInicioString => FechaInicio.ToString("hh:mm tt");
        public string HoraFinString => FechaFin.ToString("hh:mm tt");
        public string HorarioCompleto => $"{HoraInicioString} - {HoraFinString}";

        // Para mostrar en la lista de selección
        public string DisplayText => $"{HoraInicioString} con {NombreEmpleado}";
    }
}