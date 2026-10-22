
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ClinicSyncDesktop.Api
{
    public class TokenStore
    {
        private static readonly string Ruta = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ClinicSync", "sesion.bin"
        );

        public string? AccessToken { get; private set; }
        public string? RefreshToken { get; private set; }

        public void Guardar(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;

            Directory.CreateDirectory(Path.GetDirectoryName(Ruta)!);
            var cifrado = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(refreshToken), null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(Ruta, cifrado);
        }

        public void CargarRefresh()
        {
            if (!File.Exists(Ruta)) return;

            RefreshToken = Encoding.UTF8.GetString(ProtectedData.Unprotect(
                File.ReadAllBytes(Ruta), null, DataProtectionScope.CurrentUser));
        }

        public void Limpiar()
        {
            AccessToken = null;
            RefreshToken = null;
            if (File.Exists(Ruta)) 
                File.Delete(Ruta);
        }
    }
}
