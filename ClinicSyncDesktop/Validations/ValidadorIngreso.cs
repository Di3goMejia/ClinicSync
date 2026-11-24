namespace ClinicSyncDesktop.Validations;

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.IEquatable&lt;ClinicSyncDesktop.Validations.EntradaIngreso&gt;" />
public record EntradaIngreso(object? Paciente,DateTime? Fecha, string TextoHora, string TextoDescuento,
    string? MedioPago, decimal Subtotal, bool SoloFecha = false);

/// <summary>
/// 
/// </summary>
/// <seealso cref="ClinicSyncDesktop.Validations.ResultadoValidacion" />
public sealed class ResultadoValidacionIngreso : ResultadoValidacion
{
    /// <summary>
    /// Gets or sets the fecha.
    /// </summary>
    /// <value>
    /// The fecha.
    /// </value>
    public DateTime Fecha { get; set; }
    /// <summary>
    /// Gets or sets the descuento.
    /// </summary>
    /// <value>
    /// The descuento.
    /// </value>
    public decimal Descuento { get; set; }
}

/// <summary>
/// 
/// </summary>
/// <seealso cref="ClinicSyncDesktop.Validations.IValidador&lt;ClinicSyncDesktop.Validations.EntradaIngreso, ClinicSyncDesktop.Validations.ResultadoValidacionIngreso&gt;" />
public sealed class ValidadorIngreso : IValidador<EntradaIngreso, ResultadoValidacionIngreso>
{
    /// <summary>
    /// Validars the specified entrada.
    /// </summary>
    /// <param name="entrada">The entrada.</param>
    /// <returns></returns>
    public ResultadoValidacionIngreso Validar(EntradaIngreso entrada)
    {
        var resultado = new ResultadoValidacionIngreso();
        if (entrada.Paciente is null)
            resultado.RegistrarError(nameof(entrada.Paciente), "Seleccione un paciente.");
        if (entrada.Fecha is not DateTime fecha)
        {
            resultado.RegistrarError(
                nameof(entrada.Fecha),
                "Seleccione una fecha.");
        }
        else if (entrada.SoloFecha)
        {
            resultado.Fecha = fecha.Date;
            if (fecha.Date > DateTime.Today)
            {
                resultado.RegistrarError(nameof(entrada.Fecha),"La fecha no puede ser futura.");
            }
        }
        else
        {
            if (!TimeSpan.TryParseExact(entrada.TextoHora?.Trim(),@"hh\:mm",
                System.Globalization.CultureInfo.InvariantCulture,out var hora))
            {
                resultado.RegistrarError(nameof(entrada.Fecha),"Ingrese una fecha y hora válidas.");
            }
            else
            {
                resultado.Fecha = fecha.Date.Add(hora);
                resultado.Campo(nameof(entrada.Fecha),resultado.Fecha).NoFuturo("La fecha y hora no pueden ser futuras.");
            }
        }
        if (!decimal.TryParse(entrada.TextoDescuento.Trim(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.CurrentCulture, out var descuento) &&
            !decimal.TryParse(entrada.TextoDescuento.Trim(), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out descuento))
            resultado.RegistrarError(nameof(entrada.TextoDescuento), "Ingrese un descuento válido.");
        else
        {
            resultado.Campo(nameof(entrada.TextoDescuento), descuento).ValorNoNegativo("El descuento no puede ser menor que 0.").ValorMaximo(entrada.Subtotal, "El descuento no puede superar el subtotal.");
            resultado.Descuento = descuento;
        }
        if (entrada.MedioPago is not ("Efectivo" or "Transferencia"))
            resultado.RegistrarError(nameof(entrada.MedioPago), "Seleccione Efectivo o Transferencia.");
        return resultado;
    }
}