namespace ClinicSyncServer.Features.Auth.Factores
{
    /// <summary>
    /// 
    /// </summary>
    public interface IFactorSegundoService
    {
        /// <summary>
        /// Verificars the asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="prueba">The prueba.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        Task<bool> VerificarAsync(int idUsuario, string prueba, CancellationToken ct = default);
    }
}
