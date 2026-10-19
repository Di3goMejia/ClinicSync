namespace ClinicSyncServer.Common.Seguridad
{
    public static class Permisos
    {
        public const string UsuariosLeer = "USUARIOS_LEER";
        public const string UsuariosCrear = "USUARIOS_CREAR";

        public static readonly IReadOnlyList<string> Todos =
        [
            UsuariosLeer, UsuariosCrear
        ]; 
    }

    public static class ClaimsPersonalizados
    {
        public const string Rol = "rol";
        public const string Permiso = "permiso";
        public const string Proposito = "proposito";
    }
}
