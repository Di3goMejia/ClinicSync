using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Models;

namespace ClinicSyncDesktop.Services;

/// <summary>
/// 
/// </summary>
public sealed class PermisosApiService
{
    /// <summary>
    /// Listars the asynchronous.
    /// </summary>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    public async Task<List<PermisoCatalogo>> ListarAsync(CancellationToken ct = default)
    {
        return await ClienteApi.ObtenerAutenticadoAsync<List<PermisoCatalogo>>("api/permisos");
    }
}
