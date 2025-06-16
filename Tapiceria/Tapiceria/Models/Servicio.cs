using System;
using Newtonsoft.Json;

namespace Tapiceria.Models
{
    public class Servicio
    {
        [JsonProperty("idServicio")]
        public int IdServicio { get; set; }

        [JsonProperty("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [JsonProperty("duracionEstimada")]
        public int DuracionEstimada { get; set; }

        [JsonProperty("precio")]
        public decimal Precio { get; set; }

        [JsonProperty("categoria")]
        public string Categoria { get; set; } = string.Empty;

        public string NombreServicio => Descripcion;

        public TimeSpan DuracionEstimadaTimeSpan => TimeSpan.FromMinutes(DuracionEstimada);
    }
}