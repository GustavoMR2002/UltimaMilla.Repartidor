using SQLite;
using UltimaMilla.Repartidor.Models;

namespace UltimaMilla.Repartidor.Services;

public class ParadasDatabase
{
    private readonly SesionActual sesion;
    private readonly Lazy<Task<SQLiteAsyncConnection>> conexion;
    private readonly SemaphoreSlim guardado = new(1, 1);

    public ParadasDatabase(SesionActual sesion)
    {
        this.sesion = sesion;

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

        return await database.Table<Parada>()
            .Where(p => p.RepartidorId == sesion.RepartidorId)
            .OrderBy(p => p.Numero)
            .ToListAsync();
    }

    public async Task<Parada?> GetParadaAsync(int id)
    {
        var database = await conexion.Value;

        return await database.Table<Parada>()
            .Where(p => p.Id == id &&
                        p.RepartidorId == sesion.RepartidorId)
            .FirstOrDefaultAsync();
    }

    public async Task<int> SaveParadaAsync(Parada parada)
    {
        await guardado.WaitAsync();

        try
        {
            var database = await conexion.Value;

            if (parada.Estado is not ("Pendiente" or "Entregada" or "Fallida"))
                throw new InvalidOperationException("El estado no es válido.");

            if (parada.Estado == "Fallida" &&
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

            var actual = await database.Table<Parada>()
                .Where(p => p.Id == parada.Id &&
                            p.RepartidorId == sesion.RepartidorId)
                .FirstOrDefaultAsync();

            if (actual is null)
                throw new InvalidOperationException("No se encontró la parada.");

            if (actual.Estado == "Entregada")
            {
                throw new InvalidOperationException(
                    "Una parada entregada no puede modificarse.");
            }

            if (parada.Estado == "Pendiente" && actual.Estado != "Fallida")
            {
                throw new InvalidOperationException(
                    "Solo una parada fallida puede reintentarse.");
            }

            actual.Estado = parada.Estado;
            actual.RazonFallo = parada.Estado == "Fallida"
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
