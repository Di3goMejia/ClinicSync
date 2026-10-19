namespace ClinicSyncServer.Features.Auth.Factores
{
    public interface IFactorSegundoService
    {
        Task<bool> VerificarAsync(int idUsuario, string prueba, CancellationToken ct = default);
    }
}
