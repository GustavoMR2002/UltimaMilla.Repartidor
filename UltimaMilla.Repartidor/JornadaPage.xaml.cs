using System.Globalization;

namespace UltimaMilla.Repartidor;

public partial class JornadaPage : ContentPage
{
    public JornadaPage()
    {
        InitializeComponent();
        MostrarDatosDeHoy();
    }

    private void MostrarDatosDeHoy()
    {
        string repartidor = "Gustavo";
        int entregasDeHoy = 8;
        int entregasCompletadas = 5;

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

        TotalLabel.Text = entregasDeHoy.ToString();
        CompletadasLabel.Text = entregasCompletadas.ToString();
    }
}