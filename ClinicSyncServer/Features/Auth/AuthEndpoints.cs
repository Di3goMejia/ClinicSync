
using ClinicSyncServer.Features.Auth.Data;
using ClinicSyncServer.Features.Auth.Requests;
using ClinicSyncServer.Features.Auth.Responses;
using ClinicSyncServer.Features.Auth.Services;
using ClinicSyncServer.Features.Permisos;
using ClinicSyncServer.Features.Usuarios;
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
            // REGISTRAR RECONOCIMIENTO FACIAL
            // ==========================================

            app.MapPost("/api/auth/2fa/facial", async (
                ClaimsPrincipal usuario,
                RegistrarCredencialFacialRequest req,
                RegistroCredencialFacialService registro,
                CancellationToken ct) =>
            {
                if (!int.TryParse(
                    usuario.FindFirstValue("sub"),
                    out int idUsuario))
                {
                    return Results.Unauthorized();
                }

                await registro.RegistrarAsync(
                    idUsuario,
                    req.Muestras,
                    ct);

                return Results.Ok(new
                {
                    Mensaje = "Credencial facial registrada correctamente."
                });
            })
            .RequireAuthorization();

            // DESACTIVAR RECONOCIMIENTO FACIAL
            app.MapDelete("/api/auth/2fa/facial", async (
                ClaimsPrincipal usuario,
                Metodos2FARepository metodos2FA,
                CancellationToken ct) =>
            {
                if (!int.TryParse(
                    usuario.FindFirstValue("sub"),
                    out int idUsuario))
                {
                    return Results.Unauthorized();
                }

                bool desactivado = await metodos2FA.DesactivarAsync(
                    idUsuario,
                    ct);

                if (!desactivado)
                {
                    return Results.NotFound(new
                    {
                        Mensaje = "No existe un método facial activo para este usuario."
                    });
                }

                return Results.Ok(new
                {
                    Mensaje = "Reconocimiento facial desactivado correctamente."
                });
            })
            .RequireAuthorization();

            // ==========================================
            // CONSULTAR ESTADO DE AUTENTICACIÓN 2FA
            // ==========================================

            app.MapGet("/api/auth/2fa/estado", async (
                ClaimsPrincipal usuario,
                Metodos2FARepository metodos2FA,
                CancellationToken ct) =>
            {
                if (!int.TryParse(
                    usuario.FindFirstValue("sub"),
                    out int idUsuario))
                {
                    return Results.Unauthorized();
                }

                bool configurado =
                    await metodos2FA.TieneMetodoActivoAsync(
                        idUsuario,
                        ct);

                return Results.Ok(new
                {
                    Configurado = configurado
                });
            })
            .RequireAuthorization();

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

            // MODIFICAR DATOS DEL USUARIO AUTENTICADO
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

                // ACTUALIZAR DATOS
                await usuarios.ModificarAsync(
                    solicitud,
                    idUsuario,
                    false,
                    ct);

                return Results.NoContent();
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

                await usuarios.EliminarImagenPerfilAsync(
                    idUsuario,
                    ct);

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
