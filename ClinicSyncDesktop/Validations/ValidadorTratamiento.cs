namespace ClinicSyncDesktop.Validations;

public record EntradaTratamiento(string Nombre, string TextoPrecio, string? Descripcion);

public class ResultadoValidacionTratamiento : ResultadoValidacion
{
    public decimal PrecioNormalizado { get; set; }
}

public class ValidadorTratamiento : IValidador<EntradaTratamiento, ResultadoValidacionTratamiento>
{
    public ResultadoValidacionTratamiento Validar(EntradaTratamiento entrada)
    {
        var resultado = new ResultadoValidacionTratamiento();

        // NOMBRE
        resultado.Campo<string?>(nameof(entrada.Nombre), entrada.Nombre)
            .Requerido("El nombre es obligatorio.")
            .LongitudMaxima(100, "El nombre no puede superar los 100 caracteres.");

        // PRECIO
        resultado.Campo<string?>(nameof(entrada.TextoPrecio), entrada.TextoPrecio)
            .Requerido("El precio es obligatorio")
            .DecimalValido(out decimal precio, "Ingresa un precio válido (ej. 350.00).");

        if (resultado.Errores.All(e => e.Campo != nameof(entrada.TextoPrecio)))
        {
            resultado.Campo(nameof(entrada.TextoPrecio), precio)
                .ValorPositivo("El precio debe ser mayor a cero.")
                .ValorMaximo(99_999_999.99m, "El precio supera el limite permitido.")
                .MaximoDecimales(2, "El precio solo puede tener dos decimales.");
        }

        // DESCRIPCIÓN
        resultado.Campo(nameof(entrada.Descripcion), entrada.Descripcion)
            .LongitudMaxima(200, "La descripción no puede superar los 200 caracteres.");

        if (resultado.EsValido)
            resultado.PrecioNormalizado = precio;

        return resultado;
    }
}
