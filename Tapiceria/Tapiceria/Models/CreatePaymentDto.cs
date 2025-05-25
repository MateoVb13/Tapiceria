using System.ComponentModel.DataAnnotations;

namespace Tapiceria.Models
{
    public class CreatePaymentDto
    {
        [Required]
        public int IdCita { get; set; }

        [Required]
        public string TipoPago { get; set; }

        public decimal? ValorPago { get; set; }
    }
}