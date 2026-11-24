namespace ClinicSyncServer.Common
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class ReglaNegocioException(string mensaje, int codigo) : Exception(mensaje)
    {
        /// <summary>
        /// Gets the codigo.
        /// </summary>
        /// <value>
        /// The codigo.
        /// </value>
        public int Codigo { get; } = codigo;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class ConflictoException(string mensaje) : Exception(mensaje);

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class NoAutorizadoException(string mensaje) : Exception(mensaje);
}
