using ClinicSyncServer.Features.Inventario.Data;

namespace ClinicSyncServer.Features.Inventario
{
    public static class InventarioModule
    {
        public static IServiceCollection AddInventario(this IServiceCollection services)
        {
            services.AddScoped<InventarioRepository>();
            services.AddScoped<IInventarioService, InventarioService>();
            return services;
        }

        public static WebApplication MapInventario(this WebApplication app)
        {
            app.MapInventarioEndpoints();
            return app;
        }
    }
}
