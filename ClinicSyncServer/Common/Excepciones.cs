namespace ClinicSyncServer.Common
{
    public class ReglaNegocioException(string mensaje, int codigo) : Exception(mensaje)
    {
        public int Codigo { get; } = codigo;
    }

    public class ConflictoException(string mensaje) : Exception(mensaje);

    public class NoAutorizadoException(string mensaje) : Exception(mensaje);
}
