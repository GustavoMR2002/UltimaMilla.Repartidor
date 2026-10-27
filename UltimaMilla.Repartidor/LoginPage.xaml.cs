using UltimaMilla.Repartidor.Services;
using UltimaMilla.Repartidor.ViewModels;
using Perfil = UltimaMilla.Repartidor.Models.Repartidor;

namespace UltimaMilla.Repartidor;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel viewModel;
    private readonly SesionActual sesion;
    private bool navegando;

    public LoginPage(LoginViewModel viewModel, SesionActual sesion)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
        this.sesion = sesion;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        sesion.RepartidorId = 0;
        Perfiles.SelectedItem = null;
        await viewModel.CargarAsync();
    }

    private async void OnRepartidorSeleccionado(object sender, SelectionChangedEventArgs e)
    {
        if (navegando || e.CurrentSelection.FirstOrDefault() is not Perfil perfil)
            return;

        navegando = true;
        Perfiles.IsEnabled = false;
        try
        {
            Preferences.Default.Set("RepartidorActualId", perfil.Id);
            sesion.RepartidorId = perfil.Id;
            await Shell.Current.GoToAsync("//JornadaPage");
        }
        catch (Exception ex)
        {
            sesion.RepartidorId = 0;
            viewModel.MensajeEstado = $"No se pudo iniciar sesión: {ex.Message}";
        }
        finally
        {
            Perfiles.SelectedItem = null;
            Perfiles.IsEnabled = true;
            navegando = false;
        }
    }
}
