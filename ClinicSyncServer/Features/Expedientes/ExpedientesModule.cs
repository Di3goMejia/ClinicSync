
using ClinicSyncServer.Features.Expedientes.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicSyncServer.Features.Expedientes
{
    public static class ExpedientesModule
    {
        // REGISTRAR SERVICIOS
        public static IServiceCollection AddExpedientes(
            this IServiceCollection services)
        {
            services.AddScoped<ExpedientesRepository>();
            services.AddScoped<ArchivosExpedienteRepository>();
            services.AddScoped<VisitasRepository>();
            services.AddScoped<IExpedientesService, ExpedientesService>();
            services.AddScoped<IArchivosExpedienteService, ArchivosExpedienteService>();
            services.AddScoped<IVisitasService, VisitasService>();

            return services;
        }

        // REGISTRAR ENDPOINTS
        public static WebApplication MapExpedientesModule(
            this WebApplication app)
        {
            app.MapExpedientesEndpoints();
            app.MapArchivosExpedienteEndpoints();
            app.MapVisitasEndpoints();

            return app;
        }
    }
}
