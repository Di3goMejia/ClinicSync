namespace ClinicSyncServer.Features.Expedientes.Responses
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.IEquatable&lt;ClinicSyncServer.Features.Expedientes.Responses.ArchivoRespuesta&gt;" />
    public record ArchivoRespuesta(
        Guid IdArchivo, int IdUsuario, string Nombre, string Clave, string? Descripcion,
        string TipoMime, int TamanoBytes, DateTime FechaSubida, bool Activo);
}
