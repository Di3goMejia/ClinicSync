using ClinicSyncServer.Features.Sesiones.Data;

namespace ClinicSyncServer.Features.Sesiones
{
    public static class SesionesModule
    {
        public static IServiceCollection AddSesiones(this IServiceCollection services)
        {
            services.AddScoped<SesionesRepository>();
            services.AddScoped<ISesionService, SesionService>();

            return services;
        }
    }
}
