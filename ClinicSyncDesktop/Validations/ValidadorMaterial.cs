namespace ClinicSyncDesktop.Validations;

public record EntradaMaterial(
    string Nombre,
    string Codigo,
    string UnidadMedida,
    string TextoPrecioCompra,
    string TextoCantidadInicial,
    string TextoStockMinimo);

public class ResultadoValidacionMaterial : ResultadoValidacion
{
    public decimal PrecioCompra { get; set; }
    public decimal CantidadInicial { get; set; }
    public decimal StockMinimo { get; set; }
}

public class ValidadorMaterial : IValidador<EntradaMaterial, ResultadoValidacionMaterial>
{
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