using System.ComponentModel.DataAnnotations;

namespace ClinicSyncServer.Features.Usuarios.Requests
{
    public record CrearUsuarioRequest(
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

        [property: Required(ErrorMessage = "El ID del rol es obligatorio.")]
        int? IdRol,

        [property: Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [property: StringLength(64, ErrorMessage = "El nombre de usuario debe ser menor a 64 caracteres.")]
        string NombreUsuario,

        [property: Required(ErrorMessage = "El correo para iniciar sesión es obligatorio.")]
        [property: EmailAddress(ErrorMessage = "El formato del correo para iniciar sesión es inválido.")]
        string CorreoLogin,

        [property: EmailAddress(ErrorMessage = "El formato del correo de contacto es inválido.")]
        string CorreoContacto
    );
}
