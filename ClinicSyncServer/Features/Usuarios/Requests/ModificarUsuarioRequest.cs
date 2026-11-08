using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Usuarios.Requests
{
    public record ModificarUsuarioRequest(
        int? IdUsuario,

        [property: Required(ErrorMessage = "El número de identidad es obligatorio.")]
        string Identidad,

        [property: Required(ErrorMessage = "El nombre es obligatorio.")]
        string Nombres,

        [property: Required(ErrorMessage = "El apellido es obligatorio.")]
        string Apellidos,

        [property: Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        DateOnly FechaNacimiento,

        [property: Required(ErrorMessage = "El género es obligatorio.")]
        [property: AllowedValues('F', 'M', ErrorMessage = "El género debe ser F o M.")]
        char? Genero,

        [property: Required(ErrorMessage = "El teléfono es obligatorio.")]
        [property: Phone(ErrorMessage = "El formato de télefono es inválido.")]
        string Telefono,

        [property: EmailAddress(ErrorMessage = "El formato del correo de contacto es inválido.")]
        string CorreoContacto,

        [property: Url(ErrorMessage = "El formato de URL de la imágen de perfil es inválido.")]
        string? ImagenPerfil,

        [property: Required(ErrorMessage = "El ID del rol es obligatorio.")]
        int? IdRol
    );
}
