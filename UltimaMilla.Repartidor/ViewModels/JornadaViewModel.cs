using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaMilla.Repartidor.Models;
using UltimaMilla.Repartidor.Helpers;
using UltimaMilla.Repartidor.Services;

namespace UltimaMilla.Repartidor.ViewModels;

public partial class JornadaViewModel : ObservableObject
{
    private readonly ParadasDatabase database;
    private readonly RepartidorDatabase repartidores;
    private readonly SesionActual sesion;
    private readonly TurnoDatabase turnos;
    private readonly UbicacionService ubicacion;
    private CancellationTokenSource? seguimiento;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TextoBotonTurno))]
    public partial Turno? TurnoActivo { get; set; }

    [ObservableProperty]
    public partial string TiempoTurno { get; set; }

    public string TextoBotonTurno =>
        TurnoActivo is null ? "Iniciar turno" : "Terminar turno";
    private List<Parada> todasLasParadas = new();

    [ObservableProperty]
    public partial string Saludo { get; set; }

    [ObservableProperty]
    public partial string Fecha { get; set; }

    [ObservableProperty]
    public partial int TotalEntregas { get; set; }

    [ObservableProperty]
    public partial int EntregasCompletadas { get; set; }

    [ObservableProperty]
    public partial int TotalPendientes { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AlternarTurnoCommand))]
    [NotifyCanExecuteChangedFor(nameof(CerrarSesionCommand))]
    public partial bool EstaCargando { get; set; }

    [ObservableProperty]
    public partial bool OcultarEntregadas { get; set; }

    [ObservableProperty]
    public partial string MensajeEstado { get; set; }

    [ObservableProperty]
    public partial string MensajeListaVacia { get; set; }

    [ObservableProperty]
    public partial int FiltroSeleccionado { get; set; }

    public IReadOnlyList<string> Filtros { get; } =
        new[] { "Todas", "Pendientes", "Entregadas", "Fallidas" };

    public ObservableCollection<Parada> Paradas { get; } = new();

    public JornadaViewModel(ParadasDatabase database,
        RepartidorDatabase repartidores, SesionActual sesion, TurnoDatabase turnos,
        UbicacionService ubicacion)
    {
        this.database = database;
        this.repartidores = repartidores;
        this.sesion = sesion;
        this.turnos = turnos;
        this.ubicacion = ubicacion;
        TiempoTurno = "Sin turno abierto";

        Saludo = "";
        Fecha = "";
        MensajeEstado = "";
        MensajeListaVacia = "No tienes paradas hoy";

    }

    partial void OnOcultarEntregadasChanged(bool value)
    {
        if (value && FiltroSeleccionado == 2)
            FiltroSeleccionado = 0;
        AplicarFiltro();
    }

    partial void OnFiltroSeleccionadoChanged(int value)
    {
        if (value == 2)
            OcultarEntregadas = false;
        AplicarFiltro();
    }

    private void ActualizarEncabezado(string nombre)
    {
        DateTime ahora = DateTime.Now;
        var cultura = CultureInfo.GetCultureInfo("es-MX");

        string saludo = ahora.Hour < 12
            ? "Buenos días"
            : ahora.Hour < 20
                ? "Buenas tardes"
                : "Buenas noches";

        Saludo = $"{saludo}, {nombre}";

        string fecha = ahora.ToString("dddd d 'de' MMMM", cultura);
        Fecha = char.ToUpper(fecha[0], cultura) + fecha.Substring(1);
    }

    public void IniciarSeguimiento()
    {
        seguimiento?.Dispose();
        seguimiento = new CancellationTokenSource();
    }

    public void DetenerSeguimiento()
    {
        seguimiento?.Cancel();
    }

    [RelayCommand]
    private async Task RefrescarAsync()
    {
        if (EstaCargando)
            return;

        var token = seguimiento?.Token ?? CancellationToken.None;
        if (token.IsCancellationRequested)
            return;

        EstaCargando = true;
        MensajeEstado = "";

        try
        {

            if (sesion.RepartidorId == 0)
            {
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            var repartidor = await repartidores.GetRepartidorAsync(sesion.RepartidorId);
            if (repartidor is null)
            {
                Preferences.Default.Remove("RepartidorActualId");
                sesion.RepartidorId = 0;
                await Shell.Current.GoToAsync("//LoginPage");
                return;
            }

            ActualizarEncabezado(repartidor.Nombre);
            TurnoActivo = await turnos.GetTurnoAbiertoAsync();
            ActualizarTiempoTurno();

            var resultado = await database.GetParadasAsync();

#if DEBUG
            if (resultado.Count == 0)
            {
                await SembrarParadasAsync();
                resultado = await database.GetParadasAsync();
            }
            await database.CompletarCoordenadasPruebaAsync(resultado);
#endif

            token.ThrowIfCancellationRequested();
            var miUbicacion = await ubicacion.ObtenerUbicacionActualAsync(token);
            token.ThrowIfCancellationRequested();

            foreach (var parada in resultado)
            {
                if (miUbicacion is not null &&
                    parada.Latitud is double latitud && parada.Longitud is double longitud)
                {
                    double km = Distancia.EnKilometros(
                        miUbicacion.Latitude, miUbicacion.Longitude, latitud, longitud);
                    parada.DistanciaKm = km;
                    parada.DistanciaTexto = $"{km:F1} km";
                }
                else
                {
                    parada.DistanciaKm = null;
                    parada.DistanciaTexto = "Sin ubicación";
                }
            }

            MensajeEstado = ubicacion.MensajeEstado;
            todasLasParadas = resultado
                .OrderBy(p => p.DistanciaKm ?? double.MaxValue)
                .ThenBy(p => p.Numero)
                .ToList();
            TotalEntregas = resultado.Count;
            EntregasCompletadas =
                resultado.Count(p => p.Estado == EstadoEntrega.Entregada);
            TotalPendientes =
                resultado.Count(p => p.Estado == EstadoEntrega.Pendiente);

            AplicarFiltro();

        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            MensajeEstado = $"No se pudieron cargar las paradas: {ex.Message}";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    private bool PuedeCambiarTurno() => !EstaCargando;

    [RelayCommand(CanExecute = nameof(PuedeCambiarTurno))]
    private async Task AlternarTurnoAsync()
    {
        if (EstaCargando)
            return;

        EstaCargando = true;
        MensajeEstado = "";
        try
        {
            if (TurnoActivo is null)
                await turnos.IniciarTurnoAsync();
            else
                await turnos.CerrarTurnoAsync(TurnoActivo);

            TurnoActivo = await turnos.GetTurnoAbiertoAsync();
            ActualizarTiempoTurno();
        }
        catch (Exception ex)
        {
            MensajeEstado = $"No se pudo actualizar el turno: {ex.Message}";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    [RelayCommand(CanExecute = nameof(PuedeCambiarTurno))]
    private async Task CerrarSesionAsync()
    {
        if (EstaCargando)
            return;

        EstaCargando = true;
        try
        {
            sesion.RepartidorId = 0;
            TurnoActivo = null;
            ActualizarTiempoTurno();
            Paradas.Clear();
            todasLasParadas.Clear();
            TotalEntregas = 0;
            EntregasCompletadas = 0;
            TotalPendientes = 0;
            Saludo = "";
            await Shell.Current.GoToAsync("//LoginPage");
        }
        catch (Exception ex)
        {
            MensajeEstado = $"No se pudo cerrar sesión: {ex.Message}";
        }
        finally
        {
            EstaCargando = false;
        }
    }

    public void ActualizarTiempoTurno()
    {
        if (TurnoActivo is null)
        {
            TiempoTurno = "Sin turno abierto";
            return;
        }

        var transcurrido = DateTime.Now - TurnoActivo.FechaInicio;
        int minutos = Math.Max(0, (int)transcurrido.TotalMinutes);
        TiempoTurno = $"Turno: {minutos / 60} h {minutos % 60:00} min";
    }

    private void AplicarFiltro()
    {
        Paradas.Clear();

        EstadoEntrega? estado = FiltroSeleccionado switch
        {
            1 => EstadoEntrega.Pendiente,
            2 => EstadoEntrega.Entregada,
            3 => EstadoEntrega.Fallida,
            _ => null
        };

        foreach (var parada in todasLasParadas)
        {
            if (estado is not null && parada.Estado != estado.Value)
                continue;
            if (OcultarEntregadas && parada.Estado == EstadoEntrega.Entregada)
                continue;
            Paradas.Add(parada);
        }

        MensajeListaVacia = todasLasParadas.Count == 0
            ? "No tienes paradas hoy"
            : "No hay paradas con el filtro seleccionado";
    }

    private async Task SembrarParadasAsync()
    {
        var asignadas = new List<Parada>
        {
            new()
            {
                Numero = 1,
                Cliente = "Abarrotes El Mezquite",
                Direccion = "Calle Nogal 120, Saltillo",
                Estado = EstadoEntrega.Pendiente
            },
            new()
            {
                Numero = 2,
                Cliente = "Papelería Los Pinos",
                Direccion = "Calle Pino 245, Saltillo",
                Estado = EstadoEntrega.Entregada
            },
            new()
            {
                Numero = 3,
                Cliente = "Farmacia Santa Elena",
                Direccion = "Av. del Valle 380, Saltillo",
                Estado = EstadoEntrega.Pendiente
            },
            new()
            {
                Numero = 4,
                Cliente = "Panadería La Espiga",
                Direccion = "Calle Trigo 56, Saltillo",
                Estado = EstadoEntrega.Entregada
            },
            new()
            {
                Numero = 5,
                Cliente = "Taller El Roble",
                Direccion = "Calle Roble 710, Saltillo",
                Estado = EstadoEntrega.Pendiente
            }
        };

        foreach (var parada in asignadas)
            await database.SaveParadaAsync(parada);
    }
}
