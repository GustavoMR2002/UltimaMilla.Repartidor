using Microsoft.Extensions.DependencyInjection;

namespace UltimaMilla.Repartidor;

public partial class App : Application
{
    private readonly IServiceProvider servicios;

    public App(IServiceProvider servicios)
    {
        InitializeComponent();
        this.servicios = servicios;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var splash = servicios.GetRequiredService<SplashPage>();
        return new Window(splash);
    }
}