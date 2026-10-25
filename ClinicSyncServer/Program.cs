
using ClinicSyncServer.Common;
using ClinicSyncServer.Common.Seguridad;
using ClinicSyncServer.Features.Auth;
using ClinicSyncServer.Features.Permisos;
using ClinicSyncServer.Features.Sesiones;
using ClinicSyncServer.Features.Usuarios;
using ClinicSyncServer.Features.Tratamientos;
using ClinicSyncServer.Infrastructure;
using ClinicSyncServer.Infrastructure.Auth;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

/* Servicios */

// Errores estandarizados
builder.Services.AddProblemDetails(opciones =>
{
    opciones.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;

        if (ctx.ProblemDetails is HttpValidationProblemDetails)
            ctx.ProblemDetails.Title = "Datos no válidos.";
    };
});

// Validación de parámetros de Minimal API
builder.Services.AddValidation();

builder.Services
    .AddInfraestructura(builder.Configuration)  // ConnectionOptions, SqlConnectionFactory, hasher, correo, Dapper
    .AddJwtAuth(builder.Configuration)          // JwtOptions, ITokenService, autenticación Bearer
    .AddPoliticasPermisos()                     // FallbackPolicy + una política por permiso
    .AddPermisos()
    .AddSesiones()
    .AddAuth()
    .AddUsuarios()
    .AddTratamientos();  

var app = builder.Build();

/* Pipeline */

// Captura de errores.
app.UseMiddleware<ExceptionMiddleware>();

// Cultura para los mensajes de validación
var culturas = new[] { new CultureInfo("es-HN"), new CultureInfo("es") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es-HN"),
    SupportedCultures = culturas,
    SupportedUICultures = culturas
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

/* Endpoints */
app.MapAuth();
app.MapUsuarios();
app.MapTratamientos();

app.Run();