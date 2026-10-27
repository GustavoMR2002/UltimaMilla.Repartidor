using SQLite;

namespace UltimaMilla.Repartidor.Models;

public class Repartidor
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
}
