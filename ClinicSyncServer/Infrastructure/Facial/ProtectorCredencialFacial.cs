
using Microsoft.AspNetCore.DataProtection;

namespace ClinicSyncServer.Infrastructure.Facial
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ProtectorCredencialFacial
    {
        /// <summary>
        /// The protector
        /// </summary>
        private readonly IDataProtector _protector;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProtectorCredencialFacial"/> class.
        /// </summary>
        /// <param name="provider">The provider.</param>
        public ProtectorCredencialFacial(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector(
                "ClinicSync.CredencialesFaciales.v1");
        }

        /// <summary>
        /// Protegers the specified modelo.
        /// </summary>
        /// <param name="modelo">The modelo.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">El modelo facial no puede estar vacío. - modelo</exception>
        public byte[] Proteger(byte[] modelo)
        {
            if (modelo == null || modelo.Length == 0)
            {
                throw new ArgumentException(
                    "El modelo facial no puede estar vacío.",
                    nameof(modelo));
            }

            return _protector.Protect(modelo);
        }

        /// <summary>
        /// Desprotegers the specified credencial.
        /// </summary>
        /// <param name="credencial">The credencial.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">La credencial facial no puede estar vacía. - credencial</exception>
        public byte[] Desproteger(byte[] credencial)
        {
            if (credencial == null || credencial.Length == 0)
            {
                throw new ArgumentException(
                    "La credencial facial no puede estar vacía.",
                    nameof(credencial));
            }

            return _protector.Unprotect(credencial);
        }
    }
}
