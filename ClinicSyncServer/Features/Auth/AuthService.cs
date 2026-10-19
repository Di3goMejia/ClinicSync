using ClinicSyncServer.Common;
using ClinicSyncServer.Common.Abstracciones;
using ClinicSyncServer.Features.Auth.Data;
using ClinicSyncServer.Features.Auth.Factores;
using ClinicSyncServer.Features.Auth.Requests;
using ClinicSyncServer.Features.Auth.Responses;
using ClinicSyncServer.Features.Permisos;
using ClinicSyncServer.Features.Sesiones;

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
        IEmailSender correo) : IAuthService
    {
        private const int DiasRefresh = 30;
        private const int MinutosRestablecimiento = 30;
        private const int MinLongitudPassword = 12;
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

        public async Task ActivarCuentaAsync(string token, string nuevaPassword) =>
            await UsarTokenAsync(token, "PRIMER_ACCESO", nuevaPassword);

        public async Task RestablecerContrasenaAsync(string token, string nuevaPassword) =>
            await UsarTokenAsync(token, "NUEVA_CLAVE", nuevaPassword);

        public async Task SolicitarRestablecimientoAsync(string identificador, CancellationToken ct = default)
        {
            var contacto = await credenciales.ObtenerContactoAsync(Normalizar(identificador));

            // El controlador responde 202 siempre. Sin cuenta, no se hace nada más.
            if (contacto is null) 
                return;

            var token = tokenService.GenerarRefreshToken(); // generador de 32 bytes aleatorios, mismo esquema
            await tokens.InsertarAsync(contacto.IdUsuario, token.Hash, "NUEVA_CLAVE",
                DateTime.UtcNow.AddMinutes(MinutosRestablecimiento));

            await correo.EnviarRestablecimientoAsync(contacto.Correo, token.Token, ct);
        }

        private async Task UsarTokenAsync(string token, string proposito, string nuevaPassword)
        {
            ValidarPolitica(nuevaPassword);

            var hash = tokenService.HashRefreshToken(token)
                ?? throw new ReglaNegocioException("El enlace no es válido o expiró.", CodigoEnlace);

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

        private static string Normalizar(string valor) => valor.Trim().ToLowerInvariant();
    }
}