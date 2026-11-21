using ClinicSyncServer.Common;
using ClinicSyncServer.Common.Abstracciones;
using ClinicSyncServer.Features.Auth.Data;
using ClinicSyncServer.Features.Auth.Factores;
using ClinicSyncServer.Features.Auth.Requests;
using ClinicSyncServer.Features.Auth.Responses;
using ClinicSyncServer.Features.Permisos;
using ClinicSyncServer.Features.Sesiones;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ClinicSyncServer.Features.Auth
{
    public class AuthService(
        CredencialesRepository credenciales,
        TokensUsuarioRepository tokens,
        IPermisosService permisos,
        ISesionService sesiones,
        IPasswordHasher hasher,
        ITokenService tokenService,
        IFactorSegundoService factores,
        IEmailSender correo,
        IConfiguration config) : IAuthService
    {
        private const int DiasRefresh = 30;
        private const int MinutosRestablecimiento = 30;
        private const int MinLongitudPassword = 8;
        private const int CodigoPolitica = 50012;
        private const int CodigoEnlace = 50011;

        private const string MensajeGenerico = "Usuario o contraseña incorrectos.";
        private const string SesionNoValida = "La sesión no es válida o expiró.";

        public async Task<LoginResponse> LoginAsync(LoginRequest req, DatosCliente cliente, CancellationToken ct = default)
        {
            var u = await credenciales.ObtenerParaLoginAsync(Normalizar(req.Identificador));

            // Usuario inexistente, inactivo, sin contraseña o bloqueado: mismo mensaje y mismo tiempo
            if (u is null || u.Estado != "ACTIVO" || u.ClaveHash is null)
            {
                hasher.Comparar(req.Password, hasher.HashFicticio);
                throw new NoAutorizadoException(MensajeGenerico);
            }

            if (!hasher.Comparar(req.Password, u.ClaveHash))
            {
                throw new NoAutorizadoException(MensajeGenerico);
            }

            if (u.Requiere2FA)
            {
                var desafio = tokenService.GenerarDesafio2fa(u.IdUsuario);
                return new LoginResponse(true, desafio, null, null);
            }

            return await EmitirSesionAsync(u.IdUsuario, cliente);
        }

        public async Task<LoginResponse> VerificarSegundoFactorAsync(
            VerificarSegundoFactorRequest req, DatosCliente cliente, CancellationToken ct = default)
        {
            var idUsuario = tokenService.LeerDesafio2fa(req.Desafio)
                ?? throw new NoAutorizadoException("El desafío no es válido o expiró.");

            if (!await factores.VerificarAsync(idUsuario, req.Prueba, ct))
            {
                throw new NoAutorizadoException("La verificación no es válida.");
            }

            return await EmitirSesionAsync(idUsuario, cliente);
        }

        public async Task<LoginResponse> RenovarAsync(string refreshToken, CancellationToken ct = default)
        {
            var hashActual = tokenService.HashRefreshToken(refreshToken)
                ?? throw new NoAutorizadoException(SesionNoValida);

            var nuevo = tokenService.GenerarRefreshToken();

            var rotada = await sesiones.RotarAsync(hashActual, nuevo.Hash, ct)
                ?? throw new NoAutorizadoException(SesionNoValida);

            // Los permisos se releen aquí: así un cambio de rol se aplica en la siguiente renovación
            var perfil = await permisos.ObtenerPorUsuarioAsync(rotada.IdUsuario, ct)
                ?? throw new NoAutorizadoException(SesionNoValida);

            var acceso = tokenService.GenerarAccesoJwt(rotada.IdUsuario, rotada.IdSesion, perfil);
            return new LoginResponse(false, null, acceso, nuevo.Token);
        }

        public Task CerrarSesionAsync(Guid idSesion) =>
            sesiones.CerrarAsync(idSesion, "LOGOUT");

        public async Task ValidarTokenActivacionAsync(string token, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ReglaNegocioException("Ingrese el código o token de activación.", 50011);

            var hash = ObtenerHashTokenActivacion(token);

            var valido = await tokens.ValidarTokenAsync(hash, "PRIMER_LOGIN", ct);
            if (!valido)
                throw new ReglaNegocioException("El código es incorrecto o ha caducado.", 50011);
        }

        public async Task ActivarCuentaAsync(string token, string nuevaPassword, string? confirmarPassword = null)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ReglaNegocioException("Ingrese el código o token de activación.", 50011);

            if (confirmarPassword is not null && nuevaPassword != confirmarPassword)
                throw new ReglaNegocioException("Las contraseñas no coinciden.", 50011);

            await UsarTokenAsync(token.Trim(), "PRIMER_LOGIN", nuevaPassword);
        }

        public async Task RestablecerContrasenaAsync(
    string identificador,
    string codigo,
    string nuevaPassword,
    string confirmarPassword)
        {
            if (string.IsNullOrWhiteSpace(identificador))
                throw new ReglaNegocioException(
                    "Ingrese su correo electrónico.", 50012);

            if (string.IsNullOrWhiteSpace(codigo))
                throw new ReglaNegocioException(
                    "Ingrese el código de recuperación.", 50011);

            if (codigo.Length != 6 ||
                !codigo.All(c => c >= '0' && c <= '9'))
                throw new ReglaNegocioException(
                    "El código debe contener exactamente 6 dígitos.", 50011);

            if (string.IsNullOrWhiteSpace(nuevaPassword))
                throw new ReglaNegocioException(
                    "Ingrese la nueva contraseña.", 50011);

            if (nuevaPassword.Length < 8)
                throw new ReglaNegocioException(
                    "La contraseña debe tener al menos 8 caracteres.",50011);

            if (nuevaPassword != confirmarPassword)
                throw new ReglaNegocioException(
                    "Las contraseñas no coinciden.",50011);

            ValidarPolitica(nuevaPassword);

            var contacto =
                await credenciales.ObtenerContactoAsync(
                    Normalizar(identificador));

            if (contacto is null)
                throw new ReglaNegocioException(
                    "El código es incorrecto o ha caducado.", 50012);

            byte[] codigoHash =
                ObtenerHashCodigo(
                    contacto.IdUsuario,
                    codigo);

            await credenciales.EstablecerPasswordAsync(
                codigoHash,
                "NUEVA_CLAVE",
                hasher.Hash(nuevaPassword));
        }

        public async Task SolicitarRestablecimientoAsync(
    string identificador,
    CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(identificador))
                throw new ReglaNegocioException(
                    "Ingrese su correo electrónico.", 50012);

            identificador = Normalizar(identificador);

            var contacto =
                await credenciales.ObtenerContactoAsync(
                    identificador);

            // No revelar si existe la cuenta.
            if (contacto is null)
                return;

            // Generar número seguro de 6 dígitos.
            string codigo =
                RandomNumberGenerator.GetInt32(
                    0, 1000000).ToString("D6");

            // Nunca guardar el código original.
            byte[] codigoHash =
                ObtenerHashCodigo(
                    contacto.IdUsuario,
                    codigo);

            DateTime fechaExpiracion =
                DateTime.UtcNow.AddMinutes(
                    MinutosRestablecimiento);

            await tokens.InsertarAsync(
                contacto.IdUsuario,
                codigoHash,
                "NUEVA_CLAVE",
                fechaExpiracion);

            await correo.EnviarRestablecimientoAsync(
                contacto.Correo,
                codigo,
                ct);
        }

        private static byte[] ObtenerHashTokenActivacion(string token)
        {
            token = token.Trim();

            if (token.Length == 44 && token.EndsWith('='))
            {
                try
                {
                    var bytes = Convert.FromBase64String(token);
                    if (bytes.Length == 32)
                        return SHA256.HashData(bytes);
                }
                catch (FormatException)
                {
                    // Si no es base64 válido, continúa con UTF-8
                }
            }

            return SHA256.HashData(Encoding.UTF8.GetBytes(token));
        }

        private async Task UsarTokenAsync(string token, string proposito, string nuevaPassword)
        {
            ValidarPolitica(nuevaPassword);

            var hash = ObtenerHashTokenActivacion(token);

            // El procedimiento valida, consume el token y revoca las sesiones en una transacción
            await credenciales.EstablecerPasswordAsync(hash, proposito, hasher.Hash(nuevaPassword));
        }

        private async Task<LoginResponse> EmitirSesionAsync(int idUsuario, DatosCliente cliente)
        {
            // Sin permisos ni rol válido, no hay acceso
            var perfil = await permisos.ObtenerPorUsuarioAsync(idUsuario)
                ?? throw new NoAutorizadoException(MensajeGenerico);

            var refresh = tokenService.GenerarRefreshToken();
            var idSesion = await sesiones.CrearAsync(idUsuario, refresh.Hash, cliente.Equipo,
                cliente.Ip, DateTime.UtcNow.AddDays(DiasRefresh));

            await credenciales.RegistrarLoginExitosoAsync(idUsuario);

            var acceso = tokenService.GenerarAccesoJwt(idUsuario, idSesion, perfil);
            return new LoginResponse(false, null, acceso, refresh.Token);
        }

        private static void ValidarPolitica(string password)
        {
            if (password.Length < MinLongitudPassword || !password.Any(char.IsDigit) || !password.Any(char.IsLetter))
                throw new ReglaNegocioException(
                    $"La contraseña debe tener al menos {MinLongitudPassword} caracteres, con letras y números.",
                    CodigoPolitica);
        }

        private byte[] ObtenerHashCodigo(
        int idUsuario,
        string codigo)
        {
            string secreto = config["Recuperacion:ClaveSecreta"]
                ?? throw new InvalidOperationException(
                    "No se configuró la clave de recuperación.");

            byte[] clave = Convert.FromBase64String(secreto);

            if (clave.Length < 32)
                throw new InvalidOperationException(
                    "La clave de recuperación es demasiado corta.");

            string datos = $"{idUsuario}:{codigo}";

            return HMACSHA256.HashData(
                clave,
                Encoding.UTF8.GetBytes(datos));
        }

        private static string Normalizar(string valor) => valor.Trim().ToLowerInvariant();

        // CAMBIAR CONTRASEÑA DESDE PERFIL
        public async Task CambiarMiContrasenaAsync(
            int idUsuario,
            Guid idSesionActual,
            string actual,
            string nueva,
            string confirmacion)
        {
            if (string.IsNullOrWhiteSpace(actual))
            {
                throw new ReglaNegocioException(
                    "Ingrese su contraseña actual.", 50012);
            }

            if (string.IsNullOrWhiteSpace(nueva))
            {
                throw new ReglaNegocioException(
                    "Ingrese la nueva contraseña.", 50012);
            }

            if (nueva != confirmacion)
            {
                throw new ReglaNegocioException(
                    "Las contraseñas no coinciden.", 50012);
            }

            if (actual == nueva)
            {
                throw new ReglaNegocioException(
                    "La nueva contraseña debe ser diferente a la actual.",
                    50012);
            }

            // VALIDAR LOS REQUISITOS DE LA INTERFAZ
            if (nueva.Length < 8 ||
                !nueva.Any(char.IsUpper) ||
                !nueva.Any(char.IsLower) ||
                !nueva.Any(char.IsDigit) ||
                !nueva.Any(c =>
                    !char.IsLetterOrDigit(c) &&
                    !char.IsWhiteSpace(c)))
            {
                throw new ReglaNegocioException(
                    "La nueva contraseña debe contener al menos " +
                    "8 caracteres, una mayúscula, una minúscula, " +
                    "un número y un carácter especial.",
                    50012);
            }

            // OBTENER EL HASH ACTUAL DESDE SQL SERVER
            string? claveActual =
                await credenciales.ObtenerClaveAsync(idUsuario);

            if (string.IsNullOrWhiteSpace(claveActual) ||
                !hasher.Comparar(actual, claveActual))
            {
                throw new ReglaNegocioException(
                    "La contraseña actual es incorrecta.", 50012);
            }

            // GENERAR HASH ARGON2 CON EL SERVICIO EXISTENTE
            string nuevoHash = hasher.Hash(nueva);

            // GUARDAR Y REVOCAR LAS OTRAS SESIONES
            await credenciales.CambiarMiPasswordAsync(
                idUsuario,
                idSesionActual,
                nuevoHash);
        }
    }
}