using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using UltimaMilla.Repartidor.Models;
using UltimaMilla.Repartidor.Services;

namespace UltimaMilla.Repartidor.ViewModels;

public partial class HistoricoViewModel : ObservableObject
{
    private readonly ParadasDatabase database;

    [ObservableProperty]
    public partial int EntregasEstaSemana { get; set; }

    [ObservableProperty]
    public partial int EntregasEsteMes { get; set; }

    [ObservableProperty]
    public partial bool EstaCargando { get; set; }

    [ObservableProperty]
    public partial string MensajeEstado { get; set; }

    public ObservableCollection<Parada> EntregasDelMes { get; } = new();

    public HistoricoViewModel(ParadasDatabase database)
    {
        this.database = database;
        MensajeEstado = "";
    }

    [RelayCommand]
    private async Task CargarHistoricoMesAsync()
    {
        if (EstaCargando)
            return;

        EstaCargando = true;
        MensajeEstado = "";
        EntregasDelMes.Clear();
        EntregasEstaSemana = 0;
        EntregasEsteMes = 0;

        try
        {
            var todas = await database.GetParadasAsync();
            var ahora = DateTime.Now;
            var desdeSemana = ahora.AddDays(-7);
            var desdeMes = new DateTime(ahora.Year, ahora.Month, 1);

            EntregasEstaSemana = todas.Count(p =>
                p.Estado == "Entregada" &&
                p.ModificadoEn >= desdeSemana && p.ModificadoEn <= ahora);

            var delMes = todas.Where(p =>
                    p.Estado == "Entregada" &&
                    p.ModificadoEn >= desdeMes && p.ModificadoEn <= ahora)
                .OrderByDescending(p => p.ModificadoEn);

            foreach (var parada in delMes)
                EntregasDelMes.Add(parada);

            EntregasEsteMes = EntregasDelMes.Count;
        }
        catch (Exception ex)
        {
            MensajeEstado = $"No se pudo cargar el histórico: {ex.Message}";
        }
        finally
        {
            EstaCargando = false;
        }
    }
}
