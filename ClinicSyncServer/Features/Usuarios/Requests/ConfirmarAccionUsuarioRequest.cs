using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Usuarios.Requests;

public sealed record ConfirmarAccionUsuarioRequest(
    [property: Required(ErrorMessage = "La contraseña del administrador es obligatoria.")]
    string Password);
