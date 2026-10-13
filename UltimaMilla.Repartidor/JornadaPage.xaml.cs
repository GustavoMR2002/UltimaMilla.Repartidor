using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using UltimaMilla.Repartidor.Models;

namespace UltimaMilla.Repartidor;

public partial class JornadaPage : ContentPage
{
    public ObservableCollection<Parada> Paradas { get; } = new();

    public JornadaPage()
    {
        InitializeComponent();

        ParadasView.ItemsSource = Paradas;

        Paradas.CollectionChanged += (_, _) => ActualizarResumen();

        CargarParadasDeEjemplo();
        MostrarDatosDeHoy();
    }

    private void MostrarDatosDeHoy()
    {
        string repartidor = "Gustavo";
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

        SaludoLabel.Text = $"{saludo}, {repartidor}";

        string fecha = ahora.ToString(
            "dddd d 'de' MMMM", cultura);

        FechaLabel.Text =
            char.ToUpper(fecha[0], cultura) + fecha.Substring(1);

        ActualizarResumen();
    }

    private void CargarParadasDeEjemplo()
    {
        // Evita duplicar las paradas al pulsar Actualizar.
        Paradas.Clear();

        // Datos ficticios para el ejercicio.
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
        TotalLabel.Text = Paradas.Count.ToString();

        CompletadasLabel.Text =
            Paradas.Count(p => p.Estado == "Entregada").ToString();
    }

    private async void OnParadaTapped(object? sender, TappedEventArgs e)
    {
        if (e.Parameter is Parada parada)
        {
            await DisplayAlertAsync(
                "Detalle de la parada",
                $"Cliente: {parada.Cliente}\n" +
                $"Dirección: {parada.Direccion}\n" +
                $"Estado: {parada.Estado}",
                "Aceptar");
        }
    }

    private async void OnActualizarClicked(object? sender, EventArgs e)
    {
        ActualizarBtn.IsEnabled = false;
        PruebasPanel.IsEnabled = false;

        try
        {
            // Simula una carga; todavía no usamos una base de datos.
            await Task.Delay(1000);

            CargarParadasDeEjemplo();
            MostrarDatosDeHoy();

            await DisplayAlertAsync(
                "Actualización",
                "Las paradas se actualizaron correctamente.",
                "Aceptar");
        }
        finally
        {
            ActualizarBtn.IsEnabled = true;
            PruebasPanel.IsEnabled = true;
        }
    }

    private void OnAgregarPruebaClicked(object? sender, EventArgs e)
    {
        int siguienteNumero = Paradas.Count == 0
            ? 1
            : Paradas.Max(p => p.Numero) + 1;

        Paradas.Add(new Parada
        {
            Numero = siguienteNumero,
            Cliente = $"Cliente de prueba {siguienteNumero}",
            Direccion = "Dirección de prueba, Saltillo",
            Estado = "Pendiente"
        });
    }

    private void OnVaciarClicked(object? sender, EventArgs e)
    {
        Paradas.Clear();
    }
}