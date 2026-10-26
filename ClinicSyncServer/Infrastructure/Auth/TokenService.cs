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
    public class TokenService : ITokenService
    {
        // El desafío de 2FA usa una audiencia distinta, así el middleware de la API
        // rechaza ese token como si fuera un acceso.
        private const string SufijoAudienciaDesafio = ":2fa";

        private readonly OpcionesJwt _jwt;
        private readonly SigningCredentials _credenciales;

        public TokenService(IOptions<OpcionesJwt> opciones)
        {
            _jwt = opciones.Value;
            _credenciales = new SigningCredentials(
                ParametrosJwt.CrearClave(_jwt), 
                SecurityAlgorithms.HmacSha256
            );
        }

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

        public RefreshTokenGenerado GenerarRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);

            // Al cliente se le entrega el valor; a la BD solo el hash (usp_Sesiones_*)
            return new RefreshTokenGenerado(Convert.ToBase64String(bytes), SHA256.HashData(bytes));
        }

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

        private string AudienciaDesafio => _jwt.Audience + SufijoAudienciaDesafio;

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
