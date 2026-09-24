namespace UltimaMilla.Repartidor;

public partial class SplashPage : ContentPage
{
    private const int SplashDurationMs = 2000;
    private bool _iniciado;

    public SplashPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_iniciado)
            return;

        _iniciado = true;

        await Task.Delay(SplashDurationMs);

        if (Application.Current is not null &&
            Application.Current.Windows.Count > 0)
        {
            Application.Current.Windows[0].Page = new AppShell();
        }
    }
}