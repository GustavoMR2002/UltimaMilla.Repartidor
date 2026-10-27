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

    public bool EsEntregada => Parada?.Estado == "Entregada";
    public bool EsFallida => Parada?.Estado == "Fallida";

    public bool PuedeEditar =>
        !EstaOcupado &&
        Parada is not null &&
        !EsEntregada;

    public ParadaDetalleViewModel(ParadasDatabase database)
    {
        this.database = database;
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
            Parada = await database.GetParadaAsync(id);
            RazonFallo = Parada?.RazonFallo ?? "";

            if (Parada is null)
                MensajeEstado = "No se encontró esta parada.";
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
        return PuedeEditar;
    }

    private bool PuedeMarcarFallida()
    {
        return PuedeEditar && !string.IsNullOrWhiteSpace(RazonFallo);
    }

    private bool PuedeReintentar()
    {
        return PuedeEditar && EsFallida;
    }

    [RelayCommand(CanExecute = nameof(PuedeMarcarEntregada))]
    private async Task MarcarEntregadaAsync()
    {
        if (!PuedeMarcarEntregada())
            return;

        await GuardarEstadoAsync("Entregada");
    }

    [RelayCommand(CanExecute = nameof(PuedeMarcarFallida))]
    private async Task MarcarFallidaAsync()
    {
        if (!PuedeMarcarFallida())
            return;

        await GuardarEstadoAsync("Fallida");
    }

    [RelayCommand(CanExecute = nameof(PuedeReintentar))]
    private async Task ReintentarEntregaAsync()
    {
        if (!PuedeReintentar())
            return;

        await GuardarEstadoAsync("Pendiente");
    }

    private async Task GuardarEstadoAsync(string estado)
    {
        if (!PuedeEditar || Parada is null)
            return;

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
                Estado = estado,
                RazonFallo = estado == "Fallida"
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
