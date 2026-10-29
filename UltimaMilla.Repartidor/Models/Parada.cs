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
    public EstadoEntrega Estado { get; set; } = EstadoEntrega.Pendiente;
    public string? RazonFallo { get; set; }
    public double? Latitud { get; set; }
    public double? Longitud { get; set; }

    [Ignore]
    public double? DistanciaKm { get; set; }

    [Ignore]
    public string DistanciaTexto { get; set; } = "Sin ubicación";

    [Ignore]
    public string TiempoEnFallida
    {
        get
        {
            if (Estado != EstadoEntrega.Fallida)
                return "";

            int minutos = Math.Max(0, (int)(DateTime.Now - ModificadoEn).TotalMinutes);
            if (minutos < 1)
                return "Fallida hace menos de un minuto";
            if (minutos < 60)
                return $"Fallida desde hace {minutos} min";
            return $"Fallida desde hace {minutos / 60} h {minutos % 60:00} min";
        }
    }

    public DateTime ModificadoEn { get; set; } = DateTime.Now;
}
