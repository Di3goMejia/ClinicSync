
using ClinicSyncServer.Features.Auth.Requests;
using ClinicSyncServer.Features.Auth.Responses;
using ClinicSyncServer.Features.Usuarios;
using ClinicSyncServer.Features.Permisos;
using ClinicSyncServer.Infrastructure.Auth;
using System.Security.Claims;

namespace ClinicSyncServer.Features.Auth
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            var publico = app.MapGroup("/api/auth").AllowAnonymous();

            publico.MapPost("/login", async (
                LoginRequest req,
                HttpContext ctx,
                IAuthService auth,
                CancellationToken ct) =>
            {
                return Results.Ok(
                    await auth.LoginAsync(req, DatosDe(ctx), ct));
            });

            publico.MapPost("/2fa", async (
                VerificarSegundoFactorRequest req,
                HttpContext ctx,
                IAuthService auth,
                CancellationToken ct) =>
            {
                return Results.Ok(
                    await auth.VerificarSegundoFactorAsync(
                        req, DatosDe(ctx), ct));
            });

            publico.MapPost("/refresh", async (
                RefreshRequest req,
                IAuthService auth,
                CancellationToken ct) =>
            {
                return Results.Ok(
                    await auth.RenovarAsync(req.RefreshToken, ct));
            });

            publico.MapPost("/activar", async (
                ActivarCuentaRequest req,
                IAuthService auth) =>
            {
                await auth.ActivarCuentaAsync(
                    req.Token, req.NuevaPassword);

                return Results.NoContent();
            });

            publico.MapPost("/olvide-contrasena", async (
                OlvideContrasenaRequest req,
                IAuthService auth,
                CancellationToken ct) =>
            {
                await auth.SolicitarRestablecimientoAsync(
                    req.Identificador, ct);

                return Results.Accepted();
            });

            publico.MapPost("/restablecer", async (
            RestablecerContrasenaRequest req,
            IAuthService auth) =>
            {
                await auth.RestablecerContrasenaAsync(
                    req.Identificador,
                    req.Codigo,
                    req.NuevaPassword,
                    req.ConfirmarPassword);

                return Results.NoContent();
            });

            // Perfil del usuario autenticado.
            app.MapGet("/api/auth/me", async (
                ClaimsPrincipal usuario,
                IUsuariosService usuarios,
                IPermisosService permisos,
                CancellationToken ct) =>
            {
                var idTexto = usuario.FindFirstValue("sub");

                if (!int.TryParse(idTexto, out var idUsuario))
                    return Results.Unauthorized();

                var perfilPermisos =
                    await permisos.ObtenerPorUsuarioAsync(idUsuario, ct);

                if (perfilPermisos is null)
                    return Results.Unauthorized();

                var detalle = await usuarios.ObtenerAsync(
                    idUsuario,
                    idUsuario,
                    false,
                    ct);

                if (detalle is null)
                    return Results.NotFound();

                var tieneFinanzas =
                    perfilPermisos.Rol.Equals(
                    "ADMINISTRADOR",
                    StringComparison.OrdinalIgnoreCase);

                var respuesta = new PerfilActualResponse(
                    detalle.NombreUsuario,
                    perfilPermisos.Rol,
                    tieneFinanzas,
                    detalle.IdEmpleado,
                    $"{detalle.Nombres} {detalle.Apellidos}".Trim()
                );

                return Results.Ok(respuesta);
            })
                .RequireAuthorization();

            // Cierre de sesión.
            app.MapPost("/api/auth/logout", async (
                ClaimsPrincipal usuario,
                IAuthService auth) =>
            {
                var sidTexto = usuario.FindFirstValue("sid");

                if (!Guid.TryParse(sidTexto, out var sid))
                    return Results.Unauthorized();

                await auth.CerrarSesionAsync(sid);

                return Results.NoContent();
            })
                .RequireAuthorization();
        }

        // El cliente WPF envía su nombre de equipo en un encabezado propio.
        private static DatosCliente DatosDe(HttpContext ctx) => new(
            ctx.Connection.RemoteIpAddress?.ToString(),
            ctx.Request.Headers["X-Equipo"].FirstOrDefault()
                ?? "Desconocido");
    }
}
