using Microsoft.Maui.Dispatching;
using System.Linq;
using UltimaMilla.Repartidor.Models;
using UltimaMilla.Repartidor.ViewModels;

namespace UltimaMilla.Repartidor;

public partial class JornadaPage : ContentPage
{
    private readonly JornadaViewModel viewModel;
    private bool? distribucionHorizontal;
    private bool navegando;
    private bool visible;
    private Window? ventana;
    private readonly IDispatcherTimer ubicacionTimer;
    private readonly Microsoft.Maui.Dispatching.IDispatcherTimer reloj;

    public JornadaPage(JornadaViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
        reloj = Dispatcher.CreateTimer();
        reloj.Interval = TimeSpan.FromMinutes(1);
        reloj.IsRepeating = true;
        reloj.Tick += (_, _) => viewModel.ActualizarTiempoTurno();
        ubicacionTimer = Dispatcher.CreateTimer();
        ubicacionTimer.Interval = TimeSpan.FromSeconds(30);
        ubicacionTimer.IsRepeating = true;
        ubicacionTimer.Tick += ActualizarUbicacion;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        visible = true;
        ventana = Window;
        if (ventana is not null)
        {
            ventana.Stopped += OnVentanaDetenida;
            ventana.Resumed += OnVentanaReanudada;
        }
        await IniciarActualizacionAsync();
    }

    private async Task IniciarActualizacionAsync()
    {
        var comando = viewModel.RefrescarCommand;
        if (comando.IsRunning && comando.ExecutionTask is Task tarea)
            await tarea;

        if (!visible)
            return;

        viewModel.IniciarSeguimiento();
        reloj.Start();
        await comando.ExecuteAsync(null);

        if (visible)
            ubicacionTimer.Start();
    }

    private async void ActualizarUbicacion(object? sender, EventArgs e)
    {
        if (visible && !navegando && !viewModel.EstaCargando &&
            !viewModel.RefrescarCommand.IsRunning)
            await viewModel.RefrescarCommand.ExecuteAsync(null);
    }

    private void DetenerActualizacion()
    {
        visible = false;
        reloj.Stop();
        ubicacionTimer.Stop();
        viewModel.DetenerSeguimiento();
    }

    private void OnVentanaDetenida(object? sender, EventArgs e)
    {
        DetenerActualizacion();
    }

    private async void OnVentanaReanudada(object? sender, EventArgs e)
    {
        visible = true;
        await IniciarActualizacionAsync();
    }

    protected override void OnDisappearing()
    {
        DetenerActualizacion();
        if (ventana is not null)
        {
            ventana.Stopped -= OnVentanaDetenida;
            ventana.Resumed -= OnVentanaReanudada;
            ventana = null;
        }
        base.OnDisappearing();
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