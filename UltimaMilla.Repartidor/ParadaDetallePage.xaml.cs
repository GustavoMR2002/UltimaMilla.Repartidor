using UltimaMilla.Repartidor.ViewModels;

namespace UltimaMilla.Repartidor;

public partial class ParadaDetallePage : ContentPage
{
    private readonly ParadaDetalleViewModel viewModel;

    public ParadaDetallePage(ParadaDetalleViewModel viewModel)
    {
        InitializeComponent();
        this.viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override bool OnBackButtonPressed()
    {
        if (viewModel.EstaOcupado)
            return true;

        return base.OnBackButtonPressed();
    }
}