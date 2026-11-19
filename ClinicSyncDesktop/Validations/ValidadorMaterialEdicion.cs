namespace ClinicSyncDesktop.Validations;

public record EntradaMaterialEdicion(
    string Nombre, string Codigo, string UnidadMedida,
    string TextoPrecioCompra, string TextoStockMinimo, string TextoStockActual,
    string MotivoAjuste, decimal StockOriginal);

public sealed class ResultadoValidacionMaterialEdicion : ResultadoValidacion
{
    public decimal PrecioCompra { get; set; }
    public decimal StockMinimo { get; set; }
    public decimal StockActual { get; set; }
}

public sealed class ValidadorMaterialEdicion : IValidador<EntradaMaterialEdicion, ResultadoValidacionMaterialEdicion>
{
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