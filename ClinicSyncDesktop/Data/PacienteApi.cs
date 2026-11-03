using System;

namespace ClinicSyncDesktop.Data
{
    /// <summary>
    /// Proyección de paciente devuelta por la API REST.
    /// </summary>
    public class PacienteApi
    {
        public int IdPaciente { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Genero { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public bool Activo { get; set; }
    }
}

