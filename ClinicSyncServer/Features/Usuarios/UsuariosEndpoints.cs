using ClinicSyncServer.Features.Usuarios.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using PermisosSeguridad = ClinicSyncServer.Common.Seguridad.Permisos;
using ClaimsPersonalizados = ClinicSyncServer.Common.Seguridad.ClaimsPersonalizados;

namespace ClinicSyncServer.Features.Usuarios
{
    /// <summary>
    /// 
    /// </summary>
    public static class UsuariosEndpoints
    {   
        public static void MapUsuariosEndpoints(this WebApplication app)
        {
            var usuarios = app.MapGroup("/api/usuarios/").RequireAuthorization();

            // ENDPOINT CREAR
            usuarios.MapPost("/", async (IUsuariosService svc, [FromBody] CrearUsuarioRequest usuario) =>
            {
                var resp = await svc.CrearAsync(usuario);

                return Results.Created($"/api/usuarios/{resp.IdUsuario}", resp);
            })
            .WithName("CrearUsuario")
            .RequireAuthorization(PermisosSeguridad.UsuariosCrear);

            usuarios.MapPost("/{id:int}/imagen", async (
                int id,
                IFormFile archivo,
                IUsuariosService svc,
                CancellationToken ct) =>
            {
                await svc.SubirImagenPerfilAsync(id, archivo, ct);
                return Results.NoContent();
            })
            .Accepts<IFormFile>("multipart/form-data")
            .DisableAntiforgery()
            .WithName("SubirImagenPerfilUsuario")
            .RequireAuthorization(PermisosSeguridad.UsuariosCrear);

            // ENDPOINT LISTAR
            usuarios.MapGet("/", async (IUsuariosService svc) =>
            {
                var listado = await svc.ListarAsync();
                return Results.Ok(listado);
            })
            .WithName("ListarUsuarios")
            .RequireAuthorization(PermisosSeguridad.UsuariosLeer);

            usuarios.MapGet("/{id:int}/imagen", async (
                int id,
                IUsuariosService svc,
                CancellationToken ct) =>
            {
                var imagen = await svc.ObtenerImagenPerfilAsync(id, ct);
                return imagen is null
                    ? Results.NotFound()
                    : Results.File(imagen.Stream, imagen.TipoContenido);
            })
            .WithName("ObtenerImagenPerfilUsuario")
            .RequireAuthorization(PermisosSeguridad.UsuariosLeer);

            // ENDPOINT OBTENER USUARIO POR ID
            usuarios.MapGet("/{id:int}", async (int id, ClaimsPrincipal usuario, IUsuariosService svc) =>
            {
                var idSolicitante = int.Parse(usuario.FindFirstValue("sub")!);
                var puedeLeerTodos = usuario.HasClaim(
                    ClaimsPersonalizados.Permiso, 
                    PermisosSeguridad.UsuariosLeer
                );

                var detalle = await svc.ObtenerAsync(id, idSolicitante, puedeLeerTodos);
                return detalle is null
                    ? Results.NotFound()
                    : Results.Ok(detalle);
            })
            .WithName("ObtenerUsuario")
            .RequireAuthorization(PermisosSeguridad.UsuariosLeer);

            // ENDPOINT MODIFICAR USUARIO POR ID
            usuarios.MapPut("/{id:int}", async (int id, [FromBody] ModificarUsuarioRequest req, ClaimsPrincipal usuario, IUsuariosService svc) =>
            {
                var idSolicitante = int.Parse(usuario.FindFirstValue("sub")!);
                var puedeEditarTodos = usuario.HasClaim(
                    ClaimsPersonalizados.Permiso,
                    PermisosSeguridad.UsuariosModificar
                );

                var solicitud = req with { IdUsuario = id };
                await svc.ModificarAsync(solicitud, idSolicitante, puedeEditarTodos);

                return Results.NoContent();
            })
            .WithName("ModificarUsuario");

            // ENDPOINT MODIFICAR ROL DE USUARIO
            usuarios.MapPatch("/{id:int}/rol", async (int id, [FromBody] ModificarRolUsuarioRequest req, ClaimsPrincipal usuario, IUsuariosService svc) =>
            {
                var idSolicitante = int.Parse(usuario.FindFirstValue("sub")!);

                var solicitud = req with { IdUsuario = id };
                await svc.ModificarRolAsync(solicitud, idSolicitante);

                return Results.NoContent();
            })
            .WithName("ModificarRolUsuario")
            .RequireAuthorization(PermisosSeguridad.UsuariosModificar);


            // ENDPOINT REENVIAR ACTIVACIÓN
            usuarios.MapPost("/{id:int}/reenviar-activacion", async (
                int id, IUsuariosService svc, CancellationToken ct) =>
            {
                await svc.ReenviarActivacionAsync(id, ct);
                return Results.Ok(new { mensaje = "La solicitud de activación fue reenviada." });
            })
            .WithName("ReenviarActivacionUsuario")
            .RequireAuthorization(PermisosSeguridad.UsuariosModificar);

            // ENDPOINT DESACTIVAR USUARIO;
            usuarios.MapPost("/{id:int}/desactivar", async (
                int id,
                ClaimsPrincipal administrador,
                [FromBody] ConfirmarAccionUsuarioRequest req,
                IUsuariosService svc,
                CancellationToken ct) =>
            {
                var idAdministrador = int.Parse(administrador.FindFirstValue("sub")!);
                await svc.DesactivarAsync(id, idAdministrador, req.Password, ct);
                return Results.Ok(new { mensaje = "El usuario fue desactivado." });
            })
            .WithName("DesactivarUsuario")
            .RequireAuthorization(PermisosSeguridad.UsuariosEstado);

            usuarios.MapPost("/{id:int}/reactivar", async (
                int id,
                ClaimsPrincipal administrador,
                [FromBody] ConfirmarAccionUsuarioRequest req,
                IUsuariosService svc,
                CancellationToken ct) =>
            {
                var idAdministrador = int.Parse(administrador.FindFirstValue("sub")!);
                await svc.ReactivarAsync(id, idAdministrador, req.Password, ct);
                return Results.Ok(new { mensaje = "El usuario fue reactivado." });
            })
            .WithName("ReactivarUsuario")
            .RequireAuthorization(PermisosSeguridad.UsuariosEstado);

            // ENDPOINT RESTABLECER CONTRASEÑA
            usuarios.MapPost("/{id:int}/restablecer-password", async (
                int id,
                ClaimsPrincipal administrador,
                [FromBody] ConfirmarAccionUsuarioRequest req,
                IUsuariosService svc,
                CancellationToken ct) =>
            {
                var idAdministrador = int.Parse(administrador.FindFirstValue("sub")!);
                await svc.RestablecerPasswordAsync(id, idAdministrador, req.Password, ct);
                return Results.Ok(new { mensaje = "La solicitud de restablecimiento fue enviada." });
            })
            .WithName("RestablecerPasswordUsuario")
            .RequireAuthorization(PermisosSeguridad.UsuariosModificar);
        }
    }
}
