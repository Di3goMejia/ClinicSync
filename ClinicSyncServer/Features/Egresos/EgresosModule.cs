using ClinicSyncServer.Features.Egresos.Data;

namespace ClinicSyncServer.Features.Egresos
{
    /// <summary>
    /// 
    /// </summary>
    public static class EgresosModule
    {
        /// <summary>
        /// Adds the egresos.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddEgresos(
            this IServiceCollection services)
        {
            services.AddScoped<EgresosRepository>();

            services.AddScoped<
                IEgresosService,
                EgresosService>();

            return services;
        }

        /// <summary>
        /// Maps the egresos module.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <returns></returns>
        public static WebApplication MapEgresosModule(
            this WebApplication app)
        {
            app.MapEgresosEndpoints();

            return app;
        }
    }
}
