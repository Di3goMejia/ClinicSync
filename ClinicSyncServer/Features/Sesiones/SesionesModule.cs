using ClinicSyncServer.Features.Sesiones.Data;

namespace ClinicSyncServer.Features.Sesiones
{
    /// <summary>
    /// 
    /// </summary>
    public static class SesionesModule
    {
        /// <summary>
        /// Adds the sesiones.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddSesiones(this IServiceCollection services)
        {
            services.AddScoped<SesionesRepository>();
            services.AddScoped<ISesionService, SesionService>();

            return services;
        }
    }
}
