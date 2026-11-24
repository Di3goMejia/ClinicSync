using System;
using System.Net;
using System.Net.Http;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSyncDesktop.Data
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class RespuestaLogin
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="RespuestaLogin"/> is requiere2fa.
        /// </summary>
        /// <value>
        ///   <c>true</c> if requiere2fa; otherwise, <c>false</c>.
        /// </value>
        public bool Requiere2fa { get; set; }
        /// <summary>
        /// Gets or sets the desafio.
        /// </summary>
        /// <value>
        /// The desafio.
        /// </value>
        public string? Desafio { get; set; }
        /// <summary>
        /// Gets or sets the access token.
        /// </summary>
        /// <value>
        /// The access token.
        /// </value>
        public string? AccessToken { get; set; }
        /// <summary>
        /// Gets or sets the refresh token.
        /// </summary>
        /// <value>
        /// The refresh token.
        /// </value>
        public string? RefreshToken { get; set; }
    }


    /// <summary>
    /// 
    /// </summary>
    public sealed class PerfilActual
    {
        /// <summary>
        /// Gets or sets the nombre usuario.
        /// </summary>
        /// <value>
        /// The nombre usuario.
        /// </value>
        public string NombreUsuario { get; set; } = "";
        /// <summary>
        /// Gets or sets the rol.
        /// </summary>
        /// <value>
        /// The rol.
        /// </value>
        public string Rol { get; set; } = "";
        /// <summary>
        /// Gets or sets a value indicating whether [tiene finanzas].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [tiene finanzas]; otherwise, <c>false</c>.
        /// </value>
        public bool TieneFinanzas { get; set; }
        /// <summary>
        /// Gets or sets the identifier empleado.
        /// </summary>
        /// <value>
        /// The identifier empleado.
        /// </value>
        public int IdEmpleado { get; set; }
        /// <summary>
        /// Gets or sets the nombre empleado.
        /// </summary>
        /// <value>
        /// The nombre empleado.
        /// </value>
        public string NombreEmpleado { get; set; } = "";

        /// <summary>
        /// Gets or sets the identidad.
        /// </summary>
        /// <value>
        /// The identidad.
        /// </value>
        public string Identidad { get; set; } = "";
        /// <summary>
        /// Gets or sets the nombres.
        /// </summary>
        /// <value>
        /// The nombres.
        /// </value>
        public string Nombres { get; set; } = "";
        /// <summary>
        /// Gets or sets the apellidos.
        /// </summary>
        /// <value>
        /// The apellidos.
        /// </value>
        public string Apellidos { get; set; } = "";
        /// <summary>
        /// Gets or sets the fecha nacimiento.
        /// </summary>
        /// <value>
        /// The fecha nacimiento.
        /// </value>
        public DateTime FechaNacimiento { get; set; }
        /// <summary>
        /// Gets or sets the genero.
        /// </summary>
        /// <value>
        /// The genero.
        /// </value>
        public string Genero { get; set; } = "";
        /// <summary>
        /// Gets or sets the telefono.
        /// </summary>
        /// <value>
        /// The telefono.
        /// </value>
        public string Telefono { get; set; } = "";
        /// <summary>
        /// Gets or sets the correo contacto.
        /// </summary>
        /// <value>
        /// The correo contacto.
        /// </value>
        public string? CorreoContacto { get; set; }
        /// <summary>
        /// Gets or sets the imagen perfil.
        /// </summary>
        /// <value>
        /// The imagen perfil.
        /// </value>
        public string? ImagenPerfil { get; set; }

        /// <summary>
        /// Gets the nombre corto.
        /// </summary>
        /// <value>
        /// The nombre corto.
        /// </value>
        public string NombreCorto
        {
            get
            {
                string primerNombre = Nombres
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault() ?? "";

                string primerApellido = Apellidos
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault() ?? "";

                string nombre = $"{primerNombre} {primerApellido}".Trim();

                return string.IsNullOrWhiteSpace(nombre)
                    ? NombreUsuario
                    : nombre;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class Estado2FA
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Estado2FA"/> is configurado.
        /// </summary>
        /// <value>
        ///   <c>true</c> if configurado; otherwise, <c>false</c>.
        /// </value>
        public bool Configurado { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class ClienteApi
    {
        /// <summary>
        /// The cliente
        /// </summary>
        private static readonly HttpClient cliente = new()
        {
            BaseAddress = new Uri("http://localhost:5227/"),
            Timeout = TimeSpan.FromSeconds(60)
        };

        // CAMBIAR CONTRASEÑA DEL USUARIO AUTENTICADO
        /// <summary>
        /// Cambiars the mi contrasena asynchronous.
        /// </summary>
        /// <param name="actual">The actual.</param>
        /// <param name="nueva">The nueva.</param>
        /// <param name="confirmacion">The confirmacion.</param>
        /// <exception cref="InvalidOperationException">
        /// Debe ingresar su contraseña actual.
        /// or
        /// Debe ingresar una nueva contraseña.
        /// or
        /// Las contraseñas no coinciden.
        /// or
        /// La nueva contraseña debe ser diferente a la actual.
        /// </exception>
        public static async Task CambiarMiContrasenaAsync(
            string actual,
            string nueva,
            string confirmacion)
        {
            if (string.IsNullOrWhiteSpace(actual))
                throw new InvalidOperationException(
                    "Debe ingresar su contraseña actual.");

            if (string.IsNullOrWhiteSpace(nueva))
                throw new InvalidOperationException(
                    "Debe ingresar una nueva contraseña.");

            if (nueva != confirmacion)
                throw new InvalidOperationException(
                    "Las contraseñas no coinciden.");

            if (actual == nueva)
                throw new InvalidOperationException(
                    "La nueva contraseña debe ser diferente a la actual.");

            var datos = new
            {
                ContrasenaActual = actual,
                NuevaContrasena = nueva,
                ConfirmarContrasena = confirmacion
            };

            using var solicitud =
                CrearSolicitudCambioContrasena(datos);

            using var respuesta =
                await cliente.SendAsync(solicitud);

            System.Diagnostics.Debug.WriteLine(
                "CAMBIO CONTRASEÑA - HTTP: " +
                (int)respuesta.StatusCode);

            System.Diagnostics.Debug.WriteLine(
                "CAMBIO CONTRASEÑA - ÉXITO: " +
                respuesta.IsSuccessStatusCode);

            await VerificarRespuestaAsync(respuesta);

            System.Diagnostics.Debug.WriteLine(
                "CAMBIO CONTRASEÑA - ClienteApi finalizó correctamente.");
        }


        /// <summary>
        /// Crears the solicitud cambio contrasena.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datos">The datos.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">No hay una sesión iniciada.</exception>
        private static HttpRequestMessage CrearSolicitudCambioContrasena<T>(
            T datos)
        {
            string token = accessToken
                ?? throw new InvalidOperationException(
                    "No hay una sesión iniciada.");

            var solicitud = new HttpRequestMessage(
                HttpMethod.Put,
                "api/auth/me/contrasena");

            solicitud.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", token);

            solicitud.Headers.Add(
                "X-Equipo",
                Environment.MachineName);

            solicitud.Content = JsonContent.Create(
                datos,
                options: opciones);

            return solicitud;
        }

        /// <summary>
        /// The bloqueo renovacion
        /// </summary>
        private static readonly SemaphoreSlim bloqueoRenovacion =
            new(1, 1);

        /// <summary>
        /// The opciones
        /// </summary>
        private static readonly JsonSerializerOptions opciones =
            new(JsonSerializerDefaults.Web);

        /// <summary>
        /// The access token
        /// </summary>
        private static string? accessToken;
        /// <summary>
        /// The refresh token
        /// </summary>
        private static string? refreshToken;

        // INICIAR SESIÓN
        /// <summary>
        /// Iniciars the sesion asynchronous.
        /// </summary>
        /// <param name="usuario">The usuario.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// El servidor devolvió una respuesta vacía.
        /// or
        /// El servidor no devolvió los tokens de sesión.
        /// </exception>
        public static async Task<RespuestaLogin> IniciarSesionAsync(
            string usuario,
            string password)
        {
            LimpiarSesion();

            using var solicitud = new HttpRequestMessage(
                HttpMethod.Post,
                "api/auth/login");

            solicitud.Headers.Add(
                "X-Equipo",
                Environment.MachineName);

            solicitud.Content = JsonContent.Create(new
            {
                Identificador = usuario,
                Password = password
            });

            using var respuesta = await cliente.SendAsync(solicitud);

            await VerificarRespuestaAsync(respuesta);

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<RespuestaLogin>(
                    opciones)
                ?? throw new InvalidOperationException(
                    "El servidor devolvió una respuesta vacía.");

            if (!resultado.Requiere2fa)
            {
                if (string.IsNullOrWhiteSpace(resultado.AccessToken) ||
                    string.IsNullOrWhiteSpace(resultado.RefreshToken))
                {
                    throw new InvalidOperationException(
                        "El servidor no devolvió los tokens de sesión.");
                }

                accessToken = resultado.AccessToken;
                refreshToken = resultado.RefreshToken;
            }

            return resultado;
        }


        // ==========================================
        // VERIFICAR SEGUNDO FACTOR FACIAL
        // ==========================================

        /// <summary>
        /// Verificars the segundo factor asynchronous.
        /// </summary>
        /// <param name="desafio">The desafio.</param>
        /// <param name="imagenBase64">The imagen base64.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// El desafío de autenticación es obligatorio. - desafio
        /// or
        /// La captura facial es obligatoria. - imagenBase64
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// El servidor no devolvió el resultado de la verificación facial.
        /// or
        /// El servidor no devolvió una sesión válida después de verificar el segundo factor.
        /// </exception>
        public static async Task<RespuestaLogin> VerificarSegundoFactorAsync(
            string desafio,
            string imagenBase64)
        {
            if (string.IsNullOrWhiteSpace(desafio))
                throw new ArgumentException(
                    "El desafío de autenticación es obligatorio.",
                    nameof(desafio));

            if (string.IsNullOrWhiteSpace(imagenBase64))
                throw new ArgumentException(
                    "La captura facial es obligatoria.",
                    nameof(imagenBase64));

            using var respuesta = await cliente.PostAsJsonAsync(
                "api/auth/2fa",
                new
                {
                    Desafio = desafio,
                    Prueba = imagenBase64
                },
                opciones);

            await VerificarRespuestaAsync(respuesta);

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<RespuestaLogin>(
                    opciones)
                ?? throw new InvalidOperationException(
                    "El servidor no devolvió el resultado de la verificación facial.");

            if (resultado.Requiere2fa ||
                string.IsNullOrWhiteSpace(resultado.AccessToken) ||
                string.IsNullOrWhiteSpace(resultado.RefreshToken))
            {
                throw new InvalidOperationException(
                    "El servidor no devolvió una sesión válida después de verificar el segundo factor.");
            }

            accessToken = resultado.AccessToken;
            refreshToken = resultado.RefreshToken;

            return resultado;
        }


        // OBTENER PERFIL
        /// <summary>
        /// Obteners the perfil asynchronous.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">No se pudieron obtener los datos del usuario.</exception>
        public static async Task<PerfilActual> ObtenerPerfilAsync()
        {
            using var respuesta = await EnviarAutenticadoAsync(
                HttpMethod.Get,
                "api/auth/me");

            await VerificarRespuestaAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<PerfilActual>(
                opciones)
                ?? throw new InvalidOperationException(
                    "No se pudieron obtener los datos del usuario.");
        }

        // OBTENER ESTADO DE AUTENTICACIÓN 2FA
        /// <summary>
        /// Obteners the estado2 fa asynchronous.
        /// </summary>
        /// <returns></returns>
        public static async Task<Estado2FA> ObtenerEstado2FAAsync()
        {
            return await ObtenerAutenticadoAsync<Estado2FA>(
                "api/auth/2fa/estado");
        }

        // DESACTIVAR AUTENTICACIÓN DE DOS FACTORES
        /// <summary>
        /// Desactivar2s the fa asynchronous.
        /// </summary>
        public static async Task Desactivar2FAAsync()
        {
            using var respuesta = await DeleteAutenticadoAsync(
                "api/auth/2fa/facial");

            await VerificarRespuestaAsync(respuesta);
        }

        // OBTENER DATOS DESDE LA API
        /// <summary>
        /// Obteners the autenticado asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruta">The ruta.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">El servidor devolvió una respuesta vacía.</exception>
        public static async Task<T> ObtenerAutenticadoAsync<T>(
            string ruta)
        {
            using var respuesta = await EnviarAutenticadoAsync(
                HttpMethod.Get,
                ruta);

            await VerificarRespuestaAsync(respuesta);

            return await respuesta.Content.ReadFromJsonAsync<T>(
                opciones)
                ?? throw new InvalidOperationException(
                    "El servidor devolvió una respuesta vacía.");
        }

        // ENVIAR DATOS MEDIANTE POST
        /// <summary>
        /// Posts the autenticado asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruta">The ruta.</param>
        /// <param name="contenido">The contenido.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PostAutenticadoAsync<T>(
            string ruta,
            T contenido,
            CancellationToken ct = default)
        {
            return await EnviarJsonAutenticadoAsync<T>(HttpMethod.Post, ruta, contenido, ct);
        }

        /// <summary>
        /// Puts the autenticado asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruta">The ruta.</param>
        /// <param name="contenido">The contenido.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PutAutenticadoAsync<T>(
            string ruta,
            T contenido,
            CancellationToken ct = default)
        {
            return await EnviarJsonAutenticadoAsync<T>(HttpMethod.Put, ruta, contenido, ct);
        }

        /// <summary>
        /// Patches the autenticado asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ruta">The ruta.</param>
        /// <param name="contenido">The contenido.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> PatchAutenticadoAsync<T>(
            string ruta,
            T contenido,
            CancellationToken ct = default)
        {
            return await EnviarJsonAutenticadoAsync<T>(HttpMethod.Patch, ruta, contenido, ct);
        }

        // ENVIAR SOLICITUD MEDIANTE DELETE
        /// <summary>
        /// Deletes the autenticado asynchronous.
        /// </summary>
        /// <param name="ruta">The ruta.</param>
        /// <returns></returns>
        public static async Task<HttpResponseMessage> DeleteAutenticadoAsync(
            string ruta)
        {
            return await EnviarAutenticadoAsync(
                HttpMethod.Delete,
                ruta);
        }

        /// <summary>
        /// Subirs the archivo autenticado asynchronous.
        /// </summary>
        /// <param name="ruta">The ruta.</param>
        /// <param name="rutaArchivo">The ruta archivo.</param>
        /// <param name="nombreCampo">The nombre campo.</param>
        /// <param name="ct">The ct.</param>
        /// <exception cref="FileNotFoundException">No se encontró la imagen seleccionada.</exception>
        /// <exception cref="InvalidOperationException">
        /// No hay una sesión iniciada.
        /// or
        /// La sesión terminó.
        /// </exception>
        public static async Task SubirArchivoAutenticadoAsync(
            string ruta,
            string rutaArchivo,
            string nombreCampo,
            CancellationToken ct = default)
        {
            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException("No se encontró la imagen seleccionada.", rutaArchivo);

            var tokenAnterior = accessToken ??
                throw new InvalidOperationException("No hay una sesión iniciada.");

            async Task<HttpResponseMessage> EnviarAsync(string token)
            {
                using var solicitud = new HttpRequestMessage(HttpMethod.Post, ruta);
                solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                solicitud.Headers.Add("X-Equipo", Environment.MachineName);

                using var formulario = new MultipartFormDataContent();
                await using var archivo = File.OpenRead(rutaArchivo);
                using var contenido = new StreamContent(archivo);
                contenido.Headers.ContentType = new MediaTypeHeaderValue(
                    Path.GetExtension(rutaArchivo).ToLowerInvariant() switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        _ => "application/octet-stream"
                    });
                formulario.Add(contenido, nombreCampo, Path.GetFileName(rutaArchivo));
                solicitud.Content = formulario;
                return await cliente.SendAsync(solicitud, ct);
            }

            using var respuesta = await EnviarAsync(tokenAnterior);
            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                await bloqueoRenovacion.WaitAsync(ct);
                try
                {
                    if (accessToken == tokenAnterior)
                        await RenovarSesionAsync();
                }
                finally
                {
                    bloqueoRenovacion.Release();
                }

                using var reintento = await EnviarAsync(accessToken ??
                    throw new InvalidOperationException("La sesión terminó."));
                await VerificarRespuestaAsync(reintento);
                return;
            }

            await VerificarRespuestaAsync(respuesta);
        }

        // VALIDAR TOKEN DE ACTIVACIÓN
        /// <summary>
        /// Validars the token activacion asynchronous.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <exception cref="InvalidOperationException">El código o token de activación es obligatorio.</exception>
        public static async Task ValidarTokenActivacionAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("El código o token de activación es obligatorio.");

            using var respuesta = await cliente.PostAsJsonAsync(
                "api/auth/validar-token",
                new { Token = token.Trim() });

            await VerificarRespuestaAsync(respuesta);
        }

        /// <summary>
        /// Activars the cuenta asynchronous.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="nuevaPassword">The nueva password.</param>
        /// <param name="confirmarPassword">The confirmar password.</param>
        /// <exception cref="InvalidOperationException">
        /// El código o token de activación es obligatorio.
        /// or
        /// Las contraseñas no coinciden.
        /// </exception>
        public static async Task ActivarCuentaAsync(
            string token,
            string nuevaPassword,
            string confirmarPassword)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("El código o token de activación es obligatorio.");

            if (!string.Equals(nuevaPassword, confirmarPassword, StringComparison.Ordinal))
                throw new InvalidOperationException("Las contraseñas no coinciden.");

            using var respuesta = await cliente.PostAsJsonAsync(
                "api/auth/activar",
                new
                {
                    Token = token.Trim(),
                    NuevaPassword = nuevaPassword,
                    ConfirmarPassword = confirmarPassword
                });

            await VerificarRespuestaAsync(respuesta);
        }

        /// <summary>
        /// Enviars the json autenticado asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="metodo">The metodo.</param>
        /// <param name="ruta">The ruta.</param>
        /// <param name="contenido">The contenido.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// No hay una sesión iniciada.
        /// or
        /// La sesión terminó. Iniciá sesión nuevamente.
        /// </exception>
        private static async Task<HttpResponseMessage> EnviarJsonAutenticadoAsync<T>(
            HttpMethod metodo,
            string ruta,
            T contenido,
            CancellationToken ct = default)
        {
            var tokenUsado = accessToken ??
                throw new InvalidOperationException("No hay una sesión iniciada.");

            using var solicitud = new HttpRequestMessage(metodo, ruta);

            solicitud.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", tokenUsado);

            solicitud.Headers.Add("X-Equipo", Environment.MachineName);

            solicitud.Content = JsonContent.Create(
                contenido,
                options: opciones);

            var respuesta = await cliente.SendAsync(solicitud, ct);

            if (respuesta.StatusCode != HttpStatusCode.Unauthorized)
                return respuesta;

            respuesta.Dispose();

            await bloqueoRenovacion.WaitAsync(ct);

            try
            {
                if (accessToken == tokenUsado)
                    await RenovarSesionAsync();
            }
            finally
            {
                bloqueoRenovacion.Release();
            }

            string nuevoToken = accessToken
                ?? throw new InvalidOperationException(
                    "La sesión terminó. Iniciá sesión nuevamente.");

            using var reintento = new HttpRequestMessage(
                metodo,
                ruta);

            reintento.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", nuevoToken);

            reintento.Headers.Add(
                "X-Equipo",
                Environment.MachineName);

            reintento.Content = JsonContent.Create(
                contenido,
                options: opciones);

            return await cliente.SendAsync(reintento, ct);
        }

        // CAMBIAR ESTADO DE PACIENTE
        /// <summary>
        /// Cambiars the estado paciente asynchronous.
        /// </summary>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="activo">if set to <c>true</c> [activo].</param>
        /// <exception cref="InvalidOperationException">
        /// No hay una sesión iniciada.
        /// or
        /// La sesión terminó. Iniciá sesión nuevamente.
        /// </exception>
        public static async Task CambiarEstadoPacienteAsync(
            int idPaciente,
            bool activo)
        {
            string ruta =
                $"api/pacientes/{idPaciente}/estado";

            string tokenUsado = accessToken
                ?? throw new InvalidOperationException(
                    "No hay una sesión iniciada.");

            using var solicitud = new HttpRequestMessage(
                HttpMethod.Patch,
                ruta);

            solicitud.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    tokenUsado);

            solicitud.Headers.Add(
                "X-Equipo",
                Environment.MachineName);

            solicitud.Content = JsonContent.Create(
                new { Activo = activo },
                options: opciones);

            var respuesta = await cliente.SendAsync(solicitud);

            if (respuesta.StatusCode != HttpStatusCode.Unauthorized)
            {
                using (respuesta)
                {
                    await VerificarRespuestaAsync(respuesta);
                }

                return;
            }

            respuesta.Dispose();

            await bloqueoRenovacion.WaitAsync();

            try
            {
                if (accessToken == tokenUsado)
                    await RenovarSesionAsync();
            }
            finally
            {
                bloqueoRenovacion.Release();
            }

            string nuevoToken = accessToken
                ?? throw new InvalidOperationException(
                    "La sesión terminó. Iniciá sesión nuevamente.");

            using var reintento = new HttpRequestMessage(
                HttpMethod.Patch,
                ruta);

            reintento.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    nuevoToken);

            reintento.Headers.Add(
                "X-Equipo",
                Environment.MachineName);

            reintento.Content = JsonContent.Create(
                new { Activo = activo },
                options: opciones);

            using var segundaRespuesta =
                await cliente.SendAsync(reintento);

            await VerificarRespuestaAsync(segundaRespuesta);
        }


        // CREAR VISITA COMPLETA CON TRATAMIENTOS
        /// <summary>
        /// Crears the visita completa asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request">The request.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">El servidor no devolvió la visita creada.</exception>
        public static async Task<int> CrearVisitaCompletaAsync<T>(T request)
        {
            using var respuesta = await PostAutenticadoAsync(
                "api/visitas/completa", request);

            await VerificarRespuestaAsync(respuesta);

            var resultado = await respuesta.Content.ReadFromJsonAsync<VisitaCreada>(opciones)
                ?? throw new InvalidOperationException("El servidor no devolvió la visita creada.");

            return resultado.IdVisita;
        }

        // MODIFICAR VISITA COMPLETA CON TRATAMIENTOS
        /// <summary>
        /// Modificars the visita completa asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="request">The request.</param>
        /// <exception cref="ArgumentOutOfRangeException">idVisita</exception>
        public static async Task ModificarVisitaCompletaAsync<T>(int idVisita, T request)
        {
            if (idVisita <= 0)
                throw new ArgumentOutOfRangeException(nameof(idVisita));

            using var respuesta = await PutAutenticadoAsync(
                $"api/visitas/{idVisita}/completa", request);

            await VerificarRespuestaAsync(respuesta);
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class VisitaCreada
        {
            /// <summary>
            /// Gets or sets the identifier visita.
            /// </summary>
            /// <value>
            /// The identifier visita.
            /// </value>
            public int IdVisita { get; set; }
        }


        /// <summary>
        /// Subirs the archivo visita asynchronous.
        /// </summary>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="rutaArchivo">The ruta archivo.</param>
        /// <param name="descripcion">The descripcion.</param>
        /// <param name="ct">The ct.</param>
        /// <exception cref="ArgumentException">Visita inválida.</exception>
        /// <exception cref="FileNotFoundException">No se encontró el archivo.</exception>
        /// <exception cref="InvalidOperationException">
        /// No hay una sesión iniciada.
        /// or
        /// La sesión terminó.
        /// </exception>
        public static async Task SubirArchivoVisitaAsync(
            int idVisita,
            string rutaArchivo,
            string? descripcion = null,
            CancellationToken ct = default)
        {
            if (idVisita <= 0)
                throw new ArgumentException("Visita inválida.");

            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException("No se encontró el archivo.");

            string tokenAnterior = accessToken
                ?? throw new InvalidOperationException(
                    "No hay una sesión iniciada.");

            async Task<HttpResponseMessage> EnviarAsync(string token)
            {
                using var solicitud = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"api/visitas/{idVisita}/archivos");

                solicitud.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                solicitud.Headers.Add(
                    "X-Equipo",
                    Environment.MachineName);

                using var formulario = new MultipartFormDataContent();

                await using var archivo = File.OpenRead(rutaArchivo);

                using var contenido = new StreamContent(archivo);

                contenido.Headers.ContentType =
                    new MediaTypeHeaderValue(
                        Path.GetExtension(rutaArchivo).ToLowerInvariant() switch
                        {
                            ".jpg" or ".jpeg" => "image/jpeg",
                            ".png" => "image/png",
                            ".pdf" => "application/pdf",
                            _ => "application/octet-stream"
                        });

                formulario.Add(
                    contenido,
                    "archivo",
                    Path.GetFileName(rutaArchivo));

                formulario.Add(
                    new StringContent(descripcion ?? string.Empty),
                    "descripcion");

                solicitud.Content = formulario;

                return await cliente.SendAsync(solicitud, ct);
            }

            using var respuesta = await EnviarAsync(tokenAnterior);

            if (respuesta.StatusCode != HttpStatusCode.Unauthorized)
            {
                await VerificarRespuestaAsync(respuesta);
                return;
            }

            await bloqueoRenovacion.WaitAsync(ct);

            try
            {
                if (accessToken == tokenAnterior)
                    await RenovarSesionAsync();
            }
            finally
            {
                bloqueoRenovacion.Release();
            }

            string tokenNuevo = accessToken
                ?? throw new InvalidOperationException(
                    "La sesión terminó.");

            using var reintento = await EnviarAsync(tokenNuevo);

            await VerificarRespuestaAsync(reintento);
        }

        /// <summary>
        /// Subirs the archivo expediente asynchronous.
        /// </summary>
        /// <param name="idExpediente">The identifier expediente.</param>
        /// <param name="rutaArchivo">The ruta archivo.</param>
        /// <param name="descripcion">The descripcion.</param>
        /// <param name="ct">The ct.</param>
        /// <exception cref="ArgumentException">Expediente inválido.</exception>
        /// <exception cref="FileNotFoundException">No se encontró el archivo.</exception>
        /// <exception cref="InvalidOperationException">
        /// No hay una sesión iniciada.
        /// or
        /// La sesión terminó.
        /// </exception>
        public static async Task SubirArchivoExpedienteAsync(
            int idExpediente,
            string rutaArchivo,
            string? descripcion = null,
            CancellationToken ct = default)
        {
            if (idExpediente <= 0)
                throw new ArgumentException("Expediente inválido.");

            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException(
                    "No se encontró el archivo.",
                    rutaArchivo);

            string tokenAnterior = accessToken ??
                throw new InvalidOperationException(
                    "No hay una sesión iniciada.");

            async Task<HttpResponseMessage> EnviarAsync(string token)
            {
                using var solicitud = new HttpRequestMessage(
                    HttpMethod.Post,
                    $"api/expedientes/{idExpediente}/archivos");

                solicitud.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
                solicitud.Headers.Add(
                    "X-Equipo",
                    Environment.MachineName);

                using var formulario = new MultipartFormDataContent();
                await using var archivo = File.OpenRead(rutaArchivo);
                using var contenido = new StreamContent(archivo);

                contenido.Headers.ContentType =
                    new MediaTypeHeaderValue(
                        Path.GetExtension(rutaArchivo).ToLowerInvariant() switch
                        {
                            ".jpg" or ".jpeg" => "image/jpeg",
                            ".png" => "image/png",
                            ".pdf" => "application/pdf",
                            _ => "application/octet-stream"
                        });

                formulario.Add(
                    contenido,
                    "archivo",
                    Path.GetFileName(rutaArchivo));
                formulario.Add(
                    new StringContent(descripcion ?? string.Empty),
                    "descripcion");

                solicitud.Content = formulario;
                return await cliente.SendAsync(solicitud, ct);
            }

            using var respuesta = await EnviarAsync(tokenAnterior);

            if (respuesta.StatusCode != HttpStatusCode.Unauthorized)
            {
                await VerificarRespuestaAsync(respuesta);
                return;
            }

            await bloqueoRenovacion.WaitAsync(ct);
            try
            {
                if (accessToken == tokenAnterior)
                    await RenovarSesionAsync();
            }
            finally
            {
                bloqueoRenovacion.Release();
            }

            string tokenNuevo = accessToken ??
                throw new InvalidOperationException(
                    "La sesión terminó.");

            using var reintento = await EnviarAsync(tokenNuevo);
            await VerificarRespuestaAsync(reintento);
        }

        // SUBIR IMAGEN DE MATERIAL
        /// <summary>
        /// Subirs the imagen material asynchronous.
        /// </summary>
        /// <param name="rutaArchivo">The ruta archivo.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">No se encontró la imagen seleccionada.</exception>
        /// <exception cref="InvalidOperationException">
        /// No hay una sesión iniciada.
        /// or
        /// La sesión terminó. Inicia sesión nuevamente.
        /// </exception>
        public static async Task<string> SubirImagenMaterialAsync(
            string rutaArchivo,
            CancellationToken ct = default)
        {
            if (!File.Exists(rutaArchivo))
                throw new FileNotFoundException(
                    "No se encontró la imagen seleccionada.",
                    rutaArchivo);

            string tokenAnterior = accessToken
                ?? throw new InvalidOperationException(
                    "No hay una sesión iniciada.");

            async Task<HttpResponseMessage> EnviarAsync(string token)
            {
                using var solicitud = new HttpRequestMessage(
                    HttpMethod.Post,
                    "api/inventario/materiales/imagenes");

                solicitud.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                solicitud.Headers.Add(
                    "X-Equipo",
                    Environment.MachineName);

                using var formulario = new MultipartFormDataContent();

                await using var archivo = File.OpenRead(rutaArchivo);
                using var contenido = new StreamContent(archivo);

                contenido.Headers.ContentType =
                    new MediaTypeHeaderValue(
                        Path.GetExtension(rutaArchivo).ToLowerInvariant() switch
                        {
                            ".jpg" or ".jpeg" => "image/jpeg",
                            ".png" => "image/png",
                            ".webp" => "image/webp",
                            _ => "application/octet-stream"
                        });

                formulario.Add(
                    contenido,
                    "archivo",
                    Path.GetFileName(rutaArchivo));

                solicitud.Content = formulario;

                return await cliente.SendAsync(solicitud, ct);
            }

            using var respuesta = await EnviarAsync(tokenAnterior);

            if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            {
                await bloqueoRenovacion.WaitAsync(ct);

                try
                {
                    if (accessToken == tokenAnterior)
                        await RenovarSesionAsync();
                }
                finally
                {
                    bloqueoRenovacion.Release();
                }

                string tokenNuevo = accessToken
                    ?? throw new InvalidOperationException(
                        "La sesión terminó. Inicia sesión nuevamente.");

                using var reintento = await EnviarAsync(tokenNuevo);

                await VerificarRespuestaAsync(reintento);

                return await LeerReferenciaImagenAsync(reintento, ct);
            }

            await VerificarRespuestaAsync(respuesta);

            return await LeerReferenciaImagenAsync(respuesta, ct);
        }

        /// <summary>
        /// Leers the referencia imagen asynchronous.
        /// </summary>
        /// <param name="respuesta">The respuesta.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">La API no devolvió la referencia de la imagen.</exception>
        private static async Task<string> LeerReferenciaImagenAsync(
            HttpResponseMessage respuesta,
            CancellationToken ct)
        {
            using var documento = await JsonDocument.ParseAsync(
                await respuesta.Content.ReadAsStreamAsync(ct),
                cancellationToken: ct);

            if (documento.RootElement.TryGetProperty(
                "imagen", out var imagen))
            {
                string? referencia = imagen.GetString();

                if (!string.IsNullOrWhiteSpace(referencia))
                    return referencia;
            }

            throw new InvalidOperationException(
                "La API no devolvió la referencia de la imagen.");
        }

        // OBTENER ARCHIVOS DE UNA VISITA
        /// <summary>
        /// Obteners the archivos visita asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="idVisita">The identifier visita.</param>
        /// <returns></returns>
        public static async Task<T> ObtenerArchivosVisitaAsync<T>(
            int idVisita)
        {
            return await ObtenerAutenticadoAsync<T>(
                $"api/visitas/{idVisita}/archivos");
        }

        /// <summary>
        /// Obteners the archivos expediente asynchronous.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="idExpediente">The identifier expediente.</param>
        /// <returns></returns>
        public static async Task<T> ObtenerArchivosExpedienteAsync<T>(
            int idExpediente)
        {
            return await ObtenerAutenticadoAsync<T>(
                $"api/expedientes/{idExpediente}/archivos");
        }

        /// <summary>
        /// Modificars the notas expediente asynchronous.
        /// </summary>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="notas">The notas.</param>
        /// <param name="ct">The ct.</param>
        /// <exception cref="ArgumentOutOfRangeException">idPaciente</exception>
        public static async Task ModificarNotasExpedienteAsync(
            int idPaciente,
            string? notas,
            CancellationToken ct = default)
        {
            if (idPaciente <= 0)
                throw new ArgumentOutOfRangeException(nameof(idPaciente));

            using var respuesta = await PatchAutenticadoAsync(
                $"api/pacientes/{idPaciente}/expediente",
                new { Notas = notas },
                ct);

            await VerificarRespuestaAsync(respuesta);
        }


        /// <summary>
        /// Desactivars the archivo asynchronous.
        /// </summary>
        /// <param name="idArchivo">The identifier archivo.</param>
        public static async Task DesactivarArchivoAsync(Guid idArchivo)
        {
            await PutAutenticadoAsync<object>(
                $"api/archivos/{idArchivo}/estado",
                new
                {
                    Activo = false
                });
        }

        // CERRAR SESIÓN
        /// <summary>
        /// Cerrars the sesion asynchronous.
        /// </summary>
        public static async Task CerrarSesionAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(accessToken))
                    return;

                using var respuesta = await EnviarAutenticadoAsync(
                    HttpMethod.Post,
                    "api/auth/logout");

                await VerificarRespuestaAsync(respuesta);
            }
            finally
            {
                LimpiarSesion();
            }
        }

        // SOLICITAR CÓDIGO PARA RESTABLECER LA CONTRASEÑA
        /// <summary>
        /// Solicitars the codigo asynchronous.
        /// </summary>
        /// <param name="correo">The correo.</param>
        public static async Task SolicitarCodigoAsync(
            string correo)
        {
            using var respuesta = await cliente.PostAsJsonAsync(
                "api/auth/olvide-contrasena",
                new
                {
                    Identificador = correo
                });

            await VerificarRespuestaAsync(respuesta);
        }

        // RESTABLECER CONTRASEÑA
        /// <summary>
        /// Restablecers the password asynchronous.
        /// </summary>
        /// <param name="correo">The correo.</param>
        /// <param name="codigo">The codigo.</param>
        /// <param name="nuevaPassword">The nueva password.</param>
        /// <param name="confirmarPassword">The confirmar password.</param>
        public static async Task RestablecerPasswordAsync(
            string correo,
            string codigo,
            string nuevaPassword,
            string confirmarPassword)
        {
            using var respuesta = await cliente.PostAsJsonAsync(
                "api/auth/restablecer",
                new
                {
                    Identificador = correo,
                    Codigo = codigo,
                    NuevaPassword = nuevaPassword,
                    ConfirmarPassword = confirmarPassword
                });

            await VerificarRespuestaAsync(respuesta);
        }

        // LIMPIAR SESIÓN
        /// <summary>
        /// Limpiars the sesion.
        /// </summary>
        public static void LimpiarSesion()
        {
            accessToken = null;
            refreshToken = null;
        }

        // ENVIAR PETICIÓN AUTENTICADA
        /// <summary>
        /// Enviars the autenticado asynchronous.
        /// </summary>
        /// <param name="metodo">The metodo.</param>
        /// <param name="ruta">The ruta.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// No hay una sesión iniciada.
        /// or
        /// La sesión terminó. Iniciá sesión nuevamente.
        /// </exception>
        private static async Task<HttpResponseMessage> EnviarAutenticadoAsync(
            HttpMethod metodo,
            string ruta)
        {
            string tokenUsado = accessToken
                ?? throw new InvalidOperationException(
                    "No hay una sesión iniciada.");

            var respuesta = await EnviarConTokenAsync(
                metodo,
                ruta,
                tokenUsado);

            if (respuesta.StatusCode != HttpStatusCode.Unauthorized)
                return respuesta;

            respuesta.Dispose();

            await bloqueoRenovacion.WaitAsync();

            try
            {
                // Si otra petición renovó el token, reutilizarlo.
                if (accessToken == tokenUsado)
                    await RenovarSesionAsync();
            }
            finally
            {
                bloqueoRenovacion.Release();
            }

            return await EnviarConTokenAsync(
                metodo,
                ruta,
                accessToken ?? throw new InvalidOperationException(
                    "La sesión terminó. Iniciá sesión nuevamente."));
        }

        // ENVIAR PETICIÓN CON TOKEN
        /// <summary>
        /// Enviars the con token asynchronous.
        /// </summary>
        /// <param name="metodo">The metodo.</param>
        /// <param name="ruta">The ruta.</param>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> EnviarConTokenAsync(
            HttpMethod metodo,
            string ruta,
            string token)
        {
            using var solicitud = new HttpRequestMessage(
                metodo,
                ruta);

            solicitud.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            solicitud.Headers.Add(
                "X-Equipo",
                Environment.MachineName);

            return await cliente.SendAsync(solicitud);
        }

        // RENOVAR SESIÓN
        /// <summary>
        /// Renovars the sesion asynchronous.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// La sesión terminó. Iniciá sesión nuevamente.
        /// or
        /// No se pudo renovar la sesión.
        /// </exception>
        private static async Task RenovarSesionAsync()
        {
            string token = refreshToken
                ?? throw new InvalidOperationException(
                    "La sesión terminó. Iniciá sesión nuevamente.");

            using var respuesta = await cliente.PostAsJsonAsync(
                "api/auth/refresh",
                new { RefreshToken = token });

            if (!respuesta.IsSuccessStatusCode)
            {
                LimpiarSesion();

                throw new InvalidOperationException(
                    "La sesión terminó. Iniciá sesión nuevamente.");
            }

            var resultado =
                await respuesta.Content.ReadFromJsonAsync<RespuestaLogin>(
                    opciones);

            if (string.IsNullOrWhiteSpace(resultado?.AccessToken) ||
                string.IsNullOrWhiteSpace(resultado?.RefreshToken))
            {
                LimpiarSesion();

                throw new InvalidOperationException(
                    "No se pudo renovar la sesión.");
            }

            accessToken = resultado.AccessToken;
            refreshToken = resultado.RefreshToken;
        }

        // VERIFICAR RESPUESTA DEL SERVIDOR
        /// <summary>
        /// Verificars the respuesta asynchronous.
        /// </summary>
        /// <param name="respuesta">The respuesta.</param>
        /// <exception cref="InvalidOperationException"></exception>
        private static async Task VerificarRespuestaAsync(
            HttpResponseMessage respuesta)
        {
            if (respuesta.IsSuccessStatusCode)
                return;

            string mensaje = respuesta.StatusCode switch
            {
                HttpStatusCode.Unauthorized =>
                    "Usuario o contraseña incorrectos, o sesión no válida.",

                HttpStatusCode.Forbidden =>
                    "No tienes permiso para realizar esta acción.",

                _ =>
                    "El servidor no pudo completar la solicitud. Código: " +
                    (int)respuesta.StatusCode
            };

            string contenido =
                await respuesta.Content.ReadAsStringAsync();

            try
            {
                using var documento = JsonDocument.Parse(contenido);

                if (documento.RootElement.TryGetProperty(
                    "detail",
                    out var detalle))
                {
                    string? texto = detalle.GetString();

                    if (!string.IsNullOrWhiteSpace(texto))
                        mensaje = texto;
                }
                else if (documento.RootElement.TryGetProperty(
                    "mensaje",
                    out var mensajeApi))
                {
                    string? texto = mensajeApi.GetString();

                    if (!string.IsNullOrWhiteSpace(texto))
                        mensaje = texto;
                }
            }
            catch (JsonException)
            {
                // Conservar el mensaje general.
            }

            throw new InvalidOperationException(mensaje);
        }


        /// <summary>
        /// Descargars the imagen material asynchronous.
        /// </summary>
        /// <param name="referencia">The referencia.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">
        /// La referencia de la imagen está vacía. - referencia
        /// or
        /// La referencia de la imagen no es válida. - referencia
        /// </exception>
        public static async Task<byte[]> DescargarImagenMaterialAsync(
            string referencia)
        {
            if (string.IsNullOrWhiteSpace(referencia))
            {
                throw new ArgumentException(
                    "La referencia de la imagen está vacía.",
                    nameof(referencia));
            }

            const string prefijo =
                "/api/inventario/materiales/imagenes/";

            if (!referencia.StartsWith(
                prefijo,
                StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "La referencia de la imagen no es válida.",
                    nameof(referencia));
            }

            // Eliminar la barra inicial para utilizar la ruta relativa
            // respecto a la dirección base de la API.
            string ruta = referencia.TrimStart('/');

            using var respuesta = await EnviarAutenticadoAsync(
                HttpMethod.Get,
                ruta);

            await VerificarRespuestaAsync(respuesta);

            return await respuesta.Content.ReadAsByteArrayAsync();
        }

        /// <summary>
        /// Descargars the imagen perfil asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public static async Task<byte[]> DescargarImagenPerfilAsync(int idUsuario, CancellationToken ct = default)
        {
            using var respuesta = await EnviarAutenticadoAsync(
                HttpMethod.Get,
                $"api/usuarios/{idUsuario}/imagen");
            await VerificarRespuestaAsync(respuesta);
            return await respuesta.Content.ReadAsByteArrayAsync(ct);
        }

        /// <summary>
        /// Descargars the archivo asynchronous.
        /// </summary>
        /// <param name="idArchivo">The identifier archivo.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">El identificador del archivo no es válido. - idArchivo</exception>
        public static async Task<byte[]> DescargarArchivoAsync(Guid idArchivo)
        {
            if (idArchivo == Guid.Empty)
                throw new ArgumentException(
                    "El identificador del archivo no es válido.",
                    nameof(idArchivo));

            using var respuesta = await EnviarAutenticadoAsync(
                HttpMethod.Get,
                $"api/archivos/{idArchivo}/contenido");

            await VerificarRespuestaAsync(respuesta);

            return await respuesta.Content.ReadAsByteArrayAsync();
        }


        // SUBIR IMAGEN DEL PERFIL AUTENTICADO
        /// <summary>
        /// Subirs the mi imagen perfil asynchronous.
        /// </summary>
        /// <param name="rutaArchivo">The ruta archivo.</param>
        /// <param name="ct">The ct.</param>
        public static async Task SubirMiImagenPerfilAsync(
            string rutaArchivo,
            CancellationToken ct = default)
        {
            await SubirArchivoAutenticadoAsync(
                "api/auth/me/imagen",
                rutaArchivo,
                "archivo",
                ct);
        }


        // DESCARGAR IMAGEN DEL PERFIL AUTENTICADO
        /// <summary>
        /// Descargars the mi imagen perfil asynchronous.
        /// </summary>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public static async Task<byte[]?>
        DescargarMiImagenPerfilAsync(
            CancellationToken ct = default)
        {
            using var respuesta = await EnviarAutenticadoAsync(
                HttpMethod.Get,
                "api/auth/me/imagen");

            if (respuesta.StatusCode ==
                System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            await VerificarRespuestaAsync(respuesta);

            return await respuesta.Content
                .ReadAsByteArrayAsync(ct);
        }

        // ELIMINAR IMAGEN DEL PERFIL AUTENTICADO
        /// <summary>
        /// Eliminars the mi imagen perfil asynchronous.
        /// </summary>
        public static async Task EliminarMiImagenPerfilAsync()
        {
            using var respuesta = await EnviarAutenticadoAsync(
                HttpMethod.Delete,
                "api/auth/me/imagen");

            await VerificarRespuestaAsync(respuesta);
        }

    }
}
