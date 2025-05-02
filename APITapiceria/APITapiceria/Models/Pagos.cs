using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace APITapiceria.Models
{
    public class Pagos
    {
        [Key]
        public int IdPago { get; set; }

        [Required]
        public int IdCita { get; set; } // Clave foránea

        [Required]
        public string TipoPago { get; set; }

        // ¡NUEVO! Mapea a la columna ValorPago en la BD
        public decimal? ValorPago { get; set; } // Nullable en BD (DECIMAL NULL)

        // Propiedad de navegación a la Cita asociada
        [ForeignKey("IdCita")]
        public virtual Citas Citas { get; set; }
    }

    public class PaymentDto // DTO para representar Pago con info básica de Cita
    {
        public int IdPago { get; set; }
        public int IdCita { get; set; }
        public string TipoPago { get; set; }
        public decimal? ValorPago { get; set; } // Nullable
        // Info básica de la cita relacionada (del join en el SP)
        public DateTime FechaCita { get; set; }
        public string EstadoCita { get; set; }
    }

    public class CreatePaymentDto // DTO para entrada POST Pagos
    {
        [Required]
        public int IdCita { get; set; }
        [Required]
        public string TipoPago { get; set; }
        public decimal? ValorPago { get; set; } // Nullable
    }

    public class UpdatePaymentDto // DTO para entrada PUT Pagos
    {
        [Required]
        public int IdPago { get; set; } // Se necesita el ID para saber cuál actualizar
        [Required]
        public int IdCita { get; set; }
        [Required]
        public string TipoPago { get; set; }
        public decimal? ValorPago { get; set; } // Nullable
    }
}
