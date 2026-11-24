using ClinicSyncServer.Common.Abstracciones;
using ClinicSyncServer.Common.Opciones;
using ClinicSyncServer.Common.Seguridad;
using ClinicSyncServer.Features.Permisos.Projections;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace ClinicSyncServer.Infrastructure.Auth
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClinicSyncServer.Common.Abstracciones.ITokenService" />
    public class TokenService : ITokenService
    {
        // El desafío de 2FA usa una audiencia distinta, así el middleware de la API
        // rechaza ese token como si fuera un acceso.
        /// <summary>
        /// The sufijo audiencia desafio
        /// </summary>
        private const string SufijoAudienciaDesafio = ":2fa";

        /// <summary>
        /// The JWT
        /// </summary>
        private readonly OpcionesJwt _jwt;
        /// <summary>
        /// The credenciales
        /// </summary>
        private readonly SigningCredentials _credenciales;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenService"/> class.
        /// </summary>
        /// <param name="opciones">The opciones.</param>
        public TokenService(IOptions<OpcionesJwt> opciones)
        {
            _jwt = opciones.Value;
            _credenciales = new SigningCredentials(
                ParametrosJwt.CrearClave(_jwt), 
                SecurityAlgorithms.HmacSha256
            );
        }

        /// <summary>
        /// Generars the acceso JWT.
        /// </summary>
        /// <param name="usuarioId">The usuario identifier.</param>
        /// <param name="sesionId">The sesion identifier.</param>
        /// <param name="perfil">The perfil.</param>
        /// <returns></returns>
        public string GenerarAccesoJwt(int usuarioId, Guid sesionId, PermisosUsuario perfil)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new("sid", sesionId.ToString()),
                new(ClaimsPersonalizados.Rol, perfil.Rol)
            };

            claims.AddRange(
                perfil.Permisos.Select(
                    p => new Claim(ClaimsPersonalizados.Permiso, p)
                )
            );

            System.Diagnostics.Debug.WriteLine(
    "ROL DEL USUARIO: " + perfil.Rol
);

            System.Diagnostics.Debug.WriteLine(
                "PERMISOS DEL USUARIO: " +
                string.Join(", ", perfil.Permisos)
            );

            return Emitir(claims, _jwt.Audience, DateTime.UtcNow.AddMinutes(_jwt.AccesoMinutos));
        }

        /// <summary>
        /// Generars the desafio2fa.
        /// </summary>
        /// <param name="usuarioId">The usuario identifier.</param>
        /// <returns></returns>
        public string GenerarDesafio2fa(int usuarioId)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimsPersonalizados.Proposito, "2fa")
            };

            return Emitir(claims, AudienciaDesafio, DateTime.UtcNow.AddMinutes(_jwt.DesafioMinutos));
        }

        /// <summary>
        /// Valida un desafío de 2FA y devuelve el Id del usuario, o null si no es válido.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public int? LeerDesafio2fa(string token)
        {
            var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };

            try
            {
                var principal = handler.ValidateToken(
                    token, ParametrosJwt.Crear(_jwt, AudienciaDesafio), out _);

                if (principal.FindFirstValue(ClaimsPersonalizados.Proposito) != "2fa")
                    return null;

                return int.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id)
                    ? id
                    : null;
            }
            catch (SecurityTokenException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                // Token mal formado
                return null;
            }
        }

        /// <summary>
        /// Generars the refresh token.
        /// </summary>
        /// <returns></returns>
        public RefreshTokenGenerado GenerarRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);

            // Al cliente se le entrega el valor; a la BD solo el hash (usp_Sesiones_*)
            return new RefreshTokenGenerado(Convert.ToBase64String(bytes), SHA256.HashData(bytes));
        }

        /// <summary>
        /// Hashes the refresh token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public byte[]? HashRefreshToken(string token)
        {
            try
            {
                return SHA256.HashData(Convert.FromBase64String(token));
            }
            catch (FormatException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the audiencia desafio.
        /// </summary>
        /// <value>
        /// The audiencia desafio.
        /// </value>
        private string AudienciaDesafio => _jwt.Audience + SufijoAudienciaDesafio;

        /// <summary>
        /// Emitirs the specified claims.
        /// </summary>
        /// <param name="claims">The claims.</param>
        /// <param name="audiencia">The audiencia.</param>
        /// <param name="expira">The expira.</param>
        /// <returns></returns>
        private string Emitir(IEnumerable<Claim> claims, string audiencia, DateTime expira)
        {
            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: audiencia,
                claims: claims,
                notBefore: null,
                expires: expira,
                signingCredentials: _credenciales
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
