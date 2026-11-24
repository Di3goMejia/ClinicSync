using ClinicSyncServer.Common;

namespace ClinicSyncServer.Features.Finanzas
{
    /// <summary>
    /// 
    /// </summary>
    public enum Periodo { Hoy, Semana, Mes, Todos, Personalizado }
    /// <summary>
    /// 
    /// </summary>
    public enum Agrupacion { Hora, Dia, Semana, Mes }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Finanzas.RangoResuelto&gt;" />
    public record RangoResuelto(DateOnly? Desde, DateOnly? Hasta, Agrupacion Agrupacion);

    /// <summary>
    /// 
    /// </summary>
    public static class RangoFinanciero
    {
        /// <summary>
        /// The zona
        /// </summary>
        private static readonly TimeZoneInfo Zona =
            TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");

        /// <summary>
        /// Resolvers the specified periodo.
        /// </summary>
        /// <param name="periodo">The periodo.</param>
        /// <param name="desde">The desde.</param>
        /// <param name="hasta">The hasta.</param>
        /// <param name="ahoraUtc">The ahora UTC.</param>
        /// <returns></returns>
        public static RangoResuelto Resolver(Periodo periodo, DateOnly? desde, DateOnly? hasta, DateTime ahoraUtc)
        {
            var hoy = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(ahoraUtc, Zona));

            return periodo switch
            {
                Periodo.Hoy => new(hoy, hoy, Agrupacion.Hora),

                Periodo.Semana => Semana(hoy),

                Periodo.Mes => new(new DateOnly(hoy.Year, hoy.Month, 1),
                    new DateOnly(hoy.Year, hoy.Month, DateTime.DaysInMonth(hoy.Year, hoy.Month)),
                    Agrupacion.Dia),

                Periodo.Todos => new(null, hoy, Agrupacion.Mes),   // SQL usa el primer movimiento

                Periodo.Personalizado => Personalizado(desde, hasta),

                _ => throw new ArgumentOutOfRangeException(nameof(periodo))
            };
        }

        /// <summary>
        /// Semanas the specified hoy.
        /// </summary>
        /// <param name="hoy">The hoy.</param>
        /// <returns></returns>
        private static RangoResuelto Semana(DateOnly hoy)
        {
            int diasDesdeLunes = ((int)hoy.DayOfWeek + 6) % 7;
            var lunes = hoy.AddDays(-diasDesdeLunes);
            return new(lunes, lunes.AddDays(6), Agrupacion.Dia);
        }

        /// <summary>
        /// Personalizadoes the specified desde.
        /// </summary>
        /// <param name="desde">The desde.</param>
        /// <param name="hasta">The hasta.</param>
        /// <returns></returns>
        /// <exception cref="ReglaNegocioException">
        /// El rango personalizado requiere desde y hasta. - 50070
        /// or
        /// La fecha inicial no puede ser posterior a la final. - 50071
        /// or
        /// El rango no puede superar 366 días. - 50072
        /// </exception>
        private static RangoResuelto Personalizado(DateOnly? desde, DateOnly? hasta)
        {
            if (desde is null || hasta is null)
                throw new ReglaNegocioException("El rango personalizado requiere desde y hasta.", 50070);

            if (desde > hasta)
                throw new ReglaNegocioException("La fecha inicial no puede ser posterior a la final.", 50071);

            int dias = hasta.Value.DayNumber - desde.Value.DayNumber + 1;
            if (dias > 366)
                throw new ReglaNegocioException("El rango no puede superar 366 días.", 50072);

            var agrupacion = dias <= 1 ? Agrupacion.Hora
                           : dias <= 62 ? Agrupacion.Dia
                           : dias <= 366 ? Agrupacion.Semana
                           : Agrupacion.Mes;

            return new(desde, hasta, agrupacion);
        }
    }
}
