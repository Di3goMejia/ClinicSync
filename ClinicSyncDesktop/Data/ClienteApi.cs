using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

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
    }

    public static class ClienteApi
    {
        private static readonly HttpClient cliente = new()
        {
            BaseAddress = new Uri("http://localhost:5227/"),
            Timeout = TimeSpan.FromSeconds(60)
        };

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
                HttpMethod.Post,
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


        public static async Task<HttpResponseMessage> PutAutenticadoAsync<T>(
            string ruta,
            T contenido)
        {

            using var respuestaPerfil = await EnviarAutenticadoAsync(
                HttpMethod.Get,
                "api/auth/me");

            await VerificarRespuestaAsync(respuestaPerfil);

            string token = accessToken
                ?? throw new InvalidOperationException(
                    "No hay una sesión iniciada.");

            using var solicitud = new HttpRequestMessage(
                HttpMethod.Put,
                ruta);

            solicitud.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            solicitud.Headers.Add(
                "X-Equipo",
                Environment.MachineName);

            solicitud.Content = JsonContent.Create(
                contenido,
                options: opciones);

            var respuesta = await cliente.SendAsync(solicitud);

            if (respuesta.StatusCode != HttpStatusCode.Unauthorized)
                return respuesta;

            respuesta.Dispose();

            await bloqueoRenovacion.WaitAsync();

            try
            {
                if (accessToken == token)
                    await RenovarSesionAsync();
            }
            finally
            {
                bloqueoRenovacion.Release();
            }

            using var reintento = new HttpRequestMessage(
                HttpMethod.Put,
                ruta);

            reintento.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    accessToken ?? throw new InvalidOperationException(
                        "La sesión terminó. Iniciá sesión nuevamente."));

            reintento.Headers.Add(
                "X-Equipo",
                Environment.MachineName);

            reintento.Content = JsonContent.Create(
                contenido,
                options: opciones);

            return await cliente.SendAsync(reintento);
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
    }
}
