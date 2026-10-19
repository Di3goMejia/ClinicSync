using ClinicSyncServer.Features.Usuarios.Data;

namespace ClinicSyncServer.Features.Usuarios
{
    public static class UsuariosModule
    {
        public static IServiceCollection AddUsuarios(this IServiceCollection services)
        {
            services.AddScoped<UsuariosRepository>();
            services.AddScoped<IUsuariosService, UsuariosService>();
            return services;
        }

        public static WebApplication MapUsuarios(this WebApplication app)
        {
            app.MapUsuariosEndpoints();
            return app;
        }
    }
}
