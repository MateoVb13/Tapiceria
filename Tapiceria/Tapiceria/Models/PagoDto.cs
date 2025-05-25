using System;

namespace Tapiceria.Models
{
    public class PagoDto
    {
        public int IdCita { get; set; }
        public string NombreServicio { get; set; }
        public DateTime FechaCita { get; set; }
        public string Estado { get; set; }
        public decimal MontoAPagar { get; set; }
        public string FechaCitaString => FechaCita.ToString("dd/MM/yyyy");
        public string MontoString => $"${MontoAPagar:N2}";
    }
}