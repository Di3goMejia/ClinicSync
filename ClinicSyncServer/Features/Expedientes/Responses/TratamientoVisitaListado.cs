
namespace ClinicSyncServer.Features.Expedientes.Responses
{
    /// <summary>
    /// 
    /// </summary>
    public class TratamientoVisitaListado
    {
        /// <summary>
        /// Gets or sets the identifier tratamiento visita.
        /// </summary>
        /// <value>
        /// The identifier tratamiento visita.
        /// </value>
        public int IdTratamientoVisita { get; set; }
        /// <summary>
        /// Gets or sets the identifier visita.
        /// </summary>
        /// <value>
        /// The identifier visita.
        /// </value>
        public int IdVisita { get; set; }
        /// <summary>
        /// Gets or sets the identifier tratamiento.
        /// </summary>
        /// <value>
        /// The identifier tratamiento.
        /// </value>
        public int IdTratamiento { get; set; }
        /// <summary>
        /// Gets or sets the cantidad.
        /// </summary>
        /// <value>
        /// The cantidad.
        /// </value>
        public int Cantidad { get; set; }
        /// <summary>
        /// Gets or sets the precio unitario.
        /// </summary>
        /// <value>
        /// The precio unitario.
        /// </value>
        public decimal PrecioUnitario { get; set; }
        /// <summary>
        /// Gets or sets the descuento.
        /// </summary>
        /// <value>
        /// The descuento.
        /// </value>
        public decimal Descuento { get; set; }
        /// <summary>
        /// Gets or sets the notas.
        /// </summary>
        /// <value>
        /// The notas.
        /// </value>
        public string? Notas { get; set; }
        /// <summary>
        /// Gets or sets the nombre tratamiento.
        /// </summary>
        /// <value>
        /// The nombre tratamiento.
        /// </value>
        public string NombreTratamiento { get; set; } = "";
        /// <summary>
        /// Gets or sets a value indicating whether [tratamiento activo].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [tratamiento activo]; otherwise, <c>false</c>.
        /// </value>
        public bool TratamientoActivo { get; set; }
    }
}
