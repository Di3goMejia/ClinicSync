using ClinicSyncServer.Features.Usuarios.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using PermisosSeguridad = ClinicSyncServer.Common.Seguridad.Permisos;
using ClaimsPersonalizados = ClinicSyncServer.Common.Seguridad.ClaimsPersonalizados;

namespace ClinicSyncServer.Features.Usuarios
{
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

            // ENDPOINT LISTAR
            usuarios.MapGet("/", async (IUsuariosService svc) =>
            {
                var listado = await svc.ListarAsync();
                return Results.Ok(listado);
            })
            .WithName("ListarUsuarios")
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
            usuarios.MapPut("/{id:int}", async (int id, [FromBody] ModificarUsuarioRequest req, IUsuariosService svc) =>
            {
                var solicitud = req with { IdUsuario = id };
                await svc.ModificarAsync(solicitud);

                return Results.NoContent();
            })
            .WithName("ModificarUsuario")
            .RequireAuthorization(PermisosSeguridad.UsuariosModificar);
        }
    }
}
