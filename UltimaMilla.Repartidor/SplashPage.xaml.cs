using Microsoft.Extensions.DependencyInjection;

namespace UltimaMilla.Repartidor;

public partial class SplashPage : ContentPage
{
    private readonly IServiceProvider servicios;
    private bool iniciado;

    public SplashPage(IServiceProvider servicios)
    {
        InitializeComponent();
        this.servicios = servicios;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (iniciado)
            return;

        iniciado = true;

        await Task.Delay(2000);

        var ventana = Window;

        if (ventana is not null)
        {
            ventana.Page = servicios.GetRequiredService<AppShell>();
        }
    }
}