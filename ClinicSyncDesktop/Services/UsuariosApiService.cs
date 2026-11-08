using ClinicSyncDesktop.Data;

namespace ClinicSyncDesktop.Services
{
    /// <summary>
    /// UsuarioDetalle es una proyección que contiene los datos del usuario y del empleado
    /// como aparece en la configuración por cada usuario.
    /// </summary>
    public class UsuarioDetalle
    {
        public int IdUsuario { get; set; }
        public int IdEmpleado { get; set; }
        public string Identidad { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public DateOnly FechaNacimiento { get; set; }
        public char Genero { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string? CorreoContacto { get; set; }
        public string? ImagenPerfil { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public string CorreoLogin { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public string Estado { get; set; } = "PENDIENTE";
        public DateTime FechaRegistro { get; set; }
    }
    
    public class UsuarioListadoResponse
    {
        public int IdUsuario { get; set; }
        public int IdEmpleado { get; set; }
        public int IdRol { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string? ImagenPerfil { get; set; }
        public DateTime UltimoAcceso { get; set; }
        public string Estado { get; set; } = "PENDIENTE";
    }

    public class UsuariosApiService
    {
        public async Task<UsuarioDetalle> ObtenerUsuarioAsync(int idUsuario)
        {
            return await ClienteApi.ObtenerAutenticadoAsync<UsuarioDetalle>(
                $"api/usuarios/{idUsuario}");
        }

        public async Task<IEnumerable<UsuarioListadoResponse>> ListarAsync()
        {
            return await ClienteApi.ObtenerAutenticadoAsync<IEnumerable<UsuarioListadoResponse>>("api/usuarios");
        }
    }
}

