
using Microsoft.AspNetCore.DataProtection;

namespace ClinicSyncServer.Infrastructure.Facial
{
    public sealed class ProtectorCredencialFacial
    {
        private readonly IDataProtector _protector;

        public ProtectorCredencialFacial(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector(
                "ClinicSync.CredencialesFaciales.v1");
        }

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
