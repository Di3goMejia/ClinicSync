namespace ClinicSyncServer.Features.Expedientes.Projections
{
    public class ArchivoExpediente
    {

        public Guid IdArchivo { get; set; }
        public int IdExpediente { get; set; }
        public int? IdVisita { get; set; }
        public int IdUsuario { get; set; }
        public string NombreArchivo { get; set; } = string.Empty;
        public string ClaveAlmacen { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string HashArchivo { get; set; } = string.Empty;
        public string TipoMime { get; set; } = string.Empty;
        public int TamanoBytes { get; set; }
        public DateTime FechaSubida { get; set; }
        public bool Activo { get; set; }
    }
}
