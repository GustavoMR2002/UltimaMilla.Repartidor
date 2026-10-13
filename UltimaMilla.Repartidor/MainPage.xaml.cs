namespace UltimaMilla.Repartidor;

public partial class MainPage : ContentPage
{
    private int count = 0;
    private bool fondoAlternativo = false;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object? sender, EventArgs e)
    {
        count++;
        ActualizarContador();
    }

    private void OnRestarClicked(object? sender, EventArgs e)
    {
        if (count > 0)
        {
            count--;
        }

        ActualizarContador();
    }

    private void ActualizarContador()
    {
        ContadorLabel.Text = $"Contador: {count}";
        RestarBtn.IsEnabled = count > 0;

        SemanticScreenReader.Announce(ContadorLabel.Text);
    }

    private void OnCambiarFondoClicked(object? sender, EventArgs e)
    {
        fondoAlternativo = !fondoAlternativo;

        if (fondoAlternativo)
        {
            BackgroundColor = Application.Current?.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#153448")
                : Color.FromArgb("#D6EAF8");
        }
        else
        {
            ClearValue(BackgroundColorProperty);
        }
    }
}
