using ClinicSyncServer.Features.Egresos.Data;
using ClinicSyncServer.Features.Egresos.Requests;
using ClinicSyncServer.Features.Egresos.Responses;

namespace ClinicSyncServer.Features.Egresos
{
    public class EgresosService(EgresosRepository repositorio) : IEgresosService
    {
        public Task<int> InsertarAsync(
            RegistrarEgresoRequest req,
            int idUsuario,
            CancellationToken ct = default)
            => repositorio.InsertarAsync(req, idUsuario, ct);

        public Task<IEnumerable<EgresoListado>> ListarAsync(
            DateTime desde,
            DateTime hasta,
            bool? activo = null,
            CancellationToken ct = default)
            => repositorio.ListarAsync(desde, hasta, activo, ct);
    }
}
