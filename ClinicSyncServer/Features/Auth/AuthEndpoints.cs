
using ClinicSyncServer.Features.Auth.Requests;
using ClinicSyncServer.Features.Auth.Responses;
using ClinicSyncServer.Features.Usuarios;
using ClinicSyncServer.Features.Permisos;
using ClinicSyncServer.Features.Usuarios.Requests;
using ClinicSyncServer.Infrastructure.Auth;
using System.Security.Claims;

namespace ClinicSyncServer.Features.Auth
{
    public static class AuthEndpoints
    {
        public static void MapAuthEndpoints(this WebApplication app)
        {
            // ==========================================
            // ENDPOINTS PÚBLICOS
            // ==========================================

            var publico = app.MapGroup("/api/auth").AllowAnonymous();

            // INICIAR SESIÓN
            publico.MapPost("/login", async (
                LoginRequest req,
                HttpContext ctx,
                IAuthService auth,
                CancellationToken ct) =>
            {
                return Results.Ok(
                    await auth.LoginAsync(req, DatosDe(ctx), ct));
            });

            // VERIFICAR SEGUNDO FACTOR
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

            // RENOVAR TOKEN
            publico.MapPost("/refresh", async (
                RefreshRequest req,
                IAuthService auth,
                CancellationToken ct) =>
            {
                return Results.Ok(
                    await auth.RenovarAsync(req.RefreshToken, ct));
            });

            // VALIDAR TOKEN
            publico.MapPost("/validar-token", async (
                ValidarTokenRequest req,
                IAuthService auth,
                CancellationToken ct) =>
            {
                await auth.ValidarTokenActivacionAsync(
                    req.Token, ct);

                return Results.Ok();
            });

            // VALIDAR ACTIVACIÓN
            publico.MapPost("/validar-activacion", async (
                ValidarTokenRequest req,
                IAuthService auth,
                CancellationToken ct) =>
            {
                await auth.ValidarTokenActivacionAsync(
                    req.Token, ct);

                return Results.Ok();
            });

            // ACTIVAR CUENTA
            publico.MapPost("/activar", async (
                ActivarCuentaRequest req,
                IAuthService auth) =>
            {
                await auth.ActivarCuentaAsync(
                    req.Token,
                    req.NuevaPassword,
                    req.ConfirmarPassword);

                return Results.NoContent();
            });

            // OLVIDÉ MI CONTRASEÑA
            publico.MapPost("/olvide-contrasena", async (
                OlvideContrasenaRequest req,
                IAuthService auth,
                CancellationToken ct) =>
            {
                await auth.SolicitarRestablecimientoAsync(
                    req.Identificador, ct);

                return Results.Accepted();
            });

            // RESTABLECER CONTRASEÑA
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

            // ==========================================
            // PERFIL DEL USUARIO AUTENTICADO
            // ==========================================

            // CONSULTAR DATOS DEL PERFIL
            app.MapGet("/api/auth/me", async (
                ClaimsPrincipal usuario,
                IUsuariosService usuarios,
                IPermisosService permisos,
                CancellationToken ct) =>
            {
                var idTexto =
                    usuario.FindFirstValue("sub");

                if (!int.TryParse(
                    idTexto,
                    out var idUsuario))
                {
                    return Results.Unauthorized();
                }

                // CONSULTAR ROL Y PERMISOS
                var perfilPermisos =
                    await permisos.ObtenerPorUsuarioAsync(
                        idUsuario,
                        ct);

                if (perfilPermisos is null)
                    return Results.Unauthorized();

                // CONSULTAR DATOS DEL USUARIO
                var detalle =
                    await usuarios.ObtenerAsync(
                        idUsuario,
                        idUsuario,
                        false,
                        ct);

                if (detalle is null)
                    return Results.NotFound();

                // VALIDAR ACCESO A FINANZAS
                var tieneFinanzas =
                    perfilPermisos.Rol.Equals(
                        "ADMINISTRADOR",
                        StringComparison.OrdinalIgnoreCase);

                // CONSTRUIR RESPUESTA
                var respuesta = new PerfilActualResponse(
                    detalle.NombreUsuario,
                    perfilPermisos.Rol,
                    tieneFinanzas,
                    detalle.IdEmpleado,
                    $"{detalle.Nombres} {detalle.Apellidos}".Trim(),
                    detalle.Identidad,
                    detalle.Nombres,
                    detalle.Apellidos,
                    detalle.FechaNacimiento,
                    detalle.Genero,
                    detalle.Telefono,
                    detalle.CorreoLogin,
                    detalle.ImagenPerfil
                );

                return Results.Ok(respuesta);
            })
            .RequireAuthorization();

            // ==========================================
            // MODIFICAR DATOS PERSONALES DEL PERFIL
            // ==========================================

            app.MapPut("/api/auth/me/datos", async (
                ClaimsPrincipal usuario,
                ModificarUsuarioRequest req,
                IUsuariosService usuarios,
                CancellationToken ct) =>
            {
                // OBTENER ID DEL USUARIO AUTENTICADO
                if (!int.TryParse(
                    usuario.FindFirstValue("sub"),
                    out int idUsuario))
                {
                    return Results.Unauthorized();
                }

                // UTILIZAR EL ID DEL TOKEN
                // PARA IMPEDIR MODIFICAR OTRO USUARIO
                var solicitud = req with
                {
                    IdUsuario = idUsuario
                };

                // ACTUALIZAR DATOS EN LA MISMA TABLA
                // UTILIZADA POR SEGURIDAD -> USUARIOS
                await usuarios.ModificarAsync(
                    solicitud,
                    idUsuario,
                    false,
                    ct);

                return Results.NoContent();
            })
            .RequireAuthorization();


            // ==========================================
            // CAMBIAR CONTRASEÑA DESDE PERFIL
            // ==========================================

            app.MapPut("/api/auth/me/contrasena", async (
                ClaimsPrincipal usuario,
                CambiarMiContrasenaRequest req,
                IAuthService auth,
                ILogger<AuthService> logger) =>
                        {
                            string idSolicitud = Guid.NewGuid().ToString("N")[..8];

                            logger.LogInformation(
                                "CAMBIO CONTRASEÑA: inicio de solicitud {IdSolicitud}",
                                idSolicitud);

                // IDENTIFICAR AL USUARIO AUTENTICADO
                if (!int.TryParse(
                    usuario.FindFirstValue("sub"),
                    out int idUsuario))
                {
                    return Results.Unauthorized();
                }

                // IDENTIFICAR LA SESIÓN ACTUAL
                if (!Guid.TryParse(
                    usuario.FindFirstValue("sid"),
                    out Guid idSesion))
                {
                    return Results.Unauthorized();
                }

                // VERIFICAR Y ACTUALIZAR LA CONTRASEÑA
                
                try
                {
                    await auth.CambiarMiContrasenaAsync(
                        idUsuario,
                        idSesion,
                        req.ContrasenaActual,
                        req.NuevaContrasena,
                        req.ConfirmarContrasena);

                    logger.LogInformation(
                        "CAMBIO CONTRASEÑA: solicitud {IdSolicitud} completada",
                        idSolicitud);

                    return Results.NoContent();
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        ex,
                        "CAMBIO CONTRASEÑA: solicitud {IdSolicitud} falló",
                        idSolicitud);

                    throw;
                }

            })
            .RequireAuthorization();


