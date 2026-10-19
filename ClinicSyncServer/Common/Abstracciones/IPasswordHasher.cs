using System.Security.Cryptography;

namespace ClinicSyncServer.Common.Abstracciones
{
    /// <summary>
    /// IPassowrdHasher define la interfaz de la clase encargada de generar y validar
    /// las contraseñas.
    /// </summary>
    public interface IPasswordHasher
    {
        string HashFicticio { get; }

        string Hash(string clave);
        bool Comparar(string clave, string hashAlmacenado);
    }
}
