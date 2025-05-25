using System;

namespace Tapiceria.Models
{
    public class CitaDto
    {
        public int IdCita { get; set; }
        public int IdCliente { get; set; }
        public string NombreCliente { get; set; }
        public int IdServicio { get; set; }
        public string NombreServicio { get; set; }
        public int DuracionEstimada { get; set; }
        public decimal Precio { get; set; }
        public int? IdEmpleado { get; set; }
        public string NombreEmpleado { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; }
        public string Notas { get; set; }
    }
}