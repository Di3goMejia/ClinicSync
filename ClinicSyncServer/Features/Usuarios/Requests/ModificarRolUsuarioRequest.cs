using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Usuarios.Requests
{
    public record ModificarRolUsuarioRequest(
        int? IdUsuario,

        [property: Required(ErrorMessage = "El ID de rol es obligatorio.")]
        int? IdRol
    );
}
