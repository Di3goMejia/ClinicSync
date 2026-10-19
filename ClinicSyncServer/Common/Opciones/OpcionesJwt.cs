namespace ClinicSyncServer.Common.Opciones
{
    public class OpcionesJwt
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public int AccesoMinutos { get; set; } = 15;
        public int DesafioMinutos { get; set; } = 5;
    }
}
