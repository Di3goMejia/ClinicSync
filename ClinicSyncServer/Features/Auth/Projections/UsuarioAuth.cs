namespace ClinicSyncServer.Features.Auth.Projections
{
    public class UsuarioAuth
    {
        public int IdUsuario { get; set; }
        public int IdRol { get; set; }
        public string ClaveHash { get; set; } = string.Empty;
        public string Estado { get; set; } = "PENDIENTE";
        public bool Requiere2FA { get; set; }
    }
}
