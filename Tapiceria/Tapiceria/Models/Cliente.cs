using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tapiceria.Models
{
    public class Cliente
    {
        public int IdCliente { get; set; }
        public string NombreCompleto { get; set; }
        public string Contacto { get; set; }
        public string Direccion { get; set; }

        public int? IdUsuario { get; set; }

    }
}
