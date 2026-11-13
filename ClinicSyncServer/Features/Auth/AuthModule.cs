
using ClinicSyncServer.Features.Auth.Data;
using ClinicSyncServer.Features.Auth.Factores;
using ClinicSyncServer.Features.Auth.Services;
using ClinicSyncServer.Features.Usuarios;
using ClinicSyncServer.Features.Usuarios.Data;
using ClinicSyncServer.Infrastructure.Facial;

namespace ClinicSyncServer.Features.Auth
{
    public static class AuthModule
    {
        public static IServiceCollection AddAuth(
            this IServiceCollection services)
        {
            services.AddScoped<CredencialesRepository>();
            services.AddScoped<TokensUsuarioRepository>();
            services.AddScoped<Metodos2FARepository>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IFactorSegundoService, FactorSegundoFacial>();

            services.AddScoped<ServicioReconocimientoFacial>();
            services.AddScoped<RegistroCredencialFacialService>();

            return services;
        }

        public static WebApplication MapAuth(this WebApplication app)
        {
            app.MapAuthEndpoints();
            return app;
        }
    }
}
