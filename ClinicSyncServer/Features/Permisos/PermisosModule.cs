using ClinicSyncServer.Features.Permisos.Data;

namespace ClinicSyncServer.Features.Permisos
{
    public static class PermisosModule
    {
        public static IServiceCollection AddPermisos(this IServiceCollection services)
        {
            services.AddScoped<PermisosRepository>();
            services.AddScoped<IPermisosService, PermisosService>();
            return services;
        }

        public static WebApplication MapPermisos(this WebApplication app)
        {
            app.MapPermisosEndpoints();
            return app;
        }
    }
}
