using SQLite;

namespace UltimaMilla.Repartidor.Models;

public class Turno
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int RepartidorId { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
}
