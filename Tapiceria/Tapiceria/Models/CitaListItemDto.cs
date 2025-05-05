using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tapiceria.Models
{
    public class CitaListItemDto
    {
        public int IdCita { get; set; }
        public string NombreServicio { get; set; }
        public string NombreEmpleado { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; }
        public string Notas { get; set; }

        public string Horario => $"{FechaInicio:hh:mm tt} - {FechaFin:hh:mm tt}";
        public string Fecha => FechaInicio.ToShortDateString();
    }
}
