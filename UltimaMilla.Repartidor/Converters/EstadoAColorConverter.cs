using System.Globalization;
using UltimaMilla.Repartidor.Models;

namespace UltimaMilla.Repartidor.Converters;

public class EstadoAColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool oscuro = Application.Current?.RequestedTheme == AppTheme.Dark;

        return value switch
        {
            EstadoEntrega.Entregada => Color.FromArgb(oscuro ? "#81C784" : "#237A36"),
            EstadoEntrega.Fallida => Color.FromArgb(oscuro ? "#FFB4AB" : "#B3261E"),
            EstadoEntrega.Pendiente => Color.FromArgb(oscuro ? "#FFD166" : "#946200"),
            _ => Colors.Gray
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
