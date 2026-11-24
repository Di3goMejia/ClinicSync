using ClinicSyncServer.Features.Tratamientos.Requests;
using ClinicSyncServer.Features.Tratamientos.Responses;
using Microsoft.AspNetCore.Mvc;
using PermisosSeguridad = ClinicSyncServer.Common.Seguridad.Permisos;

namespace ClinicSyncServer.Features.Tratamientos
{
    /// <summary>
    /// 
    /// </summary>
    public static class TratamientosEndpoints
    {
        public static void MapTratamientosEndpoints(this WebApplication app)
        {
            var tratamientos = app.MapGroup("/api/tratamientos").RequireAuthorization();

            // ENDPOINT CREAR
            tratamientos.MapPost("/", async ([FromBody] RegistrarTratamientoRequest req, ITratamientosService svc) =>
            {
                var id = await svc.InsertarAsync(req);
                return Results.Created($"/api/tratamientos/{id}", new RegistrarTratamientoResponse(id));
            })
            .WithName("CrearTratamiento")
            .RequireAuthorization(PermisosSeguridad.TratamientosCrear);

            // ENDPOINT LISTAR
            tratamientos.MapGet("/", async ([AsParameters] ListarTratamientoQuery query, ITratamientosService svc) =>
            {
                return await svc.ListarAsync(query);
            })
            .WithName("ListarTratamientos")
            .RequireAuthorization(PermisosSeguridad.TratamientosLeer);

            // ENDPOINT OBTENER
            tratamientos.MapGet("/{id:int}", async (int id, ITratamientosService svc) =>
            {
                var tratamiento = await svc.ObtenerPorIdAsync(id);

                return tratamiento is null
                    ? Results.NotFound()
                    : Results.Ok(tratamiento);
            })
            .WithName("ObtenerTratamiento")
            .RequireAuthorization(PermisosSeguridad.TratamientosLeer);

            // ENDPOINT MODIFICAR
            tratamientos.MapPut("/{id:int}", async (int id, [FromBody] RegistrarTratamientoRequest req, ITratamientosService svc) =>
            {
                var tratamiento = await svc.ModificarAsync(id, req);

                return tratamiento is null
                    ? Results.NotFound()
                    : Results.Ok(tratamiento);
            })
            .WithName("ModificarTratamiento")
            .RequireAuthorization(PermisosSeguridad.TratamientosModificar);

            // ENDPOINT ESTADO
            tratamientos.MapPatch("/{id:int}/estado", async (int id, [FromBody] CambiarEstadoTratamientoRequest req, ITratamientosService svc) =>
            {
                var tratamiento = await svc.ObtenerPorIdAsync(id);
                if (tratamiento is null)
                    return Results.NotFound();

                await svc.CambiarEstadoAsync(id, req.Activo);
                return Results.NoContent();
            })
            .WithName("CambiarEstadoTratamiento")
            .RequireAuthorization(PermisosSeguridad.TratamientosEstado);
        }
    }
}