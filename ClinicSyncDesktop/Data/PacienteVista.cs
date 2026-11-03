using System;

namespace ClinicSyncDesktop.Data
{
    /// <summary>
    /// Proyección de paciente usada en las ventanas de agregar y editar.
    /// Contiene los campos que se muestran y editan en el formulario.
    /// </summary>
    public class PacienteVista
    {
        public int IdPaciente { get; set; }
        public string Identidad { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public DateTime FechaNacimiento { get; set; }
        public string Sexo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }
}

