
using ClinicSyncServer.Features.Expedientes.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicSyncServer.Features.Expedientes
{
    /// <summary>
    /// 
    /// </summary>
    public static class ExpedientesModule
    {
        // REGISTRAR SERVICIOS
        /// <summary>
        /// Adds the expedientes.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Maps the expedientes module.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <returns></returns>
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
