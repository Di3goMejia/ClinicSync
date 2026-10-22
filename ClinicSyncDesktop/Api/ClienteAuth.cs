using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace ClinicSyncDesktop.Api
{
    public class ClienteAuth(ClienteApi api, TokenStore tokens)
    {
        public async Task<LoginResponse> LoginAsync(string identificador, string password)
        {
            var r = await api.PostAsync<LoginResponse>("api/auth/login",
                new { Identificador = identificador, Password = password }, 
                autenticado: false)
                ?? throw new ApiException("Respuesta vacía del servidor.", HttpStatusCode.InternalServerError);

            if (!r.Requiere2fa && r.AccessToken is not null && r.RefreshToken is not null)
                tokens.Guardar(r.AccessToken, r.RefreshToken);

            return r;
        }

        public async Task<LoginResponse?> VerificarSegundoFactorAsync(string desafio, string prueba)
        {
            var r = await api.PostAsync<LoginResponse>("api/auth/2fa",
                new { desafio, prueba }, autenticado: false);

            if (r?.AccessToken is not null && r.RefreshToken is not null)
                tokens.Guardar(r.AccessToken, r.RefreshToken);

            return r;
        }

        public async Task CerrarSesionAsync()
        {
            try
            {
                await api.PostAsync<object>("api/auth/logout", null);
            }
            finally
            {
                tokens.Limpiar();
            }
        }
    }
}
