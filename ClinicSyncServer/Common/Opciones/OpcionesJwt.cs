namespace ClinicSyncServer.Common.Opciones
{
    public class OpcionesJwt
    {
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public string Key { get; set; } = "ClinicSync_Secret_Key_For_Jwt_Auth_2026_Development_Key!";
        public int AccesoMinutos { get; set; } = 15;
        public int DesafioMinutos { get; set; } = 5;
    }
}
