using ClinicSyncServer.Features.Roles.Data;
using ClinicSyncServer.Features.Roles.Projections;
using ClinicSyncServer.Features.Roles.Requests;
using ClinicSyncServer.Features.Roles.Responses;
using System.Globalization;
using System.Text;
using PermisosCatalogo = ClinicSyncServer.Common.Seguridad.Permisos;

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
        var respuesta = await repo.CrearAsync(request.Nombre.Trim(), request.Descripcion?.Trim(), codigo, ct);
        var permisos = request.Permisos ?? [];
        await ValidarPermisosAsync(permisos);
        await repo.ReemplazarPermisosAsync(respuesta.IdRol, permisos, ct);
        return respuesta;
    }

    public async Task<RolDetalle?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        return await repo.ObtenerPorIdAsync(id, ct);
    }

    public async Task ModificarAsync(int id, ModificarRolRequest request, CancellationToken ct = default)
    {
        await ValidarNoProtegidoAsync(id, ct);
        await repo.ModificarAsync(id, request with
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim()
        }, ct);
        if (request.Permisos is not null)
        {
            await ValidarPermisosAsync(request.Permisos);
            await repo.ReemplazarPermisosAsync(id, request.Permisos, ct);
        }
    }

    private static Task ValidarPermisosAsync(IEnumerable<string> codigos)
    {
        var permitidos = PermisosCatalogo.Todos.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var desconocidos = codigos.Where(c => !permitidos.Contains(c)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (desconocidos.Count > 0)
            throw new InvalidOperationException($"Los permisos no son válidos: {string.Join(", ", desconocidos)}.");
        return Task.CompletedTask;
    }

    public async Task CambiarEstadoAsync(int id, CambiarEstadoRolRequest request, CancellationToken ct = default)
    {
        await ValidarNoProtegidoAsync(id, ct);
        if (await repo.ContarUsuariosAsync(id, ct) > 0)
            throw new InvalidOperationException("No se puede cambiar el estado de un rol que tiene usuarios asignados.");
        await repo.CambiarEstadoAsync(id, request.Activo!.Value, ct);
    }

    private async Task ValidarNoProtegidoAsync(int id, CancellationToken ct)
    {
        if (await repo.EsProtegidoAsync(id, ct))
            throw new InvalidOperationException("El rol está protegido y no puede modificarse.");
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
