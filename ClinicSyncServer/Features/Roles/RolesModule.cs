using ClinicSyncServer.Features.Roles.Data;

namespace ClinicSyncServer.Features.Roles;

/// <summary>
/// 
/// </summary>
public static class RolesModule
{
    /// <summary>
    /// Adds the roles.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <returns></returns>
    public static IServiceCollection AddRoles(this IServiceCollection services)
    {
        services.AddScoped<RolesRepository>();
        services.AddScoped<IRolesService, RolesService>();
        return services;
    }

    /// <summary>
    /// Maps the roles.
    /// </summary>
    /// <param name="app">The application.</param>
    /// <returns></returns>
    public static WebApplication MapRoles(this WebApplication app)
    {
        app.MapRolesEndpoints();
        return app;
    }
}
