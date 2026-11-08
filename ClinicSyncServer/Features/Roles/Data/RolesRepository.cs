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

        return await db.QuerySingleOrDefaultAsync<RolDetalle>(new CommandDefinition(
            "api.usp_Roles_ObtenerPorId",
            new { IdRol = id },
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct));
    }

    public async Task ModificarAsync(int id, ModificarRolRequest request, CancellationToken ct = default)
    {
        await using var db = factory.Crear();
        await db.OpenAsync(ct);

        try
        {
            await db.ExecuteAsync(new CommandDefinition(
                "api.usp_Roles_Modificar",
                new { IdRol = id, request.Nombre, request.Descripcion },
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
}
