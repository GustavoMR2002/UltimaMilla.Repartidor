using UltimaMilla.Repartidor.ViewModels;

namespace UltimaMilla.Repartidor;

public partial class HistoricoPage : ContentPage
{
    private readonly HistoricoViewModel viewModel;

    public HistoricoPage(HistoricoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = this.viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        var comando = viewModel.CargarHistoricoMesCommand;
        if (comando.IsRunning && comando.ExecutionTask is Task tarea)
            await tarea;
        await comando.ExecuteAsync(null);
    }
}
