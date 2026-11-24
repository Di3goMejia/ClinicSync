namespace ClinicSyncDesktop.Validations;

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.IEquatable&lt;ClinicSyncDesktop.Validations.EntradaMaterial&gt;" />
public record EntradaMaterial(
    string Nombre,
    string Codigo,
    string UnidadMedida,
    string TextoPrecioCompra,
    string TextoCantidadInicial,
    string TextoStockMinimo);

/// <summary>
/// 
/// </summary>
/// <seealso cref="ClinicSyncDesktop.Validations.ResultadoValidacion" />
public class ResultadoValidacionMaterial : ResultadoValidacion
{
    /// <summary>
    /// Gets or sets the precio compra.
    /// </summary>
    /// <value>
    /// The precio compra.
    /// </value>
    public decimal PrecioCompra { get; set; }
    /// <summary>
    /// Gets or sets the cantidad inicial.
    /// </summary>
    /// <value>
    /// The cantidad inicial.
    /// </value>
    public decimal CantidadInicial { get; set; }
    /// <summary>
    /// Gets or sets the stock minimo.
    /// </summary>
    /// <value>
    /// The stock minimo.
    /// </value>
    public decimal StockMinimo { get; set; }
}

/// <summary>
/// 
/// </summary>
/// <seealso cref="ClinicSyncDesktop.Validations.IValidador&lt;ClinicSyncDesktop.Validations.EntradaMaterial, ClinicSyncDesktop.Validations.ResultadoValidacionMaterial&gt;" />
public class ValidadorMaterial : IValidador<EntradaMaterial, ResultadoValidacionMaterial>
{
    /// <summary>
    /// Validars the specified entrada.
    /// </summary>
    /// <param name="entrada">The entrada.</param>
    /// <returns></returns>
    public ResultadoValidacionMaterial Validar(EntradaMaterial entrada)
    {
        var resultado = new ResultadoValidacionMaterial();
        resultado.Campo<string?>(nameof(entrada.Nombre), entrada.Nombre)
            .Requerido("Ingresa el nombre del material.");
        resultado.Campo<string?>(nameof(entrada.Codigo), entrada.Codigo)
            .Requerido("Ingresa un código.");
        resultado.Campo<string?>(nameof(entrada.UnidadMedida), entrada.UnidadMedida)
            .Requerido("Ingresa una unidad de medida.");

        ValidarDecimal(resultado, nameof(entrada.TextoPrecioCompra), entrada.TextoPrecioCompra,
            "Ingresa el precio por unidad.", "Ingresa un precio válido.", 99_999_999.99m,
            out var precio);
        ValidarDecimal(resultado, nameof(entrada.TextoCantidadInicial), entrada.TextoCantidadInicial,
            "Ingresa la cantidad inicial.", "Ingresa una cantidad inicial válida.", 9_999_999_999.99m,
            out var inicial);
        ValidarDecimal(resultado, nameof(entrada.TextoStockMinimo), entrada.TextoStockMinimo,
            "Ingresa la cantidad mínima.", "Ingresa una cantidad mínima válida.", 9_999_999_999.99m,
            out var minimo);

        if (resultado.EsValido)
        {
            resultado.PrecioCompra = precio;
            resultado.CantidadInicial = inicial;
            resultado.StockMinimo = minimo;
        }

        return resultado;
    }

    /// <summary>
    /// Validars the decimal.
    /// </summary>
    /// <param name="resultado">The resultado.</param>
    /// <param name="campo">The campo.</param>
    /// <param name="texto">The texto.</param>
    /// <param name="requerido">The requerido.</param>
    /// <param name="invalido">The invalido.</param>
    /// <param name="maximo">The maximo.</param>
    /// <param name="valor">The valor.</param>
    private static void ValidarDecimal(
        ResultadoValidacion resultado, string campo, string texto,
        string requerido, string invalido, decimal maximo, out decimal valor)
    {
        valor = 0;
        var regla = resultado.Campo<string?>(campo, texto).Requerido(requerido);
        if (string.IsNullOrWhiteSpace(texto)) return;
        if (!decimal.TryParse(texto.Trim(), System.Globalization.NumberStyles.Number,
            System.Globalization.CultureInfo.CurrentCulture, out valor) &&
            !decimal.TryParse(texto.Trim(), System.Globalization.NumberStyles.Number,
                System.Globalization.CultureInfo.InvariantCulture, out valor))
        {
            regla.AgregarError(invalido);
            return;
        }
        resultado.Campo(campo, valor)
            .ValorNoNegativo("El valor no puede ser negativo.")
            .ValorMaximo(maximo, "El valor supera el límite permitido.");
    }
}