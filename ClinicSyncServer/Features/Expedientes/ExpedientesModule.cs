
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

            return services;
        }

        // REGISTRAR ENDPOINTS
        public static WebApplication MapExpedientesModule(
            this WebApplication app)
        {
            app.MapExpedientesEndpoints();

            // ENDPOINTS DE ARCHIVOS
            app.MapArchivosExpedienteEndpoints();

            return app;
        }
    }
}
