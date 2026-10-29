using SQLite;
using UltimaMilla.Repartidor.Models;

namespace UltimaMilla.Repartidor.Services;

public class ParadasDatabase
{
    private readonly SesionActual sesion;
    private readonly TurnoDatabase turnos;
    private readonly Lazy<Task<SQLiteAsyncConnection>> conexion;
    private readonly SemaphoreSlim guardado = new(1, 1);

    public ParadasDatabase(SesionActual sesion, TurnoDatabase turnos)
    {
        this.sesion = sesion;
        this.turnos = turnos;

        conexion = new Lazy<Task<SQLiteAsyncConnection>>(async () =>
        {
            string ruta = Path.Combine(
                FileSystem.AppDataDirectory, "paradasUltMill.db3");

            var database = new SQLiteAsyncConnection(ruta);
            await database.CreateTableAsync<Parada>();
            return database;
        });
    }

    public async Task<List<Parada>> GetParadasAsync()
    {
        var database = await conexion.Value;
        int repartidorId = sesion.RepartidorId;

        return await database.Table<Parada>()
            .Where(p => p.RepartidorId == repartidorId)
            .OrderBy(p => p.Numero)
            .ToListAsync();
    }

    public async Task<Parada?> GetParadaAsync(int id)
    {
        var database = await conexion.Value;
        int repartidorId = sesion.RepartidorId;

        return await database.Table<Parada>()
            .Where(p => p.Id == id &&
                        p.RepartidorId == repartidorId)
            .FirstOrDefaultAsync();
    }

#if DEBUG
    public async Task CompletarCoordenadasPruebaAsync(List<Parada> paradas)
    {
        var coordenadas = new Dictionary<string, (double Latitud, double Longitud)>
        {
            ["Abarrotes El Mezquite"] = (25.42155, -100.99969),
            ["Papelería Los Pinos"] = (25.42499, -101.00873),
            ["Farmacia Santa Elena"] = (25.41347, -100.96407),
            ["Panadería La Espiga"] = (25.42099, -100.99974),
            ["Taller El Roble"] = (25.44042, -100.99129)
        };

        var database = await conexion.Value;
        int repartidorId = sesion.RepartidorId;

        foreach (var parada in paradas)
        {
            if (parada.Latitud is not null || parada.Longitud is not null ||
                !coordenadas.TryGetValue(parada.Cliente, out var punto))
                continue;

            int cambios = await database.ExecuteAsync(
                "UPDATE Parada SET Latitud = ?, Longitud = ? " +
                "WHERE Id = ? AND RepartidorId = ? AND Latitud IS NULL AND Longitud IS NULL",
                punto.Latitud, punto.Longitud, parada.Id, repartidorId);

            if (cambios == 1)
            {
                parada.Latitud = punto.Latitud;
                parada.Longitud = punto.Longitud;
            }
        }
    }
#endif

    public async Task<int> SaveParadaAsync(Parada parada)
    {
        await guardado.WaitAsync();

        try
        {
            var database = await conexion.Value;
            int repartidorId = sesion.RepartidorId;

            if (parada.Estado is not (EstadoEntrega.Pendiente or EstadoEntrega.Entregada or EstadoEntrega.Fallida))
                throw new InvalidOperationException("El estado no es válido.");

            if (parada.Estado == EstadoEntrega.Fallida &&
                string.IsNullOrWhiteSpace(parada.RazonFallo))
            {
                throw new InvalidOperationException(
                    "Debes indicar la razón del fallo.");
            }

            if (parada.Id == 0)
            {
                if (parada.RepartidorId != 0 &&
                    parada.RepartidorId != sesion.RepartidorId)
                {
                    throw new InvalidOperationException(
                        "La parada pertenece a otro repartidor.");
                }

                parada.RepartidorId = sesion.RepartidorId;
                parada.ModificadoEn = DateTime.Now;

                return await database.InsertAsync(parada);
            }

            int paradaId = parada.Id;
            var actual = await database.Table<Parada>()
                .Where(p => p.Id == paradaId &&
                            p.RepartidorId == repartidorId)
                .FirstOrDefaultAsync();

            if (actual is null)
                throw new InvalidOperationException("No se encontró la parada.");

            string? error = ReglasEntrega.ValidarCambio(actual, parada.Estado, parada.RazonFallo);
            if (error is not null)
                throw new InvalidOperationException(error);

            if (parada.Estado != EstadoEntrega.Pendiente &&
                await turnos.GetTurnoAbiertoAsync() is null)
                throw new InvalidOperationException("Inicia un turno antes de registrar una entrega o un fallo.");

            actual.Estado = parada.Estado;
            actual.RazonFallo = parada.Estado == EstadoEntrega.Fallida
                ? parada.RazonFallo!.Trim()
                : null;

            actual.ModificadoEn = DateTime.Now;

            int resultado = await database.UpdateAsync(actual);

            parada.RazonFallo = actual.RazonFallo;
            parada.ModificadoEn = actual.ModificadoEn;

            return resultado;
        }
        finally
        {
            guardado.Release();
        }
    }
}
