using ClinicSyncServer.Validators;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSyncServer.Features.Tratamientos
{
    public static class TratamientosEndpoints
    {
        public static void MapTratamientosEndpoints(
            this WebApplication app)
        {
            var auth = app.MapGroup("/api/tratamientos")
                          .RequireAuthorization();

            auth.MapGet("/", async (
                ITratamientosService service,
                CancellationToken ct) =>
            {
                var tratamientos = await service.ListarAsync(ct);

                return Results.Ok(tratamientos);
            })
            .WithName("ListarTratamientos");

            auth.MapPost("/", async (
                [FromBody] RegistrarTratamientoRequest request,
                ITratamientosService service,
                CancellationToken ct) =>
            {
                var error = TratamientoValidator.Validar(
                    request.Nombre,
                    request.Precio,
                    request.Descripcion);

                if (error is not null)
                {
                    return Results.BadRequest(new
                    {
                        Mensaje = error
                    });
                }

                try
                {
                    var idTratamiento = await service.RegistrarAsync(
                        request.Nombre.Trim(),
                        request.Precio,
                        string.IsNullOrWhiteSpace(request.Descripcion)
                            ? null
                            : request.Descripcion.Trim(),
                        ct);

                    return Results.Created(
                        $"/api/tratamientos/{idTratamiento}",
                        new { IdTratamiento = idTratamiento });
                }
                catch (Microsoft.Data.SqlClient.SqlException ex)
                    when (ex.Number == 2627 || ex.Number == 2601)
                {
                    return Results.Conflict(new
                    {
                        Mensaje = "Ya existe un tratamiento con ese nombre."
                    });
                }
            })
            .WithName("RegistrarTratamiento");
        }
    }

    public class RegistrarTratamientoRequest
    {
        public string Nombre { get; set; } = string.Empty;

        public decimal Precio { get; set; }

        public string? Descripcion { get; set; }
    }
}
