using ClinicSyncServer.Features.Expedientes.Data;
using ClinicSyncServer.Features.Expedientes.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

using PermisosSeguridad = ClinicSyncServer.Common.Seguridad.Permisos;

namespace ClinicSyncServer.Features.Expedientes
{
    /// <summary>
    /// 
    /// </summary>
    public static class ExpedientesEndpoints
    {
        public static void MapExpedientesEndpoints(
            this WebApplication app)
        {
            var grupo = app.MapGroup("/api/pacientes")
                .RequireAuthorization();

            // OBTENER EXPEDIENTE DEL PACIENTE
            grupo.MapGet(
                "/{idPaciente:int}/expediente",
                async (
                    int idPaciente,
                    IExpedientesService svc,
                    CancellationToken ct) =>
                {
                    var expediente = await svc.ObtenerPorIdAsync(null, idPaciente, ct);
                    return expediente is null
                        ? Results.NotFound()
                        : Results.Ok(expediente);
                }
            ).RequireAuthorization(PermisosSeguridad.ExpedientesLeer);

            // MODIFICAR EXPEDIENTE DEL PACIENTE
            grupo.MapPatch(
                "/{idPaciente:int}/expediente",
                async (
                    int idPaciente,
                    [FromBody] ModificarExpedienteRequest req,
                    IExpedientesService svc,
                    CancellationToken ct) =>
                {
                    await svc.ModificarAsync(idPaciente, req.Notas, ct);
                    return Results.NoContent();
                }
            ).RequireAuthorization(PermisosSeguridad.ExpedientesModificar);

            // LISTAR VISITAS DEL PACIENTE
            grupo.MapGet(
                "/{idPaciente:int}/expediente/visitas",
                async (
                    int idPaciente,
                    IVisitasService svc,
                    CancellationToken ct) =>
                {
                    var visitas = await svc.ListarAsync(idPaciente, ct);
                    return Results.Ok(visitas);
                }
            ).RequireAuthorization(PermisosSeguridad.ExpedientesLeer);
        }
    }
}
