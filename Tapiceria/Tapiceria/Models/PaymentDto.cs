using System;

namespace Tapiceria.Models
{
    public class PaymentDto
    {
        public int IdPago { get; set; }
        public int IdCita { get; set; }
        public string TipoPago { get; set; }
        public decimal? ValorPago { get; set; }
        public DateTime FechaCita { get; set; }
        public string EstadoCita { get; set; }
    }
}