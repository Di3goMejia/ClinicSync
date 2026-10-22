using ClinicSyncServer.Common.Abstracciones;
using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Common.Opciones;
using ClinicSyncServer.Infrastructure.Auth;
using ClinicSyncServer.Infrastructure.Email;

namespace ClinicSyncServer.Infrastructure
{
    public static class Injector
    {
        public static IServiceCollection AddInfraestructura(
            this IServiceCollection services, IConfiguration config
        )
        {
            services.Configure<OpcionesConexion>(config.GetSection("ConnectionStrings"));
            services.AddSingleton<SqlConnectionFactory>();
            services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
            services.AddSingleton<IEmailSender, NoOpEmailSender>();

            return services;
        }
    }
}
