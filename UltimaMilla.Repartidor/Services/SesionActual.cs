namespace UltimaMilla.Repartidor.Services;

public class SesionActual
{
    public int RepartidorId { get; set; } =
        Preferences.Default.Get("RepartidorActualId", 0);
}
