using System.Security.Cryptography;

namespace ClinicSyncServer.Common.Abstracciones
{
    /// <summary>
    /// IPassowrdHasher define la interfaz de la clase encargada de generar y validar
    /// las contraseñas.
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>
        /// Gets the hash ficticio.
        /// </summary>
        /// <value>
        /// The hash ficticio.
        /// </value>
        string HashFicticio { get; }

        /// <summary>
        /// Hashes the specified clave.
        /// </summary>
        /// <param name="clave">The clave.</param>
        /// <returns></returns>
        string Hash(string clave);
        /// <summary>
        /// Comparars the specified clave.
        /// </summary>
        /// <param name="clave">The clave.</param>
        /// <param name="hashAlmacenado">The hash almacenado.</param>
        /// <returns></returns>
        bool Comparar(string clave, string hashAlmacenado);
    }
}