            // ==========================================
            // FOTOGRAFÍA DEL PERFIL
            // ==========================================

            // SUBIR FOTO DEL USUARIO AUTENTICADO
            app.MapPost("/api/auth/me/imagen", async (
                ClaimsPrincipal usuario,
                IFormFile archivo,
                IUsuariosService usuarios,
                CancellationToken ct) =>
            {
                if (!int.TryParse(
                    usuario.FindFirstValue("sub"),
                    out int idUsuario))
                {
                    return Results.Unauthorized();
                }

                await usuarios.SubirImagenPerfilAsync(
                    idUsuario,
                    archivo,
                    ct);

                return Results.NoContent();
            })
            .Accepts<IFormFile>("multipart/form-data")
            .DisableAntiforgery()
            .RequireAuthorization();

            // OBTENER FOTO DEL USUARIO AUTENTICADO
            app.MapGet("/api/auth/me/imagen", async (
                ClaimsPrincipal usuario,
                IUsuariosService usuarios,
                CancellationToken ct) =>
            {
                if (!int.TryParse(
                    usuario.FindFirstValue("sub"),
                    out int idUsuario))
                {
                    return Results.Unauthorized();
                }

                var imagen =
                    await usuarios.ObtenerImagenPerfilAsync(
                        idUsuario,
                        ct);

                if (imagen is null)
                    return Results.NotFound();

                return Results.File(
                    imagen.Stream,
                    imagen.TipoContenido);
            })
            .RequireAuthorization();


            // ELIMINAR FOTO DEL USUARIO AUTENTICADO
            app.MapDelete("/api/auth/me/imagen", async (
                ClaimsPrincipal usuario,
                IUsuariosService usuarios,
                CancellationToken ct) =>
            {
                if (!int.TryParse(
                    usuario.FindFirstValue("sub"),
                    out int idUsuario))
                {
                    return Results.Unauthorized();
                }

                await usuarios.EliminarImagenPerfilAsync(idUsuario, ct);

                return Results.NoContent();
            })
            .RequireAuthorization();

            // ==========================================
            // CERRAR SESIÓN
            // ==========================================

            app.MapPost("/api/auth/logout", async (
                ClaimsPrincipal usuario,
                IAuthService auth) =>
            {
                var sidTexto =
                    usuario.FindFirstValue("sid");

                if (!Guid.TryParse(
                    sidTexto,
                    out var sid))
                {
                    return Results.Unauthorized();
                }

                await auth.CerrarSesionAsync(sid);

                return Results.NoContent();
            })
            .RequireAuthorization();
        }

        // ==========================================
        // INFORMACIÓN DEL EQUIPO DEL CLIENTE
        // ==========================================

        private static DatosCliente DatosDe(
            HttpContext ctx) => new(
                ctx.Connection.RemoteIpAddress?.ToString(),
                ctx.Request.Headers["X-Equipo"]
                    .FirstOrDefault()
                    ?? "Desconocido");
    }
}
