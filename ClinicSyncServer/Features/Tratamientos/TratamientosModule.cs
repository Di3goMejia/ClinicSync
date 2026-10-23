using ClinicSyncServer.Features.Tratamientos.Data;
using ClinicSyncServer.Features.Usuarios;

namespace ClinicSyncServer.Features.Tratamientos
{
    public static class TratamientosModule
    {
        public static IServiceCollection AddTratamientos(this IServiceCollection services)
        {
            services.AddScoped<TratamientosRepository>();
            services.AddScoped<ITratamientosService, TratamientosService>();

            return services;
        }

        public static WebApplication MapTratamientos(this WebApplication app)
        {
            app.MapTratamientosEndpoints();
            return app;
        }
    }
}
