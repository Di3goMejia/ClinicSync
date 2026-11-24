using ClinicSyncServer.Features.Tratamientos.Data;

namespace ClinicSyncServer.Features.Tratamientos
{
    /// <summary>
    /// 
    /// </summary>
    public static class TratamientosModule
    {
        /// <summary>
        /// Adds the tratamientos.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddTratamientos(
            this IServiceCollection services)
        {
            services.AddScoped<TratamientosRepository>();
            services.AddScoped<ITratamientosService, TratamientoService>();

            return services;
        }

        /// <summary>
        /// Maps the tratamientos.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <returns></returns>
        public static WebApplication MapTratamientos(
            this WebApplication app)
        {
            app.MapTratamientosEndpoints();

            return app;
        }
    }
}
