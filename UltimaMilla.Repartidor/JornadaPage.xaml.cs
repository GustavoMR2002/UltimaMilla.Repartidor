using System.Linq;
using UltimaMilla.Repartidor.Models;
using UltimaMilla.Repartidor.ViewModels;

namespace UltimaMilla.Repartidor;

public partial class JornadaPage : ContentPage
{
    private bool? distribucionHorizontal;

    public JornadaPage(JornadaViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnParadaTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is not Parada parada)
            return;

        if (sender is Border fila)
        {
            VisualStateManager.GoToState(fila, "Pressed");
            await Task.Delay(180);
            VisualStateManager.GoToState(fila, "Normal");
        }

        await DisplayAlertAsync(
            "Detalle de la parada",
            $"Cliente: {parada.Cliente}\n" +
            $"Dirección: {parada.Direccion}\n" +
            $"Estado: {parada.Estado}",
            "Aceptar");
    }

    protected override void OnSizeAllocated(double width, double height)
    {
        base.OnSizeAllocated(width, height);

        if (PaginaGrid is null || width <= 0 || height <= 0)
            return;

        bool horizontal = width > height;

        if (distribucionHorizontal == horizontal)
            return;

        distribucionHorizontal = horizontal;

        PaginaGrid.Padding = horizontal
            ? new Thickness(12, 6)
            : new Thickness(20);

        PaginaGrid.RowSpacing = horizontal ? 6 : 12;

        SaludoLabel.FontSize = horizontal ? 20 : 24;
        TotalLabel.FontSize = horizontal ? 24 : 32;
        CompletadasLabel.FontSize = horizontal ? 24 : 32;

        foreach (Border tarjeta in ResumenGrid.Children.OfType<Border>())
        {
            tarjeta.Padding = horizontal
                ? new Thickness(10, 6)
                : new Thickness(12);
        }
    }
}