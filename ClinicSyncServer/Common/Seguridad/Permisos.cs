namespace ClinicSyncServer.Common.Seguridad
{
    public static class Permisos
    {
        public const string UsuariosLeer = "USUARIOS_LEER";
        public const string UsuariosCrear = "USUARIOS_CREAR";
        public const string TratamientosCrear = "TRATAMIENTOS_CREAR";
        public const string TratamientosLeer = "TRATAMIENTOS_LEER";
        public const string TratamientosModificar = "TRATAMIENTOS_MODIFICAR";
        public const string TratamientosEstado = "TRATAMIENTOS_ESTADO";

        public static readonly IReadOnlyList<string> Todos =
        [
            // Usuarios
            UsuariosLeer, UsuariosCrear,
            // Tratamientos
            TratamientosLeer, TratamientosCrear, TratamientosModificar, TratamientosEstado
        ];
    }

    public static class ClaimsPersonalizados
    {
        public const string Rol = "rol";
        public const string Permiso = "permiso";
        public const string Proposito = "proposito";
    }
}
