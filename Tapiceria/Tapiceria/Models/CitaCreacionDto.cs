using System;

namespace Tapiceria.Models
{
    public class CitaCreacionDto
    {
        public int IdCliente { get; set; }
        public int IdServicio { get; set; }
        public int IdEmpleado { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; } = "Pendiente";
        public string Notas { get; set; } = string.Empty;
    }
}