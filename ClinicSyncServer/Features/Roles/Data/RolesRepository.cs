using ClinicSyncServer.Common.Data;
using ClinicSyncServer.Features.Roles.Projections;
using ClinicSyncServer.Features.Roles.Requests;
using ClinicSyncServer.Features.Roles.Responses;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ClinicSyncServer.Features.Roles.Data;

public sealed class RolesRepository(SqlConnectionFactory factory)
{
    public async Task<IEnumerable<RolListado>> ListarAsync(ListarRolesQuery query, CancellationToken ct = default)
    {
        await using var db = factory.Crear();
        await db.OpenAsync(ct);

        return await db.QueryAsync<RolListado>(new CommandDefinition(
            "api.usp_Roles_Listar",
            new { query.Nombre, query.Activo },
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct));
    }

    public async Task<CrearRolResponse> CrearAsync(string nombre, string? descripcion, string codigo, CancellationToken ct = default)
    {
        await using var db = factory.Crear();
        await db.OpenAsync(ct);

        try
        {
            return await db.QuerySingleAsync<CrearRolResponse>(new CommandDefinition(
                "api.usp_Roles_Crear",
                new { Nombre = nombre, Codigo = codigo, Descripcion = descripcion },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct));
        }
        catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
        {
            throw SqlErrorTranslator.Traducir(ex);
        }
    }

    public async Task<RolDetalle?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        await using var db = factory.Crear();
        await db.OpenAsync(ct);

        var rol = await db.QuerySingleOrDefaultAsync<RolDetalle>(new CommandDefinition(
            "api.usp_Roles_ObtenerPorId",
            new { Id = id },
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct));

        rol?.Permisos = await ObtenerPermisosAsync(id, ct);
        return rol;
    }

    public async Task ModificarAsync(int id, ModificarRolRequest request, CancellationToken ct = default)
    {
        await using var db = factory.Crear();
        await db.OpenAsync(ct);

        try
        {
            await db.ExecuteAsync(new CommandDefinition(
                "api.usp_Roles_Modificar",
                new { IdRol = id, request.Nombre, request.Descripcion, request.Codigo },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct));
        }
        catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
        {
            throw SqlErrorTranslator.Traducir(ex);
        }
    }

    public async Task CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default)
    {
        await using var db = factory.Crear();
        await db.OpenAsync(ct);

        try
        {
            await db.ExecuteAsync(new CommandDefinition(
                "api.usp_Roles_CambiarEstado",
                new { IdRol = id, Activo = activo },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct));
        }
        catch (SqlException ex) when (SqlErrorTranslator.EsTraducible(ex))
        {
            throw SqlErrorTranslator.Traducir(ex);
        }
    }

    public async Task<int> ContarUsuariosAsync(int idRol, CancellationToken ct = default)
    {
        await using var db = factory.Crear();
        await db.OpenAsync(ct);

        var usuarios = await db.QueryAsync<int>(
            new CommandDefinition(
                "api.usp_Roles_ListarUsuarios",
                new { Id = idRol },
                commandType: CommandType.StoredProcedure,
                cancellationToken: ct
            )
        );

        return usuarios.Count();
    }

    public async Task<bool> EsProtegidoAsync(int idRol, CancellationToken ct = default)
    {
        var rol = await ObtenerPorIdAsync(idRol, ct);
        return rol!.Protegido;
    }

    public async Task<IReadOnlyList<string>> ObtenerPermisosAsync(int idRol, CancellationToken ct = default)
    {
        await using var db = factory.Crear();
        await db.OpenAsync(ct);
        var permisos = await db.QueryAsync<string>(new CommandDefinition(
            "api.usp_Roles_PermisosPorRol",
            new { IdRol = idRol },
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct));
        return permisos.ToList();
    }

    public async Task ReemplazarPermisosAsync(int idRol, IEnumerable<string> codigos, CancellationToken ct = default)
    {
        await using var db = factory.Crear();
        await db.OpenAsync(ct);

        var tabla = new DataTable();
        tabla.Columns.Add("Codigo", typeof(string));
        foreach (var codigo in codigos.Distinct(StringComparer.OrdinalIgnoreCase))
            tabla.Rows.Add(codigo);

        var parametros = new DynamicParameters();
        parametros.Add("IdRol", idRol);
        parametros.Add("CodigosPermisos", tabla.AsTableValuedParameter("dbo.CodigosPermiso"));

        await db.ExecuteAsync(new CommandDefinition(
            "api.usp_Roles_ReemplazarPermisos",
            parametros,
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct));
    }
}
