
namespace ClinicSyncServer.Common.Seguridad
{
    /// <summary>
    /// 
    /// </summary>
    public static class Permisos
    {
        // INVENTARIO
        /// <summary>
        /// The inventario crear
        /// </summary>
        public const string InventarioCrear = "INVENTARIO_CREAR";
        /// <summary>
        /// The inventario leer
        /// </summary>
        public const string InventarioLeer = "INVENTARIO_LEER";
        /// <summary>
        /// The inventario modificar
        /// </summary>
        public const string InventarioModificar = "INVENTARIO_MODIFICAR";
        /// <summary>
        /// The inventario estado
        /// </summary>
        public const string InventarioEstado = "INVENTARIO_ESTADO";
        /// <summary>
        /// The inventario consultar movimientos
        /// </summary>
        public const string InventarioConsultarMovimientos = "INVENTARIO_CONSULTAR_MOVIMIENTOS";
        /// <summary>
        /// The inventario registrar movimientos
        /// </summary>
        public const string InventarioRegistrarMovimientos = "INVENTARIO_REGISTRAR_MOVIMIENTOS";

        // USUARIOS
        /// <summary>
        /// The usuarios crear
        /// </summary>
        public const string UsuariosCrear = "USUARIOS_CREAR";
        /// <summary>
        /// The usuarios leer
        /// </summary>
        public const string UsuariosLeer = "USUARIOS_LEER";
        /// <summary>
        /// The usuarios modificar
        /// </summary>
        public const string UsuariosModificar = "USUARIOS_MODIFICAR";
        /// <summary>
        /// The usuarios estado
        /// </summary>
        public const string UsuariosEstado = "USUARIOS_ESTADO";
        /// <summary>
        /// The usuarios restablecer clave
        /// </summary>
        public const string UsuariosRestablecerClave = "USUARIOS_RESTABLECER_CLAVE";
        /// <summary>
        /// The usuarios cerrar sesiones
        /// </summary>
        public const string UsuariosCerrarSesiones = "USUARIOS_CERRAR_SESIONES";

        // ROLES
        /// <summary>
        /// The roles crear
        /// </summary>
        public const string RolesCrear = "ROLES_CREAR";
        /// <summary>
        /// The roles leer
        /// </summary>
        public const string RolesLeer = "ROLES_LEER";
        /// <summary>
        /// The roles modificar
        /// </summary>
        public const string RolesModificar = "ROLES_MODIFICAR";
        /// <summary>
        /// The roles estado
        /// </summary>
        public const string RolesEstado = "ROLES_ESTADO";
        /// <summary>
        /// The roles modificar permisos
        /// </summary>
        public const string RolesModificarPermisos = "ROLES_MODIFICAR_PERMISOS";

        // PACIENTES
        /// <summary>
        /// The pacientes crear
        /// </summary>
        public const string PacientesCrear = "PACIENTES_CREAR";
        /// <summary>
        /// The pacientes leer
        /// </summary>
        public const string PacientesLeer = "PACIENTES_LEER";
        /// <summary>
        /// The pacientes modificar
        /// </summary>
        public const string PacientesModificar = "PACIENTES_MODIFICAR";
        /// <summary>
        /// The pacientes estado
        /// </summary>
        public const string PacientesEstado = "PACIENTES_ESTADO";

        // EXPEDIENTES
        /// <summary>
        /// The expedientes leer
        /// </summary>
        public const string ExpedientesLeer = "EXPEDIENTES_LEER";
        /// <summary>
        /// The expedientes modificar
        /// </summary>
        public const string ExpedientesModificar = "EXPEDIENTES_MODIFICAR";
        /// <summary>
        /// The expedientes visitas crear
        /// </summary>
        public const string ExpedientesVisitasCrear = "EXPEDIENTES_VISITAS_CREAR";
        /// <summary>
        /// The expedientes visitas modificar
        /// </summary>
        public const string ExpedientesVisitasModificar = "EXPEDIENTES_VISITAS_MODIFICAR";
        /// <summary>
        /// The expedientes tratamientos modificar
        /// </summary>
        public const string ExpedientesTratamientosModificar = "EXPEDIENTES_TRATAMIENTOS_MODIFICAR";

        // ARCHIVOS
        /// <summary>
        /// The archivos leer
        /// </summary>
        public const string ArchivosLeer = "ARCHIVOS_LEER";
        /// <summary>
        /// The archivos subir
        /// </summary>
        public const string ArchivosSubir = "ARCHIVOS_SUBIR";
        /// <summary>
        /// The archivos desactivar
        /// </summary>
        public const string ArchivosDesactivar = "ARCHIVOS_DESACTIVAR";

        // TRATAMIENTOS
        /// <summary>
        /// The tratamientos crear
        /// </summary>
        public const string TratamientosCrear = "TRATAMIENTOS_CREAR";
        /// <summary>
        /// The tratamientos leer
        /// </summary>
        public const string TratamientosLeer = "TRATAMIENTOS_LEER";
        /// <summary>
        /// The tratamientos modificar
        /// </summary>
        public const string TratamientosModificar = "TRATAMIENTOS_MODIFICAR";
        /// <summary>
        /// The tratamientos estado
        /// </summary>
        public const string TratamientosEstado = "TRATAMIENTOS_ESTADO";

        // INGRESOS
        /// <summary>
        /// The ingresos crear
        /// </summary>
        public const string IngresosCrear = "INGRESOS_CREAR";
        /// <summary>
        /// The ingresos leer
        /// </summary>
        public const string IngresosLeer = "INGRESOS_LEER";
        /// <summary>
        /// The ingresos modificar
        /// </summary>
        public const string IngresosModificar = "INGRESOS_MODIFICAR";

        // EGRESOS
        /// <summary>
        /// The egresos crear
        /// </summary>
        public const string EgresosCrear = "EGRESOS_CREAR";
        /// <summary>
        /// The egresos leer
        /// </summary>
        public const string EgresosLeer = "EGRESOS_LEER";
        /// <summary>
        /// The egresos modificar
        /// </summary>
        public const string EgresosModificar = "EGRESOS_MODIFICAR";

        // AGENDA
        /// <summary>
        /// The agenda crear
        /// </summary>
        public const string AgendaCrear = "AGENDA_CREAR";
        /// <summary>
        /// The agenda leer
        /// </summary>
        public const string AgendaLeer = "AGENDA_LEER";
        /// <summary>
        /// The agenda modificar
        /// </summary>
        public const string AgendaModificar = "AGENDA_MODIFICAR";

        // BITACORA
        /// <summary>
        /// The bitacora leer
        /// </summary>
        public const string BitacoraLeer = "BITACORA_LEER";

        /// <summary>
        /// The todos
        /// </summary>
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

    /// <summary>
    /// 
    /// </summary>
    public static class ClaimsPersonalizados
    {
        /// <summary>
        /// The rol
        /// </summary>
        public const string Rol = "rol";
        /// <summary>
        /// The permiso
        /// </summary>
        public const string Permiso = "permiso";
        /// <summary>
        /// The proposito
        /// </summary>
        public const string Proposito = "proposito";
    }
}
