using System.Linq;
using UltimaMilla.Repartidor.Models;
using UltimaMilla.Repartidor.ViewModels;

namespace UltimaMilla.Repartidor;

public partial class JornadaPage : ContentPage
{
    private readonly JornadaViewModel viewModel;
    private bool? distribucionHorizontal;
    private bool navegando;

    public JornadaPage(JornadaViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var comando = viewModel.RefrescarCommand;

        if (comando.IsRunning && comando.ExecutionTask is Task tarea)
            await tarea;

        await comando.ExecuteAsync(null);
    }

    private async void OnParadaTapped(object? sender, TappedEventArgs e)
    {
        if (navegando || viewModel.EstaCargando ||
            e.Parameter is not Parada parada)
            return;

        navegando = true;

        try
        {
            if (sender is Border fila)
            {
                VisualStateManager.GoToState(fila, "Pressed");
                await Task.Delay(150);
                VisualStateManager.GoToState(fila, "Normal");
            }

            await Shell.Current.GoToAsync(
                $"{nameof(ParadaDetallePage)}?ParadaId={parada.Id}");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync(
                "No se pudo abrir el detalle", ex.Message, "Aceptar");
        }
        finally
        {
            navegando = false;
        }
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