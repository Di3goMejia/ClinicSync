using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Usuarios.Requests;

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Usuarios.Requests.ConfirmarAccionUsuarioRequest&gt;" />
/// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Usuarios.Requests.ConfirmarAccionUsuarioRequest&gt;" />
public sealed record ConfirmarAccionUsuarioRequest(
    [property: Required(ErrorMessage = "La contraseña del administrador es obligatoria.")]
    string Password);
