using ClinicSyncServer.Features.Permisos.Data;

namespace ClinicSyncServer.Features.Permisos
{
    /// <summary>
    /// 
    /// </summary>
    public static class PermisosModule
    {
        /// <summary>
        /// Adds the permisos.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddPermisos(this IServiceCollection services)
        {
            services.AddScoped<PermisosRepository>();
            services.AddScoped<IPermisosService, PermisosService>();
            return services;
        }

        /// <summary>
        /// Maps the permisos.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <returns></returns>
        public static WebApplication MapPermisos(this WebApplication app)
        {
            app.MapPermisosEndpoints();
            return app;
        }
    }
}
