namespace ClinicSyncServer.Common.Abstracciones
{
    /// <summary>
    /// IEmailSender define la interfaz de la clase encargada de enviar los correos
    /// transaccionales (activar/desactivar cuenta, primer login, etc.)
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Enviars the activacion asynchronous.
        /// </summary>
        /// <param name="correo">The correo.</param>
        /// <param name="nombre">The nombre.</param>
        /// <param name="token">The token.</param>
        /// <param name="ct">The ct.</param>
        public async Task EnviarActivacionAsync(string correo, string nombre, string token, CancellationToken ct = default) { }
        /// <summary>
        /// Enviars the restablecimiento asynchronous.
        /// </summary>
        /// <param name="correo">The correo.</param>
        /// <param name="token">The token.</param>
        /// <param name="ct">The ct.</param>
        public async Task EnviarRestablecimientoAsync(string correo, string token, CancellationToken ct = default) { }
        /// <summary>
        /// Enviars the desactivacion asynchronous.
        /// </summary>
        /// <param name="correo">The correo.</param>
        /// <param name="nombre">The nombre.</param>
        /// <param name="ct">The ct.</param>
        public async Task EnviarDesactivacionAsync(string correo, string nombre, CancellationToken ct = default) { }
        /// <summary>
        /// Enviars the reactivacion asynchronous.
        /// </summary>
        /// <param name="correo">The correo.</param>
        /// <param name="nombre">The nombre.</param>
        /// <param name="ct">The ct.</param>
        public async Task EnviarReactivacionAsync(string correo, string nombre, CancellationToken ct = default) { }
    }
}
