using ClinicSyncServer.Features.Inventario.Data;
using ClinicSyncServer.Features.Inventario.Imagenes;

namespace ClinicSyncServer.Features.Inventario
{
    /// <summary>
    /// 
    /// </summary>
    public static class InventarioModule
    {
        /// <summary>
        /// Adds the inventario.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddInventario(this IServiceCollection services)
        {
            services.AddScoped<InventarioRepository>();
            services.AddScoped<IInventarioService, InventarioService>();
            services.AddSingleton<IAlmacenamientoImagenesService, AlmacenamientoImagenesService>();
            return services;
        }

        /// <summary>
        /// Maps the inventario.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <returns></returns>
        public static WebApplication MapInventario(this WebApplication app)
        {
            app.MapInventarioEndpoints();
            return app;
        }
    }
}
