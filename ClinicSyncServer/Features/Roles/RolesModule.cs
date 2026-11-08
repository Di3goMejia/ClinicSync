using ClinicSyncServer.Features.Roles.Data;

namespace ClinicSyncServer.Features.Roles;

public static class RolesModule
{
    public static IServiceCollection AddRoles(this IServiceCollection services)
    {
        services.AddScoped<RolesRepository>();
        services.AddScoped<IRolesService, RolesService>();
        return services;
    }

    public static WebApplication MapRoles(this WebApplication app)
    {
        app.MapRolesEndpoints();
        return app;
    }
}
