using SQLite;

namespace UltimaMilla.Repartidor.Models;

[StoreAsText]
public enum EstadoEntrega
{
    Pendiente = 0,
    Entregada = 1,
    Fallida = 2
}
