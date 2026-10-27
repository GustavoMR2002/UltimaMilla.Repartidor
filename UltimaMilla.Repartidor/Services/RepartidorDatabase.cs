using SQLite;
using Perfil = UltimaMilla.Repartidor.Models.Repartidor;

namespace UltimaMilla.Repartidor.Services;

public class RepartidorDatabase
{
    private readonly Lazy<Task<SQLiteAsyncConnection>> conexion = new(async () =>
    {
        string ruta = Path.Combine(FileSystem.AppDataDirectory, "paradasUltMill.db3");
        var database = new SQLiteAsyncConnection(ruta);
        await database.CreateTableAsync<Perfil>();

        if (await database.Table<Perfil>().CountAsync() == 0)
        {
            await database.InsertAsync(new Perfil { Nombre = "Miguel Torres" });
            await database.InsertAsync(new Perfil { Nombre = "Ana Delgado" });
        }

        return database;
    });

    public async Task<List<Perfil>> GetRepartidoresAsync()
    {
        var database = await conexion.Value;
        return await database.Table<Perfil>().OrderBy(r => r.Id).ToListAsync();
    }

    public async Task<Perfil?> GetRepartidorAsync(int id)
    {
        var database = await conexion.Value;
        return await database.Table<Perfil>().Where(r => r.Id == id).FirstOrDefaultAsync();
    }
}
