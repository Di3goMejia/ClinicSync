
using ClinicSyncServer.Features.Pacientes.Data;

namespace ClinicSyncServer.Features.Pacientes
{
    /// <summary>
    /// 
    /// </summary>
    public static class PacientesModule
    {
        /// <summary>
        /// Adds the pacientes.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddPacientes(
            this IServiceCollection services)
        {
            services.AddScoped<PacientesRepository>();

            services.AddScoped<
                IPacientesService,
                PacientesService>();

            return services;
        }

        /// <summary>
        /// Maps the pacientes module.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <returns></returns>
        public static WebApplication MapPacientesModule(
            this WebApplication app)
        {
            app.MapPacientesEndpoints();

            return app;
        }
    }
}
