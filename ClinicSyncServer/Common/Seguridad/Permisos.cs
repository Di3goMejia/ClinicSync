
namespace ClinicSyncServer.Common.Seguridad
{
    public static class Permisos
    {
        // INVENTARIO
        public const string InventarioCrear = "INVENTARIO_CREAR";
        public const string InventarioLeer = "INVENTARIO_LEER";
        public const string InventarioModificar = "INVENTARIO_MODIFICAR";
        public const string InventarioEstado = "INVENTARIO_ESTADO";
        public const string InventarioConsultarMovimientos = "INVENTARIO_CONSULTAR_MOVIMIENTOS";
        public const string InventarioRegistrarMovimientos = "INVENTARIO_REGISTRAR_MOVIMIENTOS";

        // USUARIOS
        public const string UsuariosCrear = "USUARIOS_CREAR";
        public const string UsuariosLeer = "USUARIOS_LEER";
        public const string UsuariosModificar = "USUARIOS_MODIFICAR";
        public const string UsuariosEstado = "USUARIOS_ESTADO";
        public const string UsuariosRestablecerClave = "USUARIOS_RESTABLECER_CLAVE";
        public const string UsuariosCerrarSesiones = "USUARIOS_CERRAR_SESIONES";

        // ROLES
        public const string RolesCrear = "ROLES_CREAR";
        public const string RolesLeer = "ROLES_LEER";
        public const string RolesModificar = "ROLES_MODIFICAR";
        public const string RolesEstado = "ROLES_ESTADO";
        public const string RolesModificarPermisos = "ROLES_MODIFICAR_PERMISOS";

        // PACIENTES
        public const string PacientesCrear = "PACIENTES_CREAR";
        public const string PacientesLeer = "PACIENTES_LEER";
        public const string PacientesModificar = "PACIENTES_MODIFICAR";
        public const string PacientesEstado = "PACIENTES_ESTADO";

        // EXPEDIENTES
        public const string ExpedientesLeer = "EXPEDIENTES_LEER";
        public const string ExpedientesModificar = "EXPEDIENTES_MODIFICAR";
        public const string ExpedientesVisitasCrear = "EXPEDIENTES_VISITAS_CREAR";
        public const string ExpedientesVisitasModificar = "EXPEDIENTES_VISITAS_MODIFICAR";
        public const string ExpedientesTratamientosModificar = "EXPEDIENTES_TRATAMIENTOS_MODIFICAR";

        // ARCHIVOS
        public const string ArchivosLeer = "ARCHIVOS_LEER";
        public const string ArchivosSubir = "ARCHIVOS_SUBIR";
        public const string ArchivosDesactivar = "ARCHIVOS_DESACTIVAR";

        // TRATAMIENTOS
        public const string TratamientosCrear = "TRATAMIENTOS_CREAR";
        public const string TratamientosLeer = "TRATAMIENTOS_LEER";
        public const string TratamientosModificar = "TRATAMIENTOS_MODIFICAR";
        public const string TratamientosEstado = "TRATAMIENTOS_ESTADO";

        // INGRESOS
        public const string IngresosCrear = "INGRESOS_CREAR";
        public const string IngresosLeer = "INGRESOS_LEER";
        public const string IngresosModificar = "INGRESOS_MODIFICAR";

        // EGRESOS
        public const string EgresosCrear = "EGRESOS_CREAR";
        public const string EgresosLeer = "EGRESOS_LEER";
        public const string EgresosModificar = "EGRESOS_MODIFICAR";

        // AGENDA
        public const string AgendaCrear = "AGENDA_CREAR";
        public const string AgendaLeer = "AGENDA_LEER";
        public const string AgendaModificar = "AGENDA_MODIFICAR";

        // BITACORA
        public const string BitacoraLeer = "BITACORA_LEER";

        public static readonly IReadOnlyList<string> Todos =
        [
            // INVENTARIO
            InventarioCrear,
            InventarioLeer,
            InventarioModificar,
            InventarioEstado,
            InventarioConsultarMovimientos,
            InventarioRegistrarMovimientos,

            // USUARIOS
            UsuariosCrear,
            UsuariosLeer,
            UsuariosModificar,
            UsuariosEstado,
            UsuariosRestablecerClave,
            UsuariosCerrarSesiones,

            // ROLES
            RolesCrear,
            RolesLeer,
            RolesModificar,
            RolesEstado,
            RolesModificarPermisos,

            // PACIENTES
            PacientesCrear,
            PacientesLeer,
            PacientesModificar,
            PacientesEstado,

            // EXPEDIENTES
            ExpedientesLeer,
            ExpedientesModificar,
            ExpedientesVisitasCrear,
            ExpedientesVisitasModificar,
            ExpedientesTratamientosModificar,

            // ARCHIVOS
            ArchivosLeer,
            ArchivosSubir,
            ArchivosDesactivar,

            // TRATAMIENTOS
            TratamientosCrear,
            TratamientosLeer,
            TratamientosModificar,
            TratamientosEstado,

            // INGRESOS
            IngresosCrear,
            IngresosLeer,
            IngresosModificar,

            // EGRESOS
            EgresosCrear,
            EgresosLeer,
            EgresosModificar,

            // AGENDA
            AgendaCrear,
            AgendaLeer,
            AgendaModificar,

            // BITACORA
            BitacoraLeer
        ];
    } 

    public static class ClaimsPersonalizados
    {
        public const string Rol = "rol";
        public const string Permiso = "permiso";
        public const string Proposito = "proposito";
    }
}
