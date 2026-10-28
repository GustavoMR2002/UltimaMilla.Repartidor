using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace UltimaMilla.Repartidor.Services;

public class UbicacionService
{
    private bool permisoSolicitado;
    public string MensajeEstado { get; private set; } = "";

    public async Task<Location?> ObtenerUbicacionActualAsync(CancellationToken token = default)
    {
        MensajeEstado = "";

        try
        {
            token.ThrowIfCancellationRequested();
            var estado = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (estado != PermissionStatus.Granted && !permisoSolicitado)
            {
                permisoSolicitado = true;
                estado = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            token.ThrowIfCancellationRequested();

            if (estado != PermissionStatus.Granted)
            {
                MensajeEstado = "Permite el acceso a la ubicación en los ajustes de la app para ver distancias.";
                return null;
            }

            var solicitud = new GeolocationRequest(
                GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var ubicacion = await Geolocation.Default.GetLocationAsync(solicitud, token);

            if (ubicacion is null)
                MensajeEstado = "No se obtuvo tu ubicación. Acércate a una ventana y pulsa Actualizar.";

            return ubicacion;
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            throw;
        }
        catch (FeatureNotEnabledException)
        {
            MensajeEstado = "Activa la ubicación del teléfono para ver distancias.";
        }
        catch (FeatureNotSupportedException)
        {
            MensajeEstado = "Este dispositivo no permite obtener la ubicación.";
        }
        catch (PermissionException)
        {
            MensajeEstado = "Permite el acceso a la ubicación en los ajustes de la app para ver distancias.";
        }
        catch (Exception)
        {
            MensajeEstado = "No se pudo obtener tu ubicación. Inténtalo de nuevo con Actualizar.";
        }

        return null;
    }
}
