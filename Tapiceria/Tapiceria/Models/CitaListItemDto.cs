using System;

namespace Tapiceria.Models
{
    public class CitaListItemDto
    {
        public int IdCita { get; set; }
        public string NombreServicio { get; set; } = string.Empty;
        public string NombreEmpleado { get; set; } = string.Empty;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;

        public string Horario => $"{FechaInicio:hh:mm tt} - {FechaFin:hh:mm tt}";
        public string Fecha => FechaInicio.ToShortDateString();
    }
}