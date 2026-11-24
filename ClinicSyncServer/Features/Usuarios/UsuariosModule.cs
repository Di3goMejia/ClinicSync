using ClinicSyncServer.Features.Usuarios.Data;

namespace ClinicSyncServer.Features.Usuarios
{
    /// <summary>
    /// 
    /// </summary>
    public static class UsuariosModule
    {
        /// <summary>
        /// Adds the usuarios.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddUsuarios(this IServiceCollection services)
        {
            services.AddScoped<UsuariosRepository>();
            services.AddScoped<IUsuariosService, UsuariosService>();
            return services;
        }

        /// <summary>
        /// Maps the usuarios.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <returns></returns>
        public static WebApplication MapUsuarios(this WebApplication app)
        {
            app.MapUsuariosEndpoints();
            return app;
        }
    }
}
