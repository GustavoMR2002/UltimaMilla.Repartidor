namespace UltimaMilla.Repartidor.Helpers;

public static class Distancia
{
    public static double EnKilometros(double lat1, double lon1, double lat2, double lon2)
    {
        const double radioTierra = 6371;
        double dLat = ARadianes(lat2 - lat1);
        double dLon = ARadianes(lon2 - lon1);

        double a = Math.Pow(Math.Sin(dLat / 2), 2) +
            Math.Cos(ARadianes(lat1)) * Math.Cos(ARadianes(lat2)) *
            Math.Pow(Math.Sin(dLon / 2), 2);

        a = Math.Clamp(a, 0, 1);
        return radioTierra * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }

    private static double ARadianes(double grados) => grados * Math.PI / 180;
}
