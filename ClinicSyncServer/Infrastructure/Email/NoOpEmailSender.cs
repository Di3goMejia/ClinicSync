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
        /// <summary>
        /// Enviars the activacion asynchronous.
        /// </summary>
        /// <param name="correo">The correo.</param>
        /// <param name="nombre">The nombre.</param>
        /// <param name="token">The token.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task EnviarActivacionAsync(
            string correo,
            string nombre,
            string token,
            CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Enviars the desactivacion asynchronous.
        /// </summary>
        /// <param name="correo">The correo.</param>
        /// <param name="nombre">The nombre.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task EnviarDesactivacionAsync(
            string correo,
            string nombre,
            CancellationToken ct = default) => Task.CompletedTask;

        /// <summary>
        /// Enviars the reactivacion asynchronous.
        /// </summary>
        /// <param name="correo">The correo.</param>
        /// <param name="nombre">The nombre.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task EnviarReactivacionAsync(string correo, string nombre, CancellationToken ct = default) => Task.CompletedTask;

        /// <summary>
        /// Enviars the restablecimiento asynchronous.
        /// </summary>
        /// <param name="correo">The correo.</param>
        /// <param name="token">The token.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public Task EnviarRestablecimientoAsync(
            string correo,
            string token,
            CancellationToken ct = default)
        {
            return Task.CompletedTask;
        }
    }

}

