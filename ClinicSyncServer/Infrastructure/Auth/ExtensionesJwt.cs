using ClinicSyncServer.Common.Abstracciones;
using ClinicSyncServer.Common.Opciones;
using ClinicSyncServer.Common.Seguridad;
using ClinicSyncServer.Features.Sesiones.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace ClinicSyncServer.Infrastructure.Auth
{
    public static class ParametrosJwt
    {
        public static SymmetricSecurityKey CrearClave(OpcionesJwt jwt)
        {
            var bytes = Encoding.UTF8.GetBytes(jwt.Key ?? string.Empty);

            // HS256 (utilizado por JWT) requiere al menos 256 bits.
            if (bytes.Length < 32)
                throw new InvalidOperationException(
                    "La clave JWT debe tener al menos 32 bytes. Revise que se haya registrado la clave.");

            return new SymmetricSecurityKey(bytes);
        }

        public static TokenValidationParameters Crear(OpcionesJwt jwt, string audiencia) => new()
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = audiencia,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = CrearClave(jwt),

            ClockSkew = TimeSpan.FromSeconds(30),

            NameClaimType = "name",
            RoleClaimType = ClaimsPersonalizados.Rol
        };
    }

    public static class ExtensionesJwt
    {
        public static IServiceCollection AddJwtAuth(
            this IServiceCollection services, IConfiguration config)
        {
            services.Configure<OpcionesJwt>(config.GetSection("Jwt"));

            var jwt = config.GetSection("Jwt").Get<OpcionesJwt>()
                      ?? throw new InvalidOperationException("Falta la sección Jwt.");

            ParametrosJwt.CrearClave(jwt);

            services.AddSingleton<ITokenService, TokenService>();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opt =>
                {
                    opt.MapInboundClaims = false;
                    opt.TokenValidationParameters = ParametrosJwt.Crear(jwt, jwt.Audience);

                    opt.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async ctx =>
                        {
                            var sesiones = ctx.HttpContext.RequestServices.GetRequiredService<SesionesRepository>();

                            var sid = ctx.Principal?.FindFirstValue("sid");
                            var sub = ctx.Principal?.FindFirstValue("sub");

                            if (!Guid.TryParse(sid, out var idSesion)
                                || !int.TryParse(sub, out var idUsuario)
                                || !await sesiones.EsActivaAsync(idSesion, idUsuario))
                            {
                                ctx.Fail("La sesión ya no está activa.");
                            }
                        }
                    };
                });

            return services;
        }
    }
}
