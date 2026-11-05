namespace ClinicSyncDesktop.Models
{
    // Paciente tal como se muestra en el selector del formulario de citas.
    public class PacienteOpcion
    {
        public int IdPaciente { get; set; }
        public string Nombres { get; set; } = "";
        public string Apellidos { get; set; } = "";
        public bool Activo { get; set; }

        // Texto que se ve en el selector.
        public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();
    }
}