namespace ClinicSyncServer.Features.Auth.Factores
{
    public class FactorSegundoPendiente : IFactorSegundoService
    {
        public Task<bool> VerificarAsync(int idUsuario, string prueba, CancellationToken ct = default)
        {
            // POR HACER: Aquí iria el proceso 2FA biométrico.
            return Task.FromResult(false);
        }
    }
}
