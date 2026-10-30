using ClinicSyncServer.Features.Egresos.Data;

namespace ClinicSyncServer.Features.Egresos
{
    public static class EgresosModule
    {
        public static IServiceCollection AddEgresos(
            this IServiceCollection services)
        {
            services.AddScoped<EgresosRepository>();

            services.AddScoped<
                IEgresosService,
                EgresosService>();

            return services;
        }

        public static WebApplication MapEgresosModule(
            this WebApplication app)
        {
            app.MapEgresosEndpoints();

            return app;
        }
    }
}
