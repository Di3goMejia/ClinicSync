using ClinicSyncServer.Common.Abstracciones;

namespace ClinicSyncServer.Infrastructure.Email
{
    /// <summary>
    /// NoOpEmailSender no hace nada. Simplemente está porque se necesita en el módulo
    /// de Usuarios; a la espera de una implementación de un proveedor de correo concreto.
    /// </summary>
    /// <seealso cref="ClinicSyncServer.Common.Abstracciones.IEmailSender" />
    public class NoOpEmailSender : IEmailSender
    {
        public Task EnviarActivacionAsync(
            string correo,
            string nombre,
            string token,
            CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        public Task EnviarRestablecimientoAsync(
            string correo,
            string token,
            CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }

}

