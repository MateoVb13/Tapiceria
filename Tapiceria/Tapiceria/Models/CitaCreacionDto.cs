using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tapiceria.Models
{
    public class CitaCreacionDto
    {
        public int IdCliente { get; set; }
        public int IdServicio { get; set; }
        public int IdEmpleado { get; set; } 
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public string Estado { get; set; }
        public string Notas { get; set; } // Puede ser null
    }
}
