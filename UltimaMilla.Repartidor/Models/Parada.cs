using SQLite;

namespace UltimaMilla.Repartidor.Models;

public class Parada
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int Numero { get; set; }
    public int RepartidorId { get; set; }
    public string Cliente { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string Estado { get; set; } = "Pendiente";
    public string? RazonFallo { get; set; }
    public DateTime ModificadoEn { get; set; } = DateTime.Now;
}
