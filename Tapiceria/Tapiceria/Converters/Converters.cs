using System;
using System.Globalization;

namespace Tapiceria.Converters
{
    // Converter para mapear el estado de la cita a un color
    public class EstadoColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string estado)
            {
                return estado.ToLower() switch
                {
                    "pendiente" => "#ff9500", // Naranja
                    "confirmada" => "#34c759", // Verde
                    "en proceso" => "#007aff", // Azul
                    "completada" => "#5e5ce6", // Morado
                    "cancelada" => "#ff3b30", // Rojo
                    _ => "#8e8e93" // Gris por defecto
                };
            }
            return "#8e8e93"; // Gris por defecto
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter para verificar si un string no está vacío
    public class IsNotNullOrEmptyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                return !string.IsNullOrEmpty(strValue);
            }
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // Converter para mostrar el botón cancelar solo en citas pendientes
    public class PendienteVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string estado)
            {
                return estado.Equals("Pendiente", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}