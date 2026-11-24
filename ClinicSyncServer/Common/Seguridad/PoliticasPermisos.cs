using Microsoft.AspNetCore.Authorization;

namespace ClinicSyncServer.Common.Seguridad
{
    /// <summary>
    /// 
    /// </summary>
    public static class PoliticasPermisos
    {
        /// <summary>
        /// Adds the politicas permisos.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddPoliticasPermisos(this IServiceCollection services)
        {
            services.AddAuthorization(opciones =>
            {
                // Cualquier endpoint nuevo exige autenticación.
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
