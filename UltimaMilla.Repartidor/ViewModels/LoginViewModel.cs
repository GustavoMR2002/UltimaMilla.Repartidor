using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using UltimaMilla.Repartidor.Services;
using Perfil = UltimaMilla.Repartidor.Models.Repartidor;

namespace UltimaMilla.Repartidor.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly RepartidorDatabase database;

    [ObservableProperty]
    public partial bool EstaCargando { get; set; }

    [ObservableProperty]
    public partial string MensajeEstado { get; set; }

    public ObservableCollection<Perfil> Repartidores { get; } = new();

    public LoginViewModel(RepartidorDatabase database)
    {
        this.database = database;
        MensajeEstado = "";
    }

    public async Task CargarAsync()
    {
        if (EstaCargando)
            return;

        EstaCargando = true;
        MensajeEstado = "";

        try
        {
            var perfiles = await database.GetRepartidoresAsync();
            Repartidores.Clear();
            foreach (var perfil in perfiles)
                Repartidores.Add(perfil);
        }
        catch (Exception ex)
        {
            MensajeEstado = $"No se pudieron cargar los perfiles: {ex.Message}";
        }
        finally
        {
            EstaCargando = false;
        }
    }
}
