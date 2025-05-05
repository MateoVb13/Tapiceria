using System;
using Newtonsoft.Json;

namespace Tapiceria.Models
{
    public class Servicio
    {
        [JsonProperty("idServicio")] // Mapea si la API usa camelCase
        public int IdServicio { get; set; }

        [JsonProperty("descripcion")] // Coincide con el nombre en la API y BD
        public string Descripcion { get; set; }

        // CORREGIDO: Coincide con el tipo de dato que la API envía (INT para minutos)
        [JsonProperty("duracionEstimada")] // Mapea si la API usa camelCase
        public int DuracionEstimada { get; set; }

        [JsonProperty("precio")] // Mapea si la API usa camelCase
        public decimal Precio { get; set; }

        [JsonProperty("categoria")] // Mapea si la API usa camelCase
        public string Categoria { get; set; }

        // Propiedad adicional para usar en el Picker, mapeando desde Descripcion
        public string NombreServicio => Descripcion;

        // Opcional: Propiedad para obtener la duración como TimeSpan en la app si la necesitas
        public TimeSpan DuracionEstimadaTimeSpan => TimeSpan.FromMinutes(DuracionEstimada);
    }
}
