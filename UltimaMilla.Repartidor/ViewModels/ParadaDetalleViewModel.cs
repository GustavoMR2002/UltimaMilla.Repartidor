using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using UltimaMilla.Repartidor.Models;
using UltimaMilla.Repartidor.Services;

namespace UltimaMilla.Repartidor.ViewModels;

[QueryProperty(nameof(ParadaId), "ParadaId")]
public partial class ParadaDetalleViewModel : ObservableObject
{
    private readonly ParadasDatabase database;
    private readonly TurnoDatabase turnos;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PuedeEditar))]
    [NotifyCanExecuteChangedFor(nameof(MarcarEntregadaCommand))]
    [NotifyCanExecuteChangedFor(nameof(MarcarFallidaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReintentarEntregaCommand))]
    public partial bool TieneTurnoAbierto { get; set; }

    [ObservableProperty]
    public partial int ParadaId { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PuedeEditar))]
    [NotifyPropertyChangedFor(nameof(EsEntregada))]
    [NotifyPropertyChangedFor(nameof(EsFallida))]
    [NotifyCanExecuteChangedFor(nameof(MarcarEntregadaCommand))]
    [NotifyCanExecuteChangedFor(nameof(MarcarFallidaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReintentarEntregaCommand))]
    public partial Parada? Parada { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(MarcarFallidaCommand))]
    public partial string RazonFallo { get; set; }

    [ObservableProperty]
    public partial string MensajeEstado { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PuedeEditar))]
    [NotifyCanExecuteChangedFor(nameof(MarcarEntregadaCommand))]
    [NotifyCanExecuteChangedFor(nameof(MarcarFallidaCommand))]
    [NotifyCanExecuteChangedFor(nameof(ReintentarEntregaCommand))]
    public partial bool EstaOcupado { get; set; }

    public bool EsEntregada => Parada?.Estado == EstadoEntrega.Entregada;
    public bool EsFallida => Parada?.Estado == EstadoEntrega.Fallida;

    public bool PuedeEditar =>
        !EstaOcupado && TieneTurnoAbierto &&
        Parada is not null &&
        ReglasEntrega.PuedeFallar(Parada);

    public ParadaDetalleViewModel(ParadasDatabase database, TurnoDatabase turnos)
    {
        this.database = database;
        this.turnos = turnos;
        RazonFallo = "";
        MensajeEstado = "";
    }

    partial void OnParadaIdChanged(int value)
    {
        _ = CargarAsync(value);
    }

    private async Task CargarAsync(int id)
    {
        EstaOcupado = true;
        MensajeEstado = "";

        try
        {
            TieneTurnoAbierto = await turnos.GetTurnoAbiertoAsync() is not null;
            Parada = await database.GetParadaAsync(id);
            RazonFallo = Parada?.RazonFallo ?? "";

            if (Parada is null)
                MensajeEstado = "No se encontró esta parada.";
            else if (EsEntregada)
                MensajeEstado = "Esta parada ya fue entregada y no puede modificarse.";
            else if (EsFallida)
                MensajeEstado = "Reintenta esta parada para devolverla a pendientes.";
            else if (!TieneTurnoAbierto)
                MensajeEstado = "Inicia un turno en Mi jornada para registrar una entrega o un fallo.";
        }
        catch (Exception ex)
        {
            MensajeEstado = $"No se pudo cargar el detalle: {ex.Message}";
        }
        finally
        {
            EstaOcupado = false;
        }
    }

    private bool PuedeMarcarEntregada()
    {
        return !EstaOcupado && TieneTurnoAbierto &&
            Parada is not null && ReglasEntrega.PuedeEntregar(Parada);
    }

    private bool PuedeMarcarFallida()
    {
        return PuedeEditar && !string.IsNullOrWhiteSpace(RazonFallo);
    }

    private bool PuedeReintentar()
    {
        return !EstaOcupado && Parada is not null && ReglasEntrega.PuedeReintentar(Parada);
    }

    [RelayCommand(CanExecute = nameof(PuedeMarcarEntregada))]
    private async Task MarcarEntregadaAsync()
    {
        await GuardarEstadoAsync(EstadoEntrega.Entregada);
    }

    [RelayCommand(CanExecute = nameof(PuedeMarcarFallida))]
    private async Task MarcarFallidaAsync()
    {
        await GuardarEstadoAsync(EstadoEntrega.Fallida);
    }

    [RelayCommand(CanExecute = nameof(PuedeReintentar))]
    private async Task ReintentarEntregaAsync()
    {
        await GuardarEstadoAsync(EstadoEntrega.Pendiente);
    }

    private async Task GuardarEstadoAsync(EstadoEntrega estado)
    {
        if (EstaOcupado || Parada is null)
            return;

        string? error = ReglasEntrega.ValidarCambio(Parada, estado, RazonFallo);
        if (error is not null)
        {
            MensajeEstado = error;
            return;
        }

        if (estado != EstadoEntrega.Pendiente && !TieneTurnoAbierto)
        {
            MensajeEstado = "Inicia un turno antes de registrar una entrega o un fallo.";
            return;
        }

        EstaOcupado = true;
        MensajeEstado = "";

        try
        {
            var cambio = new Parada
            {
                Id = Parada.Id,
                Numero = Parada.Numero,
                RepartidorId = Parada.RepartidorId,
                Cliente = Parada.Cliente,
                Direccion = Parada.Direccion,
                Latitud = Parada.Latitud,
                Longitud = Parada.Longitud,
                Estado = estado,
                RazonFallo = estado == EstadoEntrega.Fallida
                    ? RazonFallo.Trim()
                    : null,
                ModificadoEn = DateTime.Now
            };

#if DEBUG
            await Task.Delay(700);
#endif

            int resultado = await database.SaveParadaAsync(cambio);

            if (resultado != 1)
                throw new InvalidOperationException("No se guardó la parada.");

            Parada = cambio;
            RazonFallo = cambio.RazonFallo ?? "";

            if (Shell.Current.CurrentPage.BindingContext == this)
                await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            MensajeEstado = $"No se pudo completar la operación: {ex.Message}";
        }
        finally
        {
            EstaOcupado = false;
        }
    }
}
