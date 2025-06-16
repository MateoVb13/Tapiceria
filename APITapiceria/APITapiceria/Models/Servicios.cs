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

        public string? Categoria { get; set; }
    }

    public class ServiceDto
    {
        public int IdServicio { get; set; }
        public string Descripcion { get; set; }
        public int DuracionEstimada { get; set; }
        public decimal Precio { get; set; }
        public string? Categoria { get; set; }
    }
}
