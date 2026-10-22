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
            var auth = app.MapGroup("/api/usuarios/").RequireAuthorization();

            // ENDPOINT LISTAR
            auth.MapGet("/", async (IUsuariosService svc) =>
            {
                var resp = await svc.ListarAsync();
                return Results.Ok(resp);
            })
            .WithName("ListarUsuarios")
            .RequireAuthorization(PermisosSeguridad.UsuariosLeer);

            // ENDPOINT CREAR
            auth.MapPost("/", async (IUsuariosService svc, [FromBody] CrearUsuarioRequest usuario) =>
            {
                var resp = await svc.CrearAsync(usuario);

                return Results.Created($"/api/usuarios/{resp.IdUsuario}", resp);
            })
            .WithName("CrearUsuario")
            .RequireAuthorization(PermisosSeguridad.UsuariosCrear);

            // ENDPOINT OBTENER USUARIO
            auth.MapGet("/{id:int}", async (int id, ClaimsPrincipal usuario, IUsuariosService svc) =>
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
            .WithName("ObtenerUsuario");
        }
    }
}
