using ClinicSyncServer.Features.Roles.Projections;
using ClinicSyncServer.Features.Roles.Requests;
using ClinicSyncServer.Features.Roles.Responses;

namespace ClinicSyncServer.Features.Roles;

/// <summary>
/// 
/// </summary>
public interface IRolesService
{
    /// <summary>
    /// Listars the asynchronous.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    Task<IEnumerable<RolListado>> ListarAsync(ListarRolesQuery query, CancellationToken ct = default);
    /// <summary>
    /// Crears the asynchronous.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    Task<CrearRolResponse> CrearAsync(CrearRolRequest request, CancellationToken ct = default);
    /// <summary>
    /// Obteners the por identifier asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    Task<RolDetalle?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    /// <summary>
    /// Modificars the asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="request">The request.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    Task ModificarAsync(int id, ModificarRolRequest request, CancellationToken ct = default);
    /// <summary>
    /// Cambiars the estado asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="request">The request.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    Task CambiarEstadoAsync(int id, CambiarEstadoRolRequest request, CancellationToken ct = default);
}
