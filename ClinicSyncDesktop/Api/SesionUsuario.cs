using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace ClinicSyncDesktop.Api
{
    public static class SesionUsuario
    {
        public static bool Tiene(TokenStore store, string permiso)
        {
            if (store.AccessToken is null) 
                return false;

            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(store.AccessToken);
            return jwt.Claims.Any(c => c.Type == "permiso" && c.Value == permiso);
        }
    }
}
