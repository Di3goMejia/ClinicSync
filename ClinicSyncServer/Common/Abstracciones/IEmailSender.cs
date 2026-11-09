namespace ClinicSyncServer.Common.Abstracciones
{
    /// <summary>
    /// IEmailSender define la interfaz de la clase encargada de enviar los correos 
    /// transaccionales (activar/desactivar cuenta, primer login, etc.)
    /// </summary>
    public interface IEmailSender
    {
        public async Task EnviarActivacionAsync(string correo, string nombre, string token, CancellationToken ct = default) { }
        public async Task EnviarRestablecimientoAsync(string correo, string token, CancellationToken ct = default) { }
        public async Task EnviarDesactivacionAsync(string correo, string nombre, CancellationToken ct = default) { }
        public async Task EnviarReactivacionAsync(string correo, string nombre, CancellationToken ct = default) { }
    }
}
