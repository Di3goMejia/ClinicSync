using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Usuarios.Requests
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Usuarios.Requests.ModificarRolUsuarioRequest&gt;" />
    public record ModificarRolUsuarioRequest(
        int? IdUsuario,

        [property: Required(ErrorMessage = "El ID de rol es obligatorio.")]
        int? IdRol
    );
}
