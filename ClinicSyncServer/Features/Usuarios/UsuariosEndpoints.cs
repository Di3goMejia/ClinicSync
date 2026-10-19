using ClinicSyncServer.Common;
using ClinicSyncServer.Features.Usuarios.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClinicSyncServer.Features.Usuarios
{
    public static class UsuariosEndpoints
    {   
        public static void MapUsuariosEndpoints(this WebApplication app)
        {
            var auth = app.MapGroup("/api/usuarios/").RequireAuthorization();

            // ENDPOINT CREAR
            auth.MapPost("/", async (IUsuariosService svc, [FromBody] CrearUsuarioRequest usuario) =>
            {
                var resp = await svc.CrearAsync(usuario);

                return Results.Created($"/api/usuarios/{resp.IdUsuario}", resp);
            })
            .WithName("CrearUsuario")
            .RequireAuthorization(Common.Seguridad.Permisos.UsuariosCrear);

            // ENDPOINT OBTENER USUARIO
            auth.MapGet("/{id:int}", async (int id, ClaimsPrincipal usuario, IUsuariosService svc) =>
            {
                var idSolicitante = int.Parse(usuario.FindFirstValue("sub")!);
                var puedeLeerTodos = usuario.HasClaim(
                    Common.Seguridad.ClaimsPersonalizados.Permiso, 
                    Common.Seguridad.Permisos.UsuariosLeer
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
