using ClinicSyncServer.Features.Sesiones.Projections;

namespace ClinicSyncServer.Features.Sesiones
{
    public interface ISesionService
    {
        Task<Guid> CrearAsync(int idUsuario, byte[] hashRefresh, string equipo,
            string? ip, DateTime expira, CancellationToken ct = default);

        Task<SesionRotada?> RotarAsync(byte[] hashActual, byte[] hashNuevo,
            CancellationToken ct = default);

        Task CerrarAsync(Guid idSesion, string motivo, CancellationToken ct = default);

        Task<bool> EsActivaAsync(Guid idSesion, int idUsuario, CancellationToken ct = default);

    }
}
