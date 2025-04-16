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

        [ForeignKey("IdCita")]
        public virtual Citas Cita { get; set; }
    }
}
