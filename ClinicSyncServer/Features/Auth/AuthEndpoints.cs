using ClinicSyncServer.Features.Auth.Requests;
using ClinicSyncServer.Infrastructure.Auth;
using System.Security.Claims;

namespace ClinicSyncServer.Features.Auth
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            var publico = app.MapGroup("/api/auth").AllowAnonymous();

            publico.MapPost("/login", async (LoginRequest req, HttpContext ctx, IAuthService auth, CancellationToken ct) =>
                Results.Ok(await auth.LoginAsync(req, DatosDe(ctx), ct)));

            publico.MapPost("/2fa", async (VerificarSegundoFactorRequest req, HttpContext ctx, IAuthService auth, CancellationToken ct) =>
                Results.Ok(await auth.VerificarSegundoFactorAsync(req, DatosDe(ctx), ct)));

            publico.MapPost("/refresh", async (RefreshRequest req, IAuthService auth, CancellationToken ct) =>
                Results.Ok(await auth.RenovarAsync(req.RefreshToken, ct)));

            publico.MapPost("/activar", async (ActivarCuentaRequest req, IAuthService auth) =>
            {
                await auth.ActivarCuentaAsync(req.Token, req.NuevaPassword);
                return Results.NoContent();
            });

            publico.MapPost("/olvide-contrasena", async (OlvideContrasenaRequest req, IAuthService auth, CancellationToken ct) =>
            {
                await auth.SolicitarRestablecimientoAsync(req.Identificador, ct);
                return Results.Accepted();
            });

            publico.MapPost("/restablecer", async (RestablecerContrasenaRequest req, IAuthService auth) =>
            {
                await auth.RestablecerContrasenaAsync(req.Token, req.NuevaPassword);
                return Results.NoContent();
            });

            app.MapPost("/api/auth/logout", async (ClaimsPrincipal usuario, IAuthService auth) =>
            {
                var sid = Guid.Parse(usuario.FindFirstValue("sid")!);
                await auth.CerrarSesionAsync(sid);
                return Results.NoContent();
            }).RequireAuthorization();
        }

        // El cliente WPF envía su nombre de equipo en un encabezado propio.
        private static DatosCliente DatosDe(HttpContext ctx) => new(
            ctx.Connection.RemoteIpAddress?.ToString(),
            ctx.Request.Headers["X-Equipo"].FirstOrDefault() ?? "Desconocido");
    }
}
    