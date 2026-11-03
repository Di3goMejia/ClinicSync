using ClinicSyncServer.Features.Egresos.Requests;
using ClinicSyncServer.Features.Egresos.Responses;

namespace ClinicSyncServer.Features.Egresos
{
    public interface IEgresosService
    {
        Task<int> InsertarAsync(
            RegistrarEgresoRequest req,
            int idUsuario,
            CancellationToken ct = default);

        Task<EgresoListado?> ObtenerAsync(
            int idEgreso,
            CancellationToken ct = default);

        Task ActualizarAsync(
            int idEgreso,
            ActualizarEgresoRequest req,
            int idUsuario,
            CancellationToken ct = default);

        Task<IEnumerable<EgresoListado>> ListarAsync(
            DateTime desde,
            DateTime hasta,
            bool? activo = null,
            CancellationToken ct = default);
    }
}
