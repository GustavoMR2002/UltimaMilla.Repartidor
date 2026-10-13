using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaMilla.Repartidor.Models;

namespace UltimaMilla.Repartidor.ViewModels;

public partial class JornadaViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Saludo { get; set; }

    [ObservableProperty]
    public partial string Fecha { get; set; }

    [ObservableProperty]
    public partial int TotalEntregas { get; set; }

    [ObservableProperty]
    public partial int EntregasCompletadas { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PuedeModificar))]
    public partial bool EstaCargando { get; set; }

    [ObservableProperty]
    public partial string MensajeEstado { get; set; }

    public bool PuedeModificar => !EstaCargando;

    public ObservableCollection<Parada> Paradas { get; } = new();

    public JornadaViewModel()
    {
        Saludo = "";
        Fecha = "";
        MensajeEstado = "";

        Paradas.CollectionChanged += (_, _) => ActualizarResumen();

        ActualizarEncabezado();
        CargarParadas();
    }

    private void ActualizarEncabezado()
    {
        DateTime ahora = DateTime.Now;
        var cultura = CultureInfo.GetCultureInfo("es-MX");

        string saludo;

        if (ahora.Hour < 12)
        {
            saludo = "Buenos días";
        }
        else if (ahora.Hour < 20)
        {
            saludo = "Buenas tardes";
        }
        else
        {
            saludo = "Buenas noches";
        }

        Saludo = $"{saludo}, Gustavo";

        string fecha = ahora.ToString(
            "dddd d 'de' MMMM", cultura);

        Fecha = char.ToUpper(fecha[0], cultura) + fecha.Substring(1);
    }

    private void CargarParadas()
    {
        Paradas.Clear();

        Paradas.Add(new Parada
        {
            Numero = 1,
            Cliente = "Abarrotes El Mezquite",
            Direccion = "Calle Nogal 120, Saltillo",
            Estado = "Pendiente"
        });

        Paradas.Add(new Parada
        {
            Numero = 2,
            Cliente = "Papelería Los Pinos",
            Direccion = "Calle Pino 245, Saltillo",
            Estado = "Entregada"
        });

        Paradas.Add(new Parada
        {
            Numero = 3,
            Cliente = "Farmacia Santa Elena",
            Direccion = "Av. del Valle 380, Saltillo",
            Estado = "Pendiente"
        });

        Paradas.Add(new Parada
        {
            Numero = 4,
            Cliente = "Panadería La Espiga",
            Direccion = "Calle Trigo 56, Saltillo",
            Estado = "Entregada"
        });

        Paradas.Add(new Parada
        {
            Numero = 5,
            Cliente = "Taller El Roble",
            Direccion = "Calle Roble 710, Saltillo",
            Estado = "Pendiente"
        });
    }

    private void ActualizarResumen()
    {
        TotalEntregas = Paradas.Count;
        EntregasCompletadas =
            Paradas.Count(p => p.Estado == "Entregada");
    }

    [RelayCommand]
    private async Task RefrescarAsync()
    {
        MensajeEstado = "";
        EstaCargando = true;

        try
        {
            await Task.Delay(1000);

            CargarParadas();
            ActualizarEncabezado();

            MensajeEstado = "Datos actualizados correctamente";
        }
        finally
        {
            EstaCargando = false;
        }

        await Task.Delay(2500);
        MensajeEstado = "";
    }

    [RelayCommand]
    private void AgregarPrueba()
    {
        if (EstaCargando)
            return;

        int numero = Paradas.Count == 0
            ? 1
            : Paradas.Max(p => p.Numero) + 1;

        Paradas.Add(new Parada
        {
            Numero = numero,
            Cliente = $"Cliente de prueba {numero}",
            Direccion = "Dirección de prueba, Saltillo",
            Estado = "Pendiente"
        });
    }

    [RelayCommand]
    private void Vaciar()
    {
        if (EstaCargando)
            return;

        Paradas.Clear();
    }
}
