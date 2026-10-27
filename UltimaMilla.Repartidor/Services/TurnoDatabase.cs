using SQLite;
using UltimaMilla.Repartidor.Models;

namespace UltimaMilla.Repartidor.Services;

public class TurnoDatabase
{
    private readonly SesionActual sesion;
    private readonly SemaphoreSlim guardado = new(1, 1);
    private readonly Lazy<Task<SQLiteAsyncConnection>> conexion = new(async () =>
    {
        string ruta = Path.Combine(FileSystem.AppDataDirectory, "paradasUltMill.db3");
        var database = new SQLiteAsyncConnection(ruta);
        await database.CreateTableAsync<Turno>();
        return database;
    });

    public TurnoDatabase(SesionActual sesion)
    {
        this.sesion = sesion;
    }

    public async Task<Turno?> GetTurnoAbiertoAsync()
    {
        var database = await conexion.Value;
        int repartidorId = sesion.RepartidorId;
        return await database.Table<Turno>()
            .Where(t => t.RepartidorId == repartidorId && t.FechaFin == null)
            .FirstOrDefaultAsync();
    }

    public async Task<int> IniciarTurnoAsync()
    {
        await guardado.WaitAsync();
        try
        {
            if (sesion.RepartidorId == 0)
                throw new InvalidOperationException("Selecciona un repartidor.");

            if (await GetTurnoAbiertoAsync() is not null)
                return 0;

            var database = await conexion.Value;
            return await database.InsertAsync(new Turno
            {
                RepartidorId = sesion.RepartidorId,
                FechaInicio = DateTime.Now
            });
        }
        finally
        {
            guardado.Release();
        }
    }

    public async Task<int> CerrarTurnoAsync(Turno turno)
    {
        await guardado.WaitAsync();
        try
        {
            var actual = await GetTurnoAbiertoAsync();
            if (actual is null || actual.Id != turno.Id)
                throw new InvalidOperationException("No se encontró el turno abierto.");

            var database = await conexion.Value;
            actual.FechaFin = DateTime.Now;
            return await database.UpdateAsync(actual);
        }
        finally
        {
            guardado.Release();
        }
    }
}
