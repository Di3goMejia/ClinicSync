using ClinicSyncServer.Features.Sesiones.Data;
using ClinicSyncServer.Features.Sesiones.Projections;

namespace ClinicSyncServer.Features.Sesiones
{
    public class SesionService(SesionesRepository repo) : ISesionService
    {
        public Task<Guid> CrearAsync(int idUsuario, byte[] hashRefresh, string equipo,
            string? ip, DateTime expira, CancellationToken ct = default) =>
            repo.CrearAsync(idUsuario, hashRefresh, equipo, ip, expira);

        public Task<SesionRotada?> RotarAsync(byte[] hashActual, byte[] hashNuevo,
            CancellationToken ct = default) =>
            repo.RotarAsync(hashActual, hashNuevo);

        public Task CerrarAsync(Guid idSesion, string motivo, CancellationToken ct = default) =>
            repo.CerrarAsync(idSesion, motivo);

        public Task<bool> EsActivaAsync(Guid idSesion, int idUsuario, CancellationToken ct = default) =>
            repo.EsActivaAsync(idSesion, idUsuario);
    }
}
