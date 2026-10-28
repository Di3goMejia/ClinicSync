namespace ClinicSyncServer.Features.Expedientes.Responses
{
    public record ArchivoRespuesta(
        Guid IdArchivo, int IdUsuario, string Nombre, string Clave, string? Descripcion,
        string TipoMime, int TamanoBytes, DateTime FechaSubida, bool Activo);
}
