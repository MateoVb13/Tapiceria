using System.ComponentModel.DataAnnotations;

namespace APITapiceria.Models
{
    public class Servicios
    {
        [Key]
        public int IdServicio { get; set; }

        [Required]
        public string Descripcion { get; set; }

        [Required]
        public int DuracionEstimada { get; set; }

        [Required]
        public decimal Precio { get; set; }

        public string Categoria { get; set; }
    }
}
