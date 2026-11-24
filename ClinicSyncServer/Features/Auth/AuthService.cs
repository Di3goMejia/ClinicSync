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
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="ClinicSyncServer.Features.Auth.IAuthService" />
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
        /// <summary>
        /// The dias refresh
        /// </summary>
        private const int DiasRefresh = 30;
        /// <summary>
        /// The minutos restablecimiento
        /// </summary>
        private const int MinutosRestablecimiento = 30;
        /// <summary>
        /// The minimum longitud password
        /// </summary>
        private const int MinLongitudPassword = 8;
        /// <summary>
        /// The codigo politica
        /// </summary>
        private const int CodigoPolitica = 50012;
        /// <summary>
        /// The codigo enlace
        /// </summary>
        private const int CodigoEnlace = 50011;

        /// <summary>
        /// The mensaje generico
        /// </summary>
        private const string MensajeGenerico = "Usuario o contraseña incorrectos.";
        /// <summary>
        /// The sesion no valida
        /// </summary>
        private const string SesionNoValida = "La sesión no es válida o expiró.";

        /// <summary>
        /// Logins the asynchronous.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="cliente">The cliente.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        /// <exception cref="NoAutorizadoException"></exception>
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

        /// <summary>
        /// Verificars the segundo factor asynchronous.
        /// </summary>
        /// <param name="req">The req.</param>
        /// <param name="cliente">The cliente.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        /// <exception cref="NoAutorizadoException">
        /// El desafío no es válido o expiró.
        /// or
        /// La verificación no es válida.
        /// </exception>
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

        /// <summary>
        /// Renovars the asynchronous.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        /// <exception cref="NoAutorizadoException"></exception>
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

        /// <summary>
        /// Cerrars the sesion asynchronous.
        /// </summary>
        /// <param name="idSesion">The identifier sesion.</param>
        /// <returns></returns>
        public Task CerrarSesionAsync(Guid idSesion) =>
            sesiones.CerrarAsync(idSesion, "LOGOUT");

        /// <summary>
        /// Validars the token activacion asynchronous.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="ct">The ct.</param>
        /// <exception cref="ReglaNegocioException">
        /// Ingrese el código o token de activación. - 50011
        /// or
        /// El código es incorrecto o ha caducado. - 50011
        /// </exception>
        public async Task ValidarTokenActivacionAsync(string token, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ReglaNegocioException("Ingrese el código o token de activación.", 50011);

            var hash = ObtenerHashTokenActivacion(token);

            var valido = await tokens.ValidarTokenAsync(hash, "PRIMER_LOGIN", ct);
            if (!valido)
                throw new ReglaNegocioException("El código es incorrecto o ha caducado.", 50011);
        }

        /// <summary>
        /// Activars the cuenta asynchronous.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="nuevaPassword">The nueva password.</param>
        /// <param name="confirmarPassword">The confirmar password.</param>
        /// <exception cref="ReglaNegocioException">
        /// Ingrese el código o token de activación. - 50011
        /// or
        /// Las contraseñas no coinciden. - 50011
        /// </exception>
        public async Task ActivarCuentaAsync(string token, string nuevaPassword, string? confirmarPassword = null)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ReglaNegocioException("Ingrese el código o token de activación.", 50011);

            if (confirmarPassword is not null && nuevaPassword != confirmarPassword)
                throw new ReglaNegocioException("Las contraseñas no coinciden.", 50011);

            await UsarTokenAsync(token.Trim(), "PRIMER_LOGIN", nuevaPassword);
        }

        /// <summary>
        /// Restablecers the contrasena asynchronous.
        /// </summary>
        /// <param name="identificador">The identificador.</param>
        /// <param name="codigo">The codigo.</param>
        /// <param name="nuevaPassword">The nueva password.</param>
        /// <param name="confirmarPassword">The confirmar password.</param>
        /// <exception cref="ReglaNegocioException">
        /// Ingrese su correo electrónico. - 50012
        /// or
        /// Ingrese el código de recuperación. - 50011
        /// or
        /// El código debe contener exactamente 6 dígitos. - 50011
        /// or
        /// Ingrese la nueva contraseña. - 50011
        /// or
        /// La contraseña debe tener al menos 8 caracteres. - 50011
        /// or
        /// Las contraseñas no coinciden. - 50011
        /// or
        /// El código es incorrecto o ha caducado. - 50012
        /// </exception>
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

        /// <summary>
        /// Solicitars the restablecimiento asynchronous.
        /// </summary>
        /// <param name="identificador">The identificador.</param>
        /// <param name="ct">The ct.</param>
        /// <exception cref="ReglaNegocioException">Ingrese su correo electrónico. - 50012</exception>
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

        /// <summary>
        /// Obteners the hash token activacion.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Usars the token asynchronous.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="proposito">The proposito.</param>
        /// <param name="nuevaPassword">The nueva password.</param>
        private async Task UsarTokenAsync(string token, string proposito, string nuevaPassword)
        {
            ValidarPolitica(nuevaPassword);

            var hash = ObtenerHashTokenActivacion(token);

            // El procedimiento valida, consume el token y revoca las sesiones en una transacción
            await credenciales.EstablecerPasswordAsync(hash, proposito, hasher.Hash(nuevaPassword));
        }

        /// <summary>
        /// Emitirs the sesion asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="cliente">The cliente.</param>
        /// <returns></returns>
        /// <exception cref="NoAutorizadoException"></exception>
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

        /// <summary>
        /// Validars the politica.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <exception cref="ReglaNegocioException">La contraseña debe tener al menos {MinLongitudPassword} caracteres, con letras y números.</exception>
        private static void ValidarPolitica(string password)
        {
            if (password.Length < MinLongitudPassword || !password.Any(char.IsDigit) || !password.Any(char.IsLetter))
                throw new ReglaNegocioException(
                    $"La contraseña debe tener al menos {MinLongitudPassword} caracteres, con letras y números.",
                    CodigoPolitica);
        }

        /// <summary>
        /// Obteners the hash codigo.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="codigo">The codigo.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// No se configuró la clave de recuperación.
        /// or
        /// La clave de recuperación es demasiado corta.
        /// </exception>
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

        /// <summary>
        /// Normalizars the specified valor.
        /// </summary>
        /// <param name="valor">The valor.</param>
        /// <returns></returns>
        private static string Normalizar(string valor) => valor.Trim().ToLowerInvariant();

        // CAMBIAR CONTRASEÑA DESDE PERFIL
        /// <summary>
        /// Cambiars the mi contrasena asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="idSesionActual">The identifier sesion actual.</param>
        /// <param name="actual">The actual.</param>
        /// <param name="nueva">The nueva.</param>
        /// <param name="confirmacion">The confirmacion.</param>
        /// <exception cref="ReglaNegocioException">
        /// Ingrese su contraseña actual. - 50012
        /// or
        /// Ingrese la nueva contraseña. - 50012
        /// or
        /// Las contraseñas no coinciden. - 50012
        /// or
        /// La nueva contraseña debe ser diferente a la actual. - 50012
        /// or
        /// La nueva contraseña debe contener al menos " +
        ///                     "8 caracteres, una mayúscula, una minúscula, " +
        ///                     "un número y un carácter especial. - 50012
        /// or
        /// La contraseña actual es incorrecta. - 50012
        /// </exception>
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