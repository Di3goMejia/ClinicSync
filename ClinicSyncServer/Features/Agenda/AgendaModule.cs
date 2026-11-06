using ClinicSyncServer.Features.Agenda.Data;

namespace ClinicSyncServer.Features.Agenda
{
    public static class AgendaModule
    {
        public static IServiceCollection AddAgenda(
            this IServiceCollection services)
        {
            services.AddScoped<AgendaRepository>();

            services.AddScoped<
                IAgendaService,
                AgendaService>();

            return services;
        }

        public static WebApplication MapAgendaModule(
            this WebApplication app)
        {
            app.MapAgendaEndpoints();

            return app;
        }
    }
}