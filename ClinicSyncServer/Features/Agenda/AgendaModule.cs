using ClinicSyncServer.Features.Agenda.Data;

namespace ClinicSyncServer.Features.Agenda
{
    /// <summary>
    /// 
    /// </summary>
    public static class AgendaModule
    {
        /// <summary>
        /// Adds the agenda.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
        public static IServiceCollection AddAgenda(
            this IServiceCollection services)
        {
            services.AddScoped<AgendaRepository>();

            services.AddScoped<
                IAgendaService,
                AgendaService>();

            return services;
        }

        /// <summary>
        /// Maps the agenda module.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <returns></returns>
        public static WebApplication MapAgendaModule(
            this WebApplication app)
        {
            app.MapAgendaEndpoints();

            return app;
        }
    }
}