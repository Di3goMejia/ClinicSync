using Microsoft.AspNetCore.Mvc;

namespace ClinicSyncServer.Common
{
    public class ExceptionMiddleware(RequestDelegate siguiente, ILogger<ExceptionMiddleware> log)
    {
        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                await siguiente(ctx);
            }
            catch (ReglaNegocioException ex)
            {
                await EscribirProblemaAsync(ctx, StatusCodes.Status400BadRequest,
                    titulo: "Regla de negocio incumplida",
                    detalle: ex.Message,
                    extras: new() { ["code"] = ex.Codigo });
            }
            catch (ConflictoException ex)
            {
                await EscribirProblemaAsync(ctx, StatusCodes.Status409Conflict,
                    titulo: "Conflicto con el estado actual",
                    detalle: ex.Message);
            }
            catch (OperationCanceledException) when (ctx.RequestAborted.IsCancellationRequested)
            {
                // El cliente cerró la conexión.
                ctx.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
            catch (BadHttpRequestException)
            {
                await EscribirProblemaAsync(ctx, StatusCodes.Status400BadRequest,
                    titulo: "Solicitud no válida.",
                    detalle: "El cuerpo de la solicitud no es válido.");
            }
            catch (NoAutorizadoException ex)
            {
                await EscribirProblemaAsync(ctx, StatusCodes.Status401Unauthorized,
                    titulo: "No autorizado",
                    detalle: ex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error no controlado en {Metodo} {Ruta}",
                    ctx.Request.Method, ctx.Request.Path);

                await EscribirProblemaAsync(ctx, StatusCodes.Status500InternalServerError,
                    titulo: "Error interno del servidor",
                    detalle: null);
            }
        }

        private static async Task EscribirProblemaAsync(
            HttpContext ctx, int estado, string titulo, string? detalle,
            Dictionary<string, object?>? extras = null)
        {
            // Si la respuesta ya empezó, no se puede cambiar el código ni escribir JSON.
            if (ctx.Response.HasStarted) 
                return;

            ctx.Response.StatusCode = estado;

            var problema = new ProblemDetails
            {
                Status = estado,
                Title = titulo,
                Detail = detalle
            };

            if (extras is not null)
                foreach (var (clave, valor) in extras)
                    problema.Extensions[clave] = valor;

            var servicio = ctx.RequestServices.GetRequiredService<IProblemDetailsService>();

            await servicio.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = ctx,
                ProblemDetails = problema
            });
        }
}
}

/*
using ClinicSyncServer.Features;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSyncServer.Infrastructure
{
    public class ExceptionMiddleware(RequestDelegate siguiente, ILogger<ExceptionMiddleware> log)
    {
        public async Task InvokeAsync(HttpContext ctx)
        {
            try
            {
                await siguiente(ctx);
            }
            catch (ReglaNegocioException ex)
            {
                await EscribirAsync(ctx, StatusCodes.Status400BadRequest,
                    new ProblemDetails
                    {
                        Status = 400,
                        Title = "Regla de negocio no cumplida.",
                        Detail = ex.Message,
                        Extensions = { ["codigo"] = ex.Codigo }
                    }
                );

                //await EscribirAsync(ctx, StatusCodes.Status400BadRequest,
                //new { error = ex.Message, codigo = ex.Codigo });
            }
            catch (ConflictoException ex)
            {
                await EscribirAsync(ctx, StatusCodes.Status409Conflict,
                    new { error = ex.Message });
            }
            catch (OperationCanceledException) when (ctx.RequestAborted.IsCancellationRequested)
            {
                // El cliente cerró la conexión.
                ctx.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error no controlado en {Metodo} {Ruta}",
                    ctx.Request.Method, ctx.Request.Path);

                await EscribirAsync(ctx, StatusCodes.Status500InternalServerError,
                    new { error = "Error interno del servidor" });
            }
        }

        private static Task EscribirAsync(HttpContext ctx, int estado, object cuerpo)
        {
            ctx.Response.StatusCode = estado;
            return ctx.Response.WriteAsJsonAsync(cuerpo);
        }
    }
}
*/