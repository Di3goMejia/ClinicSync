using ClinicSyncDesktop.Data;
using System.Diagnostics;
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
        /// <summary>
        /// Gets or sets the identifier usuario.
        /// </summary>
        /// <value>
        /// The identifier usuario.
        /// </value>
        public int IdUsuario { get; set; }
        /// <summary>
        /// Gets or sets the identifier empleado.
        /// </summary>
        /// <value>
        /// The identifier empleado.
        /// </value>
        public int IdEmpleado { get; set; }
        /// <summary>
        /// Gets or sets the identidad.
        /// </summary>
        /// <value>
        /// The identidad.
        /// </value>
        public string Identidad { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the nombres.
        /// </summary>
        /// <value>
        /// The nombres.
        /// </value>
        public string Nombres { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the apellidos.
        /// </summary>
        /// <value>
        /// The apellidos.
        /// </value>
        public string Apellidos { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the fecha nacimiento.
        /// </summary>
        /// <value>
        /// The fecha nacimiento.
        /// </value>
        public DateOnly FechaNacimiento { get; set; }
        /// <summary>
        /// Gets or sets the genero.
        /// </summary>
        /// <value>
        /// The genero.
        /// </value>
        public char Genero { get; set; }
        /// <summary>
        /// Gets or sets the telefono.
        /// </summary>
        /// <value>
        /// The telefono.
        /// </value>
        public string Telefono { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the correo contacto.
        /// </summary>
        /// <value>
        /// The correo contacto.
        /// </value>
        public string? CorreoContacto { get; set; }
        /// <summary>
        /// Gets or sets the imagen perfil.
        /// </summary>
        /// <value>
        /// The imagen perfil.
        /// </value>
        public string? ImagenPerfil { get; set; }
        /// <summary>
        /// Gets or sets the nombre usuario.
        /// </summary>
        /// <value>
        /// The nombre usuario.
        /// </value>
        public string NombreUsuario { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the correo login.
        /// </summary>
        /// <value>
        /// The correo login.
        /// </value>
        public string CorreoLogin { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the identifier rol.
        /// </summary>
        /// <value>
        /// The identifier rol.
        /// </value>
        public int IdRol { get; set; }
        /// <summary>
        /// Gets or sets the estado.
        /// </summary>
        /// <value>
        /// The estado.
        /// </value>
        public string Estado { get; set; } = "PENDIENTE";
        /// <summary>
        /// Gets or sets the fecha registro.
        /// </summary>
        /// <value>
        /// The fecha registro.
        /// </value>
        public DateTime FechaRegistro { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class UsuarioListadoResponse
    {
        /// <summary>
        /// Gets or sets the identifier usuario.
        /// </summary>
        /// <value>
        /// The identifier usuario.
        /// </value>
        public int IdUsuario { get; set; }
        /// <summary>
        /// Gets or sets the identifier empleado.
        /// </summary>
        /// <value>
        /// The identifier empleado.
        /// </value>
        public int IdEmpleado { get; set; }
        /// <summary>
        /// Gets or sets the identifier rol.
        /// </summary>
        /// <value>
        /// The identifier rol.
        /// </value>
        public int IdRol { get; set; }
        /// <summary>
        /// Gets or sets the nombres.
        /// </summary>
        /// <value>
        /// The nombres.
        /// </value>
        public string Nombres { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the apellidos.
        /// </summary>
        /// <value>
        /// The apellidos.
        /// </value>
        public string Apellidos { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the imagen perfil.
        /// </summary>
        /// <value>
        /// The imagen perfil.
        /// </value>
        public string? ImagenPerfil { get; set; }
        /// <summary>
        /// Gets or sets the ultimo acceso.
        /// </summary>
        /// <value>
        /// The ultimo acceso.
        /// </value>
        public DateTime? UltimoAcceso { get; set; }
        /// <summary>
        /// Gets or sets the ultimo login.
        /// </summary>
        /// <value>
        /// The ultimo login.
        /// </value>
        public DateTime? UltimoLogin { get => UltimoAcceso; set => UltimoAcceso = value; }
        /// <summary>
        /// Gets or sets the nombre rol.
        /// </summary>
        /// <value>
        /// The nombre rol.
        /// </value>
        public string NombreRol { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the estado.
        /// </summary>
        /// <value>
        /// The estado.
        /// </value>
        public string Estado { get; set; } = "PENDIENTE";
    }

    /// <summary>
    /// 
    /// </summary>
    public class UsuariosApiService
    {
        /// <summary>
        /// Descargars the imagen perfil asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        public async Task<byte[]> DescargarImagenPerfilAsync(int idUsuario, CancellationToken ct = default)
        {
            return await ClienteApi.DescargarImagenPerfilAsync(idUsuario, ct);
        }

        /// <summary>
        /// Obteners the usuario asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <returns></returns>
        public async Task<UsuarioDetalle> ObtenerUsuarioAsync(int idUsuario)
        {
            return await ClienteApi.ObtenerAutenticadoAsync<UsuarioDetalle>(
                $"api/usuarios/{idUsuario}");
        }

        /// <summary>
        /// Listars the asynchronous.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<UsuarioListadoResponse>> ListarAsync()
        {
            return await ClienteApi.ObtenerAutenticadoAsync<IEnumerable<UsuarioListadoResponse>>("api/usuarios");
        }

        /// <summary>
        /// Crears the asynchronous.
        /// </summary>
        /// <param name="identidad">The identidad.</param>
        /// <param name="nombres">The nombres.</param>
        /// <param name="apellidos">The apellidos.</param>
        /// <param name="fechaNacimiento">The fecha nacimiento.</param>
        /// <param name="genero">The genero.</param>
        /// <param name="telefono">The telefono.</param>
        /// <param name="idRol">The identifier rol.</param>
        /// <param name="nombreUsuario">The nombre usuario.</param>
        /// <param name="correoLogin">The correo login.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException">
        /// null
        /// or
        /// La API no devolvió los datos del usuario creado.
        /// </exception>
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
                var mensaje = await ExtraerMensajeErrorAsync(respuesta, "No fue posible crear el usuario.", ct);
                throw new HttpRequestException(mensaje, null, respuesta.StatusCode);
            }

            return await respuesta.Content.ReadFromJsonAsync<CrearUsuarioResponse>(cancellationToken: ct)
                ?? throw new HttpRequestException("La API no devolvió los datos del usuario creado.");
        }

        /// <summary>
        /// Subirs the imagen perfil asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="rutaArchivo">The ruta archivo.</param>
        /// <param name="ct">The ct.</param>
        public async Task SubirImagenPerfilAsync(int idUsuario, string rutaArchivo, CancellationToken ct = default)
        {
            await ClienteApi.SubirArchivoAutenticadoAsync(
                $"api/usuarios/{idUsuario}/imagen",
                rutaArchivo,
                "archivo",
                ct);
        }

        /// <summary>
        /// Modificars the asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="solicitud">The solicitud.</param>
        /// <param name="ct">The ct.</param>
        /// <exception cref="HttpRequestException">null</exception>
        public async Task ModificarAsync(int idUsuario, object solicitud, CancellationToken ct = default)
        {
            using var respuesta = await ClienteApi.PutAutenticadoAsync($"api/usuarios/{idUsuario}", solicitud, ct);
            if (!respuesta.IsSuccessStatusCode) 
            {
                var mensaje = await ExtraerMensajeErrorAsync(respuesta, "No fue posible actualizar el usuario.", ct);
                throw new HttpRequestException(mensaje, null, respuesta.StatusCode);
            }
        }

        /// <summary>
        /// Modificars the rol asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="idRol">The identifier rol.</param>
        /// <param name="ct">The ct.</param>
        /// <exception cref="HttpRequestException">null</exception>
        public async Task ModificarRolAsync(int idUsuario, int idRol, CancellationToken ct = default)
        {
            using var respuesta = await ClienteApi.PatchAutenticadoAsync($"api/usuarios/{idUsuario}/rol", new { idRol }, ct);
            if (!respuesta.IsSuccessStatusCode)
            {
                var mensaje = await ExtraerMensajeErrorAsync(respuesta, "No fue posible actualizar el rol del usuario.", ct);
                throw new HttpRequestException(mensaje, null, respuesta.StatusCode);
            }
        }

        /// <summary>
        /// Reenviars the activacion asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="ct">The ct.</param>
        /// <exception cref="HttpRequestException">null</exception>
        public async Task ReenviarActivacionAsync(int idUsuario, CancellationToken ct = default)
        {
            using var respuesta = await ClienteApi.PostAutenticadoAsync($"api/usuarios/{idUsuario}/reenviar-activacion", new { }, ct);
            if (!respuesta.IsSuccessStatusCode)
            {
                var mensaje = await ExtraerMensajeErrorAsync(respuesta, "No fue posible reenviar la activación.", ct);
                throw new HttpRequestException(mensaje, null, respuesta.StatusCode);
            }
        }

        /// <summary>
        /// Desactivars the asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="password">The password.</param>
        /// <param name="ct">The ct.</param>
        public async Task DesactivarAsync(int idUsuario, string password, CancellationToken ct = default)
        {
            await EjecutarAccionAsync($"api/usuarios/{idUsuario}/desactivar", password, "No fue posible desactivar el usuario.", ct);
        }

        /// <summary>
        /// Reactivars the asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="password">The password.</param>
        /// <param name="ct">The ct.</param>
        public async Task ReactivarAsync(int idUsuario, string password, CancellationToken ct = default)
        {
            await EjecutarAccionAsync($"api/usuarios/{idUsuario}/reactivar", password, "No fue posible reactivar el usuario.", ct);
        }

        /// <summary>
        /// Restablecers the password asynchronous.
        /// </summary>
        /// <param name="idUsuario">The identifier usuario.</param>
        /// <param name="password">The password.</param>
        /// <param name="ct">The ct.</param>
        public async Task RestablecerPasswordAsync(int idUsuario, string password, CancellationToken ct = default)
        {
            await EjecutarAccionAsync($"api/usuarios/{idUsuario}/restablecer-password", password, "No fue posible solicitar el restablecimiento de contraseña.", ct);
        }

        /// <summary>
        /// Ejecutars the accion asynchronous.
        /// </summary>
        /// <param name="ruta">The ruta.</param>
        /// <param name="password">The password.</param>
        /// <param name="mensaje">The mensaje.</param>
        /// <param name="ct">The ct.</param>
        /// <exception cref="HttpRequestException">null</exception>
        private static async Task EjecutarAccionAsync(string ruta, string password, string mensaje, CancellationToken ct)
        {
            using var respuesta = await ClienteApi.PostAutenticadoAsync(ruta, new { password }, ct);
            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await ExtraerMensajeErrorAsync(respuesta, mensaje, ct);
                throw new HttpRequestException(error, null, respuesta.StatusCode);
            }
        }

        /// <summary>
        /// Extraers the mensaje error asynchronous.
        /// </summary>
        /// <param name="respuesta">The respuesta.</param>
        /// <param name="mensajePorDefecto">The mensaje por defecto.</param>
        /// <param name="ct">The ct.</param>
        /// <returns></returns>
        private static async Task<string> ExtraerMensajeErrorAsync(HttpResponseMessage respuesta, string mensajePorDefecto, CancellationToken ct)
        {
            var contenido = await respuesta.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(contenido))
                return mensajePorDefecto;

            try
            {
                using var documento = JsonDocument.Parse(contenido);
                if (documento.RootElement.TryGetProperty("detail", out var detalle) && detalle.ValueKind == JsonValueKind.String)
                    return detalle.GetString() ?? mensajePorDefecto;
                if (documento.RootElement.TryGetProperty("message", out var msg) && msg.ValueKind == JsonValueKind.String)
                    return msg.GetString() ?? mensajePorDefecto;
                if (documento.RootElement.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                    return title.GetString() ?? mensajePorDefecto;
            }
            catch (JsonException)
            {
                if (!contenido.TrimStart().StartsWith("{") && !contenido.TrimStart().StartsWith("["))
                    return contenido;
            }

            return mensajePorDefecto;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class CrearUsuarioResponse
    {
        /// <summary>
        /// Gets or sets the identifier usuario.
        /// </summary>
        /// <value>
        /// The identifier usuario.
        /// </value>
        public int IdUsuario { get; set; }
        /// <summary>
        /// Gets or sets the identifier empleado.
        /// </summary>
        /// <value>
        /// The identifier empleado.
        /// </value>
        public int IdEmpleado { get; set; }
    }
}

