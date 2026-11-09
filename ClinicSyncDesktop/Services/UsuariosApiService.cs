using ClinicSyncDesktop.Data;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

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
        public DateTime? UltimoAcceso { get; set; }
        public DateTime? UltimoLogin { get => UltimoAcceso; set => UltimoAcceso = value; }
        public string NombreRol { get; set; } = string.Empty;
        public string Estado { get; set; } = "PENDIENTE";
    }

    public class UsuariosApiService
    {
        public async Task<byte[]> DescargarImagenPerfilAsync(int idUsuario, CancellationToken ct = default)
        {
            return await ClienteApi.DescargarImagenPerfilAsync(idUsuario, ct);
        }

        public async Task<UsuarioDetalle> ObtenerUsuarioAsync(int idUsuario)
        {
            return await ClienteApi.ObtenerAutenticadoAsync<UsuarioDetalle>(
                $"api/usuarios/{idUsuario}");
        }

        public async Task<IEnumerable<UsuarioListadoResponse>> ListarAsync()
        {
            return await ClienteApi.ObtenerAutenticadoAsync<IEnumerable<UsuarioListadoResponse>>("api/usuarios");
        }

        public async Task<CrearUsuarioResponse> CrearAsync(
            string identidad,
            string nombres,
            string apellidos,
            DateOnly fechaNacimiento,
            char genero,
            string telefono,
            int idRol,
            string nombreUsuario,
            string correoLogin,
            CancellationToken ct = default)
        {
            using var respuesta = await ClienteApi.PostAutenticadoAsync(
                "api/usuarios/",
                new
                {
                    identidad,
                    nombres,
                    apellidos,
                    fechaNacimiento,
                    genero,
                    telefono,
                    idRol,
                    nombreUsuario,
                    correoLogin,
                    correoContacto = (string?)null
                },
                ct);

            if (!respuesta.IsSuccessStatusCode)
            {
                var mensaje = "No fue posible crear el usuario.";
                var contenido = await respuesta.Content.ReadAsStringAsync(ct);

                try
                {
                    using var documento = JsonDocument.Parse(contenido);
                    if (documento.RootElement.TryGetProperty("detail", out var detalle))
                        mensaje = detalle.GetString() ?? mensaje;
                }
                catch (JsonException)
                {
                }

                throw new HttpRequestException(mensaje, null, respuesta.StatusCode);
            }

            return await respuesta.Content.ReadFromJsonAsync<CrearUsuarioResponse>(cancellationToken: ct)
                ?? throw new HttpRequestException("La API no devolvió los datos del usuario creado.");
        }

        public async Task SubirImagenPerfilAsync(int idUsuario, string rutaArchivo, CancellationToken ct = default)
        {
            await ClienteApi.SubirArchivoAutenticadoAsync(
                $"api/usuarios/{idUsuario}/imagen",
                rutaArchivo,
                "archivo",
                ct);
        }

        public async Task ModificarAsync(int idUsuario, object solicitud, CancellationToken ct = default)
        {
            using var respuesta = await ClienteApi.PutAutenticadoAsync($"api/usuarios/{idUsuario}", solicitud, ct);
            if (!respuesta.IsSuccessStatusCode)
                throw new HttpRequestException("No fue posible actualizar el usuario.");
        }

        public async Task ModificarRolAsync(int idUsuario, int idRol, CancellationToken ct = default)
        {
            using var respuesta = await ClienteApi.PatchAutenticadoAsync($"api/usuarios/{idUsuario}/rol", new { idRol }, ct);
            if (!respuesta.IsSuccessStatusCode)
                throw new HttpRequestException("No fue posible actualizar el rol del usuario.");
        }

        public async Task ReenviarActivacionAsync(int idUsuario, CancellationToken ct = default)
        {
            using var respuesta = await ClienteApi.PostAutenticadoAsync($"api/usuarios/{idUsuario}/reenviar-activacion", new { }, ct);
            if (!respuesta.IsSuccessStatusCode)
                throw new HttpRequestException("No fue posible reenviar la activación.");
        }

        public async Task DesactivarAsync(int idUsuario, string password, CancellationToken ct = default)
        {
            await EjecutarAccionAsync($"api/usuarios/{idUsuario}/desactivar", password, "No fue posible desactivar el usuario.", ct);
        }

        public async Task ReactivarAsync(int idUsuario, string password, CancellationToken ct = default)
        {
            await EjecutarAccionAsync($"api/usuarios/{idUsuario}/reactivar", password, "No fue posible reactivar el usuario.", ct);
        }

        public async Task RestablecerPasswordAsync(int idUsuario, string password, CancellationToken ct = default)
        {
            await EjecutarAccionAsync($"api/usuarios/{idUsuario}/restablecer-password", password, "No fue posible solicitar el restablecimiento de contraseña.", ct);
        }

        private static async Task EjecutarAccionAsync(string ruta, string password, string mensaje, CancellationToken ct)
        {
            using var respuesta = await ClienteApi.PostAutenticadoAsync(ruta, new { password }, ct);
            if (!respuesta.IsSuccessStatusCode)
            {
                var detalle = await respuesta.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException(string.IsNullOrWhiteSpace(detalle) ? mensaje : detalle);
            }
        }
    }

    public sealed class CrearUsuarioResponse
    {
        public int IdUsuario { get; set; }
        public int IdEmpleado { get; set; }
    }
}

