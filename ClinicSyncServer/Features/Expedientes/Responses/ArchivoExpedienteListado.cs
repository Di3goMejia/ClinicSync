namespace ClinicSyncServer.Features.Expedientes.Responses
{
    /* Detalles:
	   - IdExpediente e IdVisita se excluyen; son implícitos.
	   - NombreArchivo aparece solo como Nombre.
	   - HashArchivo se excluye. Se envía mediante header.
    */
    public class ArchivoExpedienteListado
    {

		public int IdArchivo { get; set; }
		public int IdUsuario { get; set; }
		public string Nombre { get; set; } = string.Empty;
		public string Clave { get; set; } = string.Empty;
		public string? Descripcion { get; set; }
		public string TipoMime { get; set; } = string.Empty;
		public int TamanoBytes { get; set; }
		public DateTime FechaSubida { get; set; }
		public bool Activo { get; set; }
    }
}
