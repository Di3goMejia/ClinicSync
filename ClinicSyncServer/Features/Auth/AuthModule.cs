using ClinicSyncServer.Features.Auth.Data;
using ClinicSyncServer.Features.Auth.Factores;
using ClinicSyncServer.Features.Usuarios;
using ClinicSyncServer.Features.Usuarios.Data;

namespace ClinicSyncServer.Features.Auth
{
    public static class AuthModule
    {
        public static IServiceCollection AddAuth(this IServiceCollection services)
        {
            services.AddScoped<CredencialesRepository>();
            services.AddScoped<TokensUsuarioRepository>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddSingleton<IFactorSegundoService, FactorSegundoPendiente>();
            return services;
        }

        public static WebApplication MapAuth(this WebApplication app)
        {
            app.MapAuthEndpoints();
            return app;
        }
    }
}
