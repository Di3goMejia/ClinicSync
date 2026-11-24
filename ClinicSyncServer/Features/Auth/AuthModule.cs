
using ClinicSyncServer.Features.Auth.Data;
using ClinicSyncServer.Features.Auth.Factores;
using ClinicSyncServer.Features.Auth.Services;
using ClinicSyncServer.Features.Usuarios;
using ClinicSyncServer.Features.Usuarios.Data;
using ClinicSyncServer.Infrastructure.Facial;

namespace ClinicSyncServer.Features.Auth
{
    /// <summary>
    /// 
    /// </summary>
    public static class AuthModule
    {
        /// <summary>
        /// Adds the authentication.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Maps the authentication.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <returns></returns>
        public static WebApplication MapAuth(this WebApplication app)
        {
            app.MapAuthEndpoints();
            return app;
        }
    }
}
