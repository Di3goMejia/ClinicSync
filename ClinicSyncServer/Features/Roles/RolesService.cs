using ClinicSyncServer.Features.Roles.Data;
using ClinicSyncServer.Features.Roles.Projections;
using ClinicSyncServer.Features.Roles.Requests;
using ClinicSyncServer.Features.Roles.Responses;
using System.Globalization;
using System.Text;

namespace ClinicSyncServer.Features.Roles;

public sealed partial class RolesService(RolesRepository repo) : IRolesService
{
    public async Task<IEnumerable<RolListado>> ListarAsync(ListarRolesQuery query, CancellationToken ct = default)
    {
        return await repo.ListarAsync(query, ct);
    }

    public async Task<CrearRolResponse> CrearAsync(CrearRolRequest request, CancellationToken ct = default)
    {
        var codigo = GenerarCodigo(request.Nombre);
        return await repo.CrearAsync(request.Nombre.Trim(), request.Descripcion?.Trim(), codigo, ct);
    }

    public async Task<RolDetalle?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        return await repo.ObtenerPorIdAsync(id, ct);
    }

    public async Task ModificarAsync(int id, ModificarRolRequest request, CancellationToken ct = default)
    {
        await repo.ModificarAsync(id, request with
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim()
        }, ct);
    }

    public async Task CambiarEstadoAsync(int id, CambiarEstadoRolRequest request, CancellationToken ct = default)
    {
        await repo.CambiarEstadoAsync(id, request.Activo!.Value, ct);
    }

    private static string GenerarCodigo(string nombre)
    {
        var normalizado = nombre.Trim().Normalize(NormalizationForm.FormD);
        var caracteres = normalizado
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray();

        var codigo = new string(caracteres).Normalize(NormalizationForm.FormC).ToLowerInvariant();
        codigo = string.Join('-', codigo.Split(
            new[] { ' ', '\t', '\r', '\n', '_', '-', '.', ',', '/', '\\' },
            StringSplitOptions.RemoveEmptyEntries));
        return codigo.Trim('-');
    }
}
