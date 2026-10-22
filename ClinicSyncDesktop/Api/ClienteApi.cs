using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace ClinicSyncDesktop.Api
{
    public class ClienteApi
    {
        private readonly HttpClient _http;
        private readonly TokenStore _tokens;
        private readonly SemaphoreSlim _renovacion = new(1, 1);

        public ClienteApi(string baseUrl, TokenStore tokens)
        {
            _tokens = tokens;
            _http = new HttpClient
            {
                BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(15)
            };
            _http.DefaultRequestHeaders.Add("X-Equipo", Environment.MachineName);
        }

        public Task<T?> GetAsync<T>(string ruta, CancellationToken ct = default) =>
            EnviarYLeerAsync<T>(() => new HttpRequestMessage(HttpMethod.Get, ruta), autenticado: true, ct);

        public Task<T?> PostAsync<T>(string ruta, object? cuerpo, bool autenticado = true,
            CancellationToken ct = default) =>
            EnviarYLeerAsync<T>(() => new HttpRequestMessage(HttpMethod.Post, ruta)
            {
                Content = cuerpo is null ? null : JsonContent.Create(cuerpo)
            }, autenticado, ct);

        private async Task<T?> EnviarYLeerAsync<T>(Func<HttpRequestMessage> crear,
            bool autenticado, CancellationToken ct)
        {
            using var resp = await EnviarAsync(crear, autenticado, ct);
            return await LeerAsync<T>(resp, ct);
        }

        private async Task<HttpResponseMessage> EnviarAsync(Func<HttpRequestMessage> crear,
            bool autenticado, CancellationToken ct)
        {
            var tokenUsado = _tokens.AccessToken;
            var resp = await _http.SendAsync(Construir(crear, autenticado), ct);

            if (!autenticado || resp.StatusCode != HttpStatusCode.Unauthorized)
                return resp;

            resp.Dispose();

            if (_tokens.RefreshToken is null || !await RenovarAsync(tokenUsado, ct))
                throw new SesionExpiradaException();

            var reintento = await _http.SendAsync(Construir(crear, autenticado), ct);
            if (reintento.StatusCode == HttpStatusCode.Unauthorized)
            {
                reintento.Dispose();
                throw new SesionExpiradaException();
            }

            return await _http.SendAsync(Construir(crear, autenticado), ct);
        }

        private HttpRequestMessage Construir(Func<HttpRequestMessage> crear, bool autenticado)
        {
            var req = crear();
            if (autenticado && _tokens.AccessToken is { } token)
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return req;
        }

        private async Task<bool> RenovarAsync(string? tokenUsado, CancellationToken ct)
        {
            await _renovacion.WaitAsync(ct);
            try
            {
                if (_tokens.AccessToken != tokenUsado)
                    return _tokens.AccessToken is not null;

                using var resp = await _http.PostAsJsonAsync("api/auth/refresh",
                    new { refreshToken = _tokens.RefreshToken }, ct);

                if (!resp.IsSuccessStatusCode)
                {
                    _tokens.Limpiar();
                    return false;
                }

                var datos = await resp.Content.ReadFromJsonAsync<LoginResponse>(ct);
                _tokens.Guardar(datos!.AccessToken!, datos.RefreshToken!);
                return true;
            }
            finally
            {
                _renovacion.Release();
            }
        }

        private static async Task<T?> LeerAsync<T>(HttpResponseMessage resp, CancellationToken ct)
        {
            if (resp.IsSuccessStatusCode)
            {
                if (resp.StatusCode == HttpStatusCode.NoContent) return default;
                return await resp.Content.ReadFromJsonAsync<T>(ct);
            }

            string mensaje = "Ocurrió un error en el servidor. Intenta más tarde.";
            try
            {
                var problema = await resp.Content.ReadFromJsonAsync<ProblemaApi>(ct);
                if (problema?.Errors is { Count: > 0 } errores)
                    mensaje = string.Join(Environment.NewLine, errores.Values.SelectMany(v => v));
                else if (!string.IsNullOrWhiteSpace(problema?.Detail))
                    mensaje = problema.Detail;
            }
            catch (Exception) { }

            throw new ApiException(mensaje, resp.StatusCode);
        }
    }
}
