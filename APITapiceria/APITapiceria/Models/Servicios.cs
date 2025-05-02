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
        public int DuracionEstimada { get; set; } // Asume minutos

        [Required]
        public decimal Precio { get; set; }

        public string? Categoria { get; set; } // Nullable en BD
    }

    public class ServiceDto // DTO para representar Servicio
    {
        public int IdServicio { get; set; }
        public string Descripcion { get; set; }
        public int DuracionEstimada { get; set; }
        public decimal Precio { get; set; }
        public string? Categoria { get; set; } // Nullable
    }
}
