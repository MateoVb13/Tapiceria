using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APITapiceria.Models
{
    public class Pagos
    {
        [Key]
        public int IdPago { get; set; }

        [Required]
        public int IdCita { get; set; }

        [Required]
        public string TipoPago { get; set; }

        public decimal? ValorPago { get; set; }

        [ForeignKey("IdCita")]
        public virtual Citas Citas { get; set; }
    }

    public class PaymentDto
    {
        public int IdPago { get; set; }
        public int IdCita { get; set; }
        public string TipoPago { get; set; }
        public decimal? ValorPago { get; set; }
        public DateTime FechaCita { get; set; }
        public string EstadoCita { get; set; }
    }

    public class CreatePaymentDto
    {
        [Required]
        public int IdCita { get; set; }
        [Required]
        public string TipoPago { get; set; }
        public decimal? ValorPago { get; set; }

        public class UpdatePaymentDto
        {
            [Required]
            public int IdPago { get; set; }
            [Required]
            public int IdCita { get; set; }
            [Required]
            public string TipoPago { get; set; }
            public decimal? ValorPago { get; set; }
        }
    }
}
