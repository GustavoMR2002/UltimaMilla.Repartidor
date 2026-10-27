using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaMilla.Repartidor.Models;
using UltimaMilla.Repartidor.Services;

namespace UltimaMilla.Repartidor.ViewModels;

public partial class JornadaViewModel : ObservableObject
{
    private readonly ParadasDatabase database;
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
    public partial bool EstaCargando { get; set; }

    [ObservableProperty]
    public partial bool OcultarEntregadas { get; set; }

    [ObservableProperty]
    public partial string MensajeEstado { get; set; }

    [ObservableProperty]
    public partial string MensajeListaVacia { get; set; }

    public ObservableCollection<Parada> Paradas { get; } = new();

    public JornadaViewModel(ParadasDatabase database)
    {
        this.database = database;

        Saludo = "";
        Fecha = "";
        MensajeEstado = "";
        MensajeListaVacia = "No tienes paradas hoy";

        ActualizarEncabezado();
    }

    partial void OnOcultarEntregadasChanged(bool value)
    {
        AplicarFiltro();
    }

    private void ActualizarEncabezado()
    {
        DateTime ahora = DateTime.Now;
        var cultura = CultureInfo.GetCultureInfo("es-MX");

        string saludo = ahora.Hour < 12
            ? "Buenos días"
            : ahora.Hour < 20
                ? "Buenas tardes"
                : "Buenas noches";

        Saludo = $"{saludo}, Gustavo";

        string fecha = ahora.ToString("dddd d 'de' MMMM", cultura);
        Fecha = char.ToUpper(fecha[0], cultura) + fecha.Substring(1);
    }

    [RelayCommand]
    private async Task RefrescarAsync()
    {
        if (EstaCargando)
            return;

        EstaCargando = true;
        MensajeEstado = "";

        try
        {
            var resultado = await database.GetParadasAsync();

#if DEBUG
            if (resultado.Count == 0)
            {
                await SembrarParadasAsync();
                resultado = await database.GetParadasAsync();
            }
#endif

            todasLasParadas = resultado;
            TotalEntregas = resultado.Count;
            EntregasCompletadas =
                resultado.Count(p => p.Estado == "Entregada");

            AplicarFiltro();
            ActualizarEncabezado();
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

    private void AplicarFiltro()
    {
        Paradas.Clear();

        foreach (var parada in todasLasParadas)
        {
            if (!OcultarEntregadas || parada.Estado != "Entregada")
                Paradas.Add(parada);
        }

        MensajeListaVacia = OcultarEntregadas
            ? "No hay paradas pendientes ni fallidas"
            : "No tienes paradas hoy";
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
                Estado = "Pendiente"
            },
            new()
            {
                Numero = 2,
                Cliente = "Papelería Los Pinos",
                Direccion = "Calle Pino 245, Saltillo",
                Estado = "Entregada"
            },
            new()
            {
                Numero = 3,
                Cliente = "Farmacia Santa Elena",
                Direccion = "Av. del Valle 380, Saltillo",
                Estado = "Pendiente"
            },
            new()
            {
                Numero = 4,
                Cliente = "Panadería La Espiga",
                Direccion = "Calle Trigo 56, Saltillo",
                Estado = "Entregada"
            },
            new()
            {
                Numero = 5,
                Cliente = "Taller El Roble",
                Direccion = "Calle Roble 710, Saltillo",
                Estado = "Pendiente"
            }
        };

        foreach (var parada in asignadas)
            await database.SaveParadaAsync(parada);
    }
}
