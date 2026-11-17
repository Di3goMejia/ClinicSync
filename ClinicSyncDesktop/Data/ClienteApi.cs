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
    public sealed class RespuestaLogin
    {
        public bool Requiere2fa { get; set; }
        public string? Desafio { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }


    public sealed class PerfilActual
    {
        public string NombreUsuario { get; set; } = "";
        public string Rol { get; set; } = "";
        public bool TieneFinanzas { get; set; }
        public int IdEmpleado { get; set; }
        public string NombreEmpleado { get; set; } = "";

        public string Identidad { get; set; } = "";
        public string Nombres { get; set; } = "";
        public string Apellidos { get; set; } = "";
        public DateTime FechaNacimiento { get; set; }
        public string Genero { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string? CorreoContacto { get; set; }
        public string? ImagenPerfil { get; set; }

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

    public sealed class Estado2FA
    {
        public bool Configurado { get; set; }
    }

    public static class ClienteApi
    {
        private static readonly HttpClient cliente = new()
        {
            BaseAddress = new Uri("http://localhost:5227/"),
            Timeout = TimeSpan.FromSeconds(60)
        };

        // CAMBIAR CONTRASEÑA DEL USUARIO AUTENTICADO
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

        private static readonly SemaphoreSlim bloqueoRenovacion =
            new(1, 1);

        private static readonly JsonSerializerOptions opciones =
            new(JsonSerializerDefaults.Web);

        private static string? accessToken;
        private static string? refreshToken;

        // INICIAR SESIÓN
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
        public static async Task<Estado2FA> ObtenerEstado2FAAsync()
        {
            return await ObtenerAutenticadoAsync<Estado2FA>(
                "api/auth/2fa/estado");
        }

        // DESACTIVAR AUTENTICACIÓN DE DOS FACTORES
        public static async Task Desactivar2FAAsync()
        {
            using var respuesta = await DeleteAutenticadoAsync(
                "api/auth/2fa/facial");

            await VerificarRespuestaAsync(respuesta);
        }

        // OBTENER DATOS DESDE LA API
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
        public static async Task<HttpResponseMessage> PostAutenticadoAsync<T>(
            string ruta,
            T contenido,
            CancellationToken ct = default)
        {
            return await EnviarJsonAutenticadoAsync<T>(HttpMethod.Post, ruta, contenido, ct);
        }

        public static async Task<HttpResponseMessage> PutAutenticadoAsync<T>(
            string ruta,
            T contenido,
            CancellationToken ct = default)
        {
            return await EnviarJsonAutenticadoAsync<T>(HttpMethod.Put, ruta, contenido, ct);
        }

        public static async Task<HttpResponseMessage> PatchAutenticadoAsync<T>(
            string ruta,
            T contenido,
            CancellationToken ct = default)
        {
            return await EnviarJsonAutenticadoAsync<T>(HttpMethod.Patch, ruta, contenido, ct);
        }

        // ENVIAR SOLICITUD MEDIANTE DELETE
        public static async Task<HttpResponseMessage> DeleteAutenticadoAsync(
            string ruta)
        {
            return await EnviarAutenticadoAsync(
                HttpMethod.Delete,
                ruta);
        }

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
        public static async Task ValidarTokenActivacionAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException("El código o token de activación es obligatorio.");

            using var respuesta = await cliente.PostAsJsonAsync(
                "api/auth/validar-token",
                new { Token = token.Trim() });

            await VerificarRespuestaAsync(respuesta);
        }

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
        public static async Task ModificarVisitaCompletaAsync<T>(int idVisita, T request)
        {
            if (idVisita <= 0)
                throw new ArgumentOutOfRangeException(nameof(idVisita));

            using var respuesta = await PutAutenticadoAsync(
                $"api/visitas/{idVisita}/completa", request);

            await VerificarRespuestaAsync(respuesta);
        }

        private sealed class VisitaCreada
        {
            public int IdVisita { get; set; }
        }


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
        public static async Task<T> ObtenerArchivosVisitaAsync<T>(
            int idVisita)
        {
            return await ObtenerAutenticadoAsync<T>(
                $"api/visitas/{idVisita}/archivos");
        }

        public static async Task<T> ObtenerArchivosExpedienteAsync<T>(
            int idExpediente)
        {
            return await ObtenerAutenticadoAsync<T>(
                $"api/expedientes/{idExpediente}/archivos");
        }

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
        public static void LimpiarSesion()
        {
            accessToken = null;
            refreshToken = null;
        }

        // ENVIAR PETICIÓN AUTENTICADA
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

        public static async Task<byte[]> DescargarImagenPerfilAsync(int idUsuario, CancellationToken ct = default)
        {
            using var respuesta = await EnviarAutenticadoAsync(
                HttpMethod.Get,
                $"api/usuarios/{idUsuario}/imagen");
            await VerificarRespuestaAsync(respuesta);
            return await respuesta.Content.ReadAsByteArrayAsync(ct);
        }

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
        public static async Task EliminarMiImagenPerfilAsync()
        {
            using var respuesta = await EnviarAutenticadoAsync(
                HttpMethod.Delete,
                "api/auth/me/imagen");

            await VerificarRespuestaAsync(respuesta);
        }

    }
}
