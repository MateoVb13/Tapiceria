using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tapiceria.Models
{
    public class Empleado
    {
        public int IdEmpleado { get; set; }
        public string NombreEmpleado { get; set; }
        public string Especialidad { get; set; }
        public string Contacto { get; set; } // Asumiendo que la API retorna el contacto si es relevante para el cliente
    }
}
