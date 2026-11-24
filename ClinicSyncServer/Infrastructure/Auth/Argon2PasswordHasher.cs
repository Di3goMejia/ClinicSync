using ClinicSyncServer.Common.Abstracciones;
using Konscious.Security.Cryptography;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace ClinicSyncServer.Infrastructure.Auth
{
    /// <summary>
    /// Clase para generar y verificar hashes de contraseñas utilizando el algoritmo Argon2id.
    /// </summary>
    /// <seealso cref="ClinicSyncServer.Common.Abstracciones.IPasswordHasher" />
    public class Argon2PasswordHasher : IPasswordHasher
    {
        // Hash ficticio generado por el servidor, utilizado en logins fallidos como método
        // de prevención de ataques de temporización.
        /// <summary>
        /// Gets the hash ficticio.
        /// </summary>
        /// <value>
        /// The hash ficticio.
        /// </value>
        public string HashFicticio { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Argon2PasswordHasher"/> class.
        /// </summary>
        public Argon2PasswordHasher()
        {
            HashFicticio = Hash(Guid.NewGuid().ToString());
        }

        // Parametros para generar el hash de la contraseña.
        // Se utiliza la segunda recomendación proporcionada por el RFC 9106.
        // https://www.rfc-editor.org/info/rfc9106/#section-4-5
        /// <summary>
        /// The memoria kb
        /// </summary>
        private const int MemoriaKb = 65_536; // 64 MB
        /// <summary>
        /// The iteraciones
        /// </summary>
        private const int Iteraciones = 3; // Número de iteraciones.
        /// <summary>
        /// The paralelismo
        /// </summary>
        private const int Paralelismo = 4; // Número de hilos a utilizar.
        /// <summary>
        /// The tamano salt
        /// </summary>
        private const int TamanoSalt = 16; // 128 bits
        /// <summary>
        /// The tamano hash
        /// </summary>
        private const int TamanoHash = 32; // 256 bits

        /// <summary>
        /// Genera el hash de la clave especificada.
        /// </summary>
        /// <param name="clave">La clave.</param>
        /// <returns></returns>
        public string Hash(string clave)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(TamanoSalt);
            byte[] hash = Calcular(clave, salt, TamanoHash);

            var saltBase64 = Convert.ToBase64String(salt).TrimEnd("=");
            var hashBase64 = Convert.ToBase64String(hash).TrimEnd("=");

            return $"{saltBase64}.{hashBase64}";
        }

        /// <summary>
        /// Compara la clave especificada.
        /// </summary>
        /// <param name="clave">La clave a comparar.</param>
        /// <param name="hashAlmacenado">El hash almacenado en la base de datos.</param>
        /// <returns></returns>
        public bool Comparar(string clave, string hashAlmacenado)
        {
            if (string.IsNullOrWhiteSpace(hashAlmacenado))
                return false;

            int separador = hashAlmacenado.IndexOf('.');
            if (separador <= 0 || separador == hashAlmacenado.Length - 1)
                return false;

            string parteSalt = hashAlmacenado[..separador];
            string parteHash = hashAlmacenado[(separador + 1)..];

            try
            {
                static byte[] DecodificarBase64SinPadding(string valor)
                {
                    int padding = (4 - valor.Length % 4) % 4;
                    return Convert.FromBase64String(valor + new string('=', padding));
                }

                byte[] salt = DecodificarBase64SinPadding(parteSalt);
                byte[] hashVerificar = DecodificarBase64SinPadding(parteHash);

                byte[] hashCalculado = Calcular(clave, salt, hashVerificar.Length);

                return CryptographicOperations.FixedTimeEquals(
                    hashCalculado,
                    hashVerificar);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Calculars the specified clave.
        /// </summary>
        /// <param name="clave">The clave.</param>
        /// <param name="salt">The salt.</param>
        /// <param name="longitud">The longitud.</param>
        /// <returns></returns>
        private static byte[] Calcular(string clave, byte[] salt, int longitud)
        {
            using var argon = new Argon2id(Encoding.UTF8.GetBytes(clave))
            {
                MemorySize = MemoriaKb,
                Iterations = Iteraciones,
                DegreeOfParallelism = Paralelismo,
                Salt = salt
            };

            return argon.GetBytes(longitud);
        }
    }
}
