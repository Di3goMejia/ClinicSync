
namespace ClinicSyncServer.Features.Pacientes.Responses
{
    public sealed class PacienteListado
    {
        public int IdPaciente { get; set; }

        public string Identidad { get; set; } = "";

        public string Nombres { get; set; } = "";

        public string Apellidos { get; set; } = "";

        public DateTime FechaNacimiento { get; set; }

        public string Genero { get; set; } = "";

        public string Telefono { get; set; } = "";

        public DateTime FechaRegistro { get; set; }

        public bool Activo { get; set; }
    }
}
