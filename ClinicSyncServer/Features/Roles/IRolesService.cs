using ClinicSyncServer.Features.Roles.Projections;
using ClinicSyncServer.Features.Roles.Requests;
using ClinicSyncServer.Features.Roles.Responses;

namespace ClinicSyncServer.Features.Roles;

public interface IRolesService
{
    Task<IEnumerable<RolListado>> ListarAsync(ListarRolesQuery query, CancellationToken ct = default);
    Task<CrearRolResponse> CrearAsync(CrearRolRequest request, CancellationToken ct = default);
    Task<RolDetalle?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task ModificarAsync(int id, ModificarRolRequest request, CancellationToken ct = default);
    Task CambiarEstadoAsync(int id, CambiarEstadoRolRequest request, CancellationToken ct = default);
}
