namespace ClinicSyncServer.Common.Opciones
{
    /// <summary>
    /// 
    /// </summary>
    public class OpcionesJwt
    {
        /// <summary>
        /// Gets or sets the issuer.
        /// </summary>
        /// <value>
        /// The issuer.
        /// </value>
        public string Issuer { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the audience.
        /// </summary>
        /// <value>
        /// The audience.
        /// </value>
        public string Audience { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the acceso minutos.
        /// </summary>
        /// <value>
        /// The acceso minutos.
        /// </value>
        public int AccesoMinutos { get; set; } = 15;
        /// <summary>
        /// Gets or sets the desafio minutos.
        /// </summary>
        /// <value>
        /// The desafio minutos.
        /// </value>
        public int DesafioMinutos { get; set; } = 5;
    }
}
