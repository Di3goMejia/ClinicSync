using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Models;

namespace ClinicSyncDesktop.Services;

public sealed class PermisosApiService
{
    public async Task<List<PermisoCatalogo>> ListarAsync(CancellationToken ct = default)
    {
        return await ClienteApi.ObtenerAutenticadoAsync<List<PermisoCatalogo>>("api/permisos");
    }
}
