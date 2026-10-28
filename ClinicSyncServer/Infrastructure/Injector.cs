using Amazon.S3;
using ClinicSyncServer.Common.Abstracciones;
using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Common.Opciones;
using ClinicSyncServer.Infrastructure.Auth;
using ClinicSyncServer.Infrastructure.Email;
using ClinicSyncServer.Infrastructure.Storage;
using Microsoft.AspNetCore.Http.Connections;

namespace ClinicSyncServer.Infrastructure
{
    public static class Injector
    {
        public static IServiceCollection AddInfraestructura(
            this IServiceCollection services, IConfiguration config
        )
        {
            services.Configure<OpcionesConexion>(config.GetSection("ConnectionStrings"));
            services.Configure<OpcionesR2>(config.GetSection("R2"));

            services.AddSingleton<SqlConnectionFactory>();
            services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();
            services.AddTransient<IEmailSender, GmailEmailSender>();

            services.AddSingleton<IAmazonS3>(sp =>
            {
                var r2 = config.GetSection("R2").Get<OpcionesR2>()
                         ?? throw new InvalidOperationException("Falta la sección R2.");

                string accessKeyId = r2.AccessKeyId;
                string secretAccessKey = r2.SecretAccessKey;

                if (string.IsNullOrWhiteSpace(accessKeyId) ||
                    string.IsNullOrWhiteSpace(secretAccessKey))
                {
                    throw new InvalidOperationException(
                        "Faltan las credenciales de R2: AccessKeyId/SecretAccessKey.");
                }

                return new AmazonS3Client(accessKeyId, secretAccessKey, new AmazonS3Config
                {
                    ServiceURL = r2.Endpoint,
                    ForcePathStyle = true,
                    AuthenticationRegion = "auto"
                });
            });

            services.AddSingleton<IStorageService, R2StorageService>();

            return services;
        }
    }
}
