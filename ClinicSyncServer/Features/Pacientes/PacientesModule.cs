
using ClinicSyncServer.Features.Pacientes.Data;

namespace ClinicSyncServer.Features.Pacientes
{
    public static class PacientesModule
    {
        public static IServiceCollection AddPacientes(
            this IServiceCollection services)
        {
            services.AddScoped<PacientesRepository>();

            services.AddScoped<
                IPacientesService,
                PacientesService>();

            return services;
        }

        public static WebApplication MapPacientesModule(
            this WebApplication app)
        {
            app.MapPacientesEndpoints();

            return app;
        }
    }
}
