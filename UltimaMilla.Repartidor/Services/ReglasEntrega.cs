using UltimaMilla.Repartidor.Models;

namespace UltimaMilla.Repartidor.Services;

public static class ReglasEntrega
{
    public static bool PuedeEntregar(Parada parada) =>
        parada.Estado == EstadoEntrega.Pendiente;

    public static bool PuedeFallar(Parada parada) =>
        parada.Estado == EstadoEntrega.Pendiente;

    public static bool PuedeReintentar(Parada parada) =>
        parada.Estado == EstadoEntrega.Fallida;

    public static string? ValidarCambio(Parada parada, EstadoEntrega destino, string? razon)
    {
        if (parada.Estado == EstadoEntrega.Entregada)
            return "Una parada entregada es de solo lectura.";

        switch (destino)
        {
            case EstadoEntrega.Entregada:
                return PuedeEntregar(parada)
                    ? null : "Reintenta la parada antes de marcarla como entregada.";
            case EstadoEntrega.Fallida:
                if (!PuedeFallar(parada))
                    return "Solo una parada pendiente puede marcarse como fallida.";
                return string.IsNullOrWhiteSpace(razon)
                    ? "Debes indicar la razón del fallo." : null;
            case EstadoEntrega.Pendiente:
                return PuedeReintentar(parada)
                    ? null : "Solo una parada fallida puede reintentarse.";
            default:
                return "El estado no es válido.";
        }
    }
}
