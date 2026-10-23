using Microsoft.AspNetCore.Authorization;

namespace ClinicSyncServer.Common.Seguridad
{
    public static class PoliticasPermisos
    {
        public static IServiceCollection AddPoliticasPermisos(this IServiceCollection services)
        {
            services.AddAuthorization(opciones =>
            {
                opciones.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                foreach (var permiso in Permisos.Todos)
                {
                    opciones.AddPolicy(permiso, p =>
                        p.RequireClaim(ClaimsPersonalizados.Permiso, permiso));
                }
            });

            return services;
        }
    }
}
