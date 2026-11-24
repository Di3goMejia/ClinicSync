namespace ClinicSyncDesktop.Validations;

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.IEquatable&lt;ClinicSyncDesktop.Validations.EntradaMaterialEdicion&gt;" />
public record EntradaMaterialEdicion(
    string Nombre, string Codigo, string UnidadMedida,
    string TextoPrecioCompra, string TextoStockMinimo, string TextoStockActual,
    string MotivoAjuste, decimal StockOriginal);

/// <summary>
/// 
/// </summary>
/// <seealso cref="ClinicSyncDesktop.Validations.ResultadoValidacion" />
public sealed class ResultadoValidacionMaterialEdicion : ResultadoValidacion
{
    /// <summary>
    /// Gets or sets the precio compra.
    /// </summary>
    /// <value>
    /// The precio compra.
    /// </value>
    public decimal PrecioCompra { get; set; }
    /// <summary>
    /// Gets or sets the stock minimo.
    /// </summary>
    /// <value>
    /// The stock minimo.
    /// </value>
    public decimal StockMinimo { get; set; }
    /// <summary>
    /// Gets or sets the stock actual.
    /// </summary>
    /// <value>
    /// The stock actual.
    /// </value>
    public decimal StockActual { get; set; }
}

/// <summary>
/// 
/// </summary>
/// <seealso cref="ClinicSyncDesktop.Validations.IValidador&lt;ClinicSyncDesktop.Validations.EntradaMaterialEdicion, ClinicSyncDesktop.Validations.ResultadoValidacionMaterialEdicion&gt;" />
public sealed class ValidadorMaterialEdicion : IValidador<EntradaMaterialEdicion, ResultadoValidacionMaterialEdicion>
{
    /// <summary>
    /// Validars the specified entrada.
    /// </summary>
    /// <param name="entrada">The entrada.</param>
    /// <returns></returns>
    public ResultadoValidacionMaterialEdicion Validar(EntradaMaterialEdicion entrada)
    {
        var resultado = new ResultadoValidacionMaterialEdicion();
        resultado.Campo<string?>(nameof(entrada.Nombre), entrada.Nombre).Requerido("Ingresa el nombre del material.");
        resultado.Campo<string?>(nameof(entrada.Codigo), entrada.Codigo).Requerido("Ingresa un código.");
        resultado.Campo<string?>(nameof(entrada.UnidadMedida), entrada.UnidadMedida).Requerido("Ingresa una unidad de medida.");
        var precio = Decimal(resultado, nameof(entrada.TextoPrecioCompra), entrada.TextoPrecioCompra, "Ingresa el precio por unidad.", "Ingresa un precio válido.", 99_999_999.99m);
        var minimo = Decimal(resultado, nameof(entrada.TextoStockMinimo), entrada.TextoStockMinimo, "Ingresa la cantidad mínima.", "Ingresa una cantidad mínima válida.", 9_999_999_999.99m);
        var actual = Decimal(resultado, nameof(entrada.TextoStockActual), entrada.TextoStockActual, "Ingresa el stock actual.", "Ingresa una cantidad de stock válida.", 9_999_999_999.99m);
        if (actual != entrada.StockOriginal && string.IsNullOrWhiteSpace(entrada.MotivoAjuste))
            resultado.RegistrarError(nameof(entrada.MotivoAjuste), "Indica el motivo del ajuste de stock.");
        resultado.Campo<string?>(nameof(entrada.MotivoAjuste), entrada.MotivoAjuste)
            .LongitudMaxima(200, "El motivo no puede superar los 200 caracteres.");
        if (resultado.EsValido)
        {
            resultado.PrecioCompra = precio;
            resultado.StockMinimo = minimo;
            resultado.StockActual = actual;
        }
        return resultado;
    }

    /// <summary>
    /// Decimals the specified resultado.
    /// </summary>
    /// <param name="resultado">The resultado.</param>
    /// <param name="campo">The campo.</param>
    /// <param name="texto">The texto.</param>
    /// <param name="requerido">The requerido.</param>
    /// <param name="invalido">The invalido.</param>
    /// <param name="maximo">The maximo.</param>
    /// <returns></returns>
    private static decimal Decimal(ResultadoValidacion resultado, string campo, string texto,
        string requerido, string invalido, decimal maximo)
    {
        resultado.Campo<string?>(campo, texto).Requerido(requerido);
        if (string.IsNullOrWhiteSpace(texto)) return 0;
        if (!decimal.TryParse(texto.Trim(), System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.CurrentCulture, out var valor) &&
            !decimal.TryParse(texto.Trim(), System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out valor))
        {
            resultado.RegistrarError(campo, invalido);
            return 0;
        }
        resultado.Campo(campo, valor).ValorNoNegativo("El valor no puede ser negativo.").ValorMaximo(maximo, "El valor supera el máximo permitido.");
        return valor;
    }
}