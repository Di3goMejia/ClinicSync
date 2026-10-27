using ClinicSyncServer.Features.Inventario.Data;
using ClinicSyncServer.Features.Inventario.Imagenes;

namespace ClinicSyncServer.Features.Inventario
{
    public static class InventarioModule
    {
        public static IServiceCollection AddInventario(this IServiceCollection services)
        {
            services.AddScoped<InventarioRepository>();
            services.AddScoped<IInventarioService, InventarioService>();
            services.AddSingleton<IAlmacenamientoImagenesService, AlmacenamientoImagenesService>();
            return services;
        }

        public static WebApplication MapInventario(this WebApplication app)
        {
            app.MapInventarioEndpoints();
            return app;
        }
    }
}
