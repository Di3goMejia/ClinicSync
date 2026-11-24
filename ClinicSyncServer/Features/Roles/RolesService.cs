using ClinicSyncServer.Features.Roles.Data;
using ClinicSyncServer.Features.Roles.Projections;
using ClinicSyncServer.Features.Roles.Requests;
using ClinicSyncServer.Features.Roles.Responses;
using System.Globalization;
using System.Text;

using PermisosCatalogo = ClinicSyncServer.Common.Seguridad.Permisos;

namespace ClinicSyncServer.Features.Roles;

/// <summary>
/// 
/// </summary>
/// <seealso cref="ClinicSyncServer.Features.Roles.IRolesService" />
public sealed partial class RolesService(RolesRepository repo) : IRolesService
{
    /// <summary>
    /// Listars the asynchronous.
    /// </summary>
    /// <param name="query">The query.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    public async Task<IEnumerable<RolListado>> ListarAsync(ListarRolesQuery query, CancellationToken ct = default)
    {
        return await repo.ListarAsync(query, ct);
    }

    /// <summary>
    /// Crears the asynchronous.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    public async Task<CrearRolResponse> CrearAsync(CrearRolRequest request, CancellationToken ct = default)
    {
        var codigo = GenerarCodigo(request.Nombre);
        var respuesta = await repo.CrearAsync(request.Nombre.Trim(), request.Descripcion?.Trim(), codigo, ct);
        var permisos = request.Permisos ?? [];
        await ValidarPermisosAsync(permisos);
        await repo.ReemplazarPermisosAsync(respuesta.IdRol, permisos, ct);
        return respuesta;
    }

    /// <summary>
    /// Obteners the por identifier asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ct">The ct.</param>
    /// <returns></returns>
    public async Task<RolDetalle?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        return await repo.ObtenerPorIdAsync(id, ct);
    }

    /// <summary>
    /// Modificars the asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="request">The request.</param>
    /// <param name="ct">The ct.</param>
    public async Task ModificarAsync(int id, ModificarRolRequest request, CancellationToken ct = default)
    {
        await ValidarNoProtegidoAsync(id, ct);
        await repo.ModificarAsync(id, request with
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            Codigo = GenerarCodigo(request.Nombre.Trim()),
        }, ct);
        if (request.Permisos is not null)
        {
            await ValidarPermisosAsync(request.Permisos);
            await repo.ReemplazarPermisosAsync(id, request.Permisos, ct);
        }
    }

    /// <summary>
    /// Validars the permisos asynchronous.
    /// </summary>
    /// <param name="codigos">The codigos.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Los permisos no son válidos: {string.Join(", ", desconocidos)}.</exception>
    private static Task ValidarPermisosAsync(IEnumerable<string> codigos)
    {
        var permitidos = PermisosCatalogo.Todos.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var desconocidos = codigos.Where(c => !permitidos.Contains(c)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        
        if (desconocidos.Count > 0)
            throw new InvalidOperationException(
                $"Los permisos no son válidos: {string.Join(", ", desconocidos)}.");

        return Task.CompletedTask;
    }

    /// <summary>
    /// Cambiars the estado asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="request">The request.</param>
    /// <param name="ct">The ct.</param>
    /// <exception cref="InvalidOperationException">No se puede cambiar el estado de un rol que tiene usuarios asignados.</exception>
    public async Task CambiarEstadoAsync(int id, CambiarEstadoRolRequest request, CancellationToken ct = default)
    {
        await ValidarNoProtegidoAsync(id, ct);
        if (await repo.ContarUsuariosAsync(id, ct) > 0)
            throw new InvalidOperationException("No se puede cambiar el estado de un rol que tiene usuarios asignados.");
        await repo.CambiarEstadoAsync(id, request.Activo!.Value, ct);
    }

    /// <summary>
    /// Validars the no protegido asynchronous.
    /// </summary>
    /// <param name="id">The identifier.</param>
    /// <param name="ct">The ct.</param>
    /// <exception cref="InvalidOperationException">El rol está protegido y no puede modificarse.</exception>
    private async Task ValidarNoProtegidoAsync(int id, CancellationToken ct)
    {
        if (await repo.EsProtegidoAsync(id, ct))
            throw new InvalidOperationException("El rol está protegido y no puede modificarse.");
    }

    /// <summary>
    /// Generars the codigo.
    /// </summary>
    /// <param name="nombre">The nombre.</param>
    /// <returns></returns>
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
