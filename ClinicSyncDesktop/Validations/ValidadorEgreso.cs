namespace ClinicSyncDesktop.Validations;

public record EntradaEgreso(
    string? Tipo, string? Medio, string Concepto, string TextoFecha,
    DateTime? FechaSeleccionada, string TextoHora, string TextoMonto);

public class ResultadoValidacionEgreso : ResultadoValidacion
{
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }
}

public class ValidadorEgreso : IValidador<EntradaEgreso, ResultadoValidacionEgreso>
{
    public ResultadoValidacionEgreso Validar(EntradaEgreso entrada)
    {
        var resultado = new ResultadoValidacionEgreso();
        if (entrada.Tipo is null) resultado.RegistrarError(nameof(entrada.Tipo), "Se debe seleccionar un tipo.");
        if (entrada.Medio is null) resultado.RegistrarError(nameof(entrada.Medio), "Se debe seleccionar un medio de pago.");
        if (string.IsNullOrWhiteSpace(entrada.Concepto) && entrada.Tipo != "MATERIALES")
            resultado.RegistrarError(nameof(entrada.Concepto), "Se debe ingresar un concepto.");
        if (entrada.FechaSeleccionada is null ||
            !TimeSpan.TryParseExact(entrada.TextoHora.Trim(), @"hh\:mm",
                System.Globalization.CultureInfo.InvariantCulture, out var hora))
            resultado.RegistrarError(nameof(entrada.TextoFecha), "La fecha u hora no es válida. Usa AAAA/MM/DD y HH:mm.");
        else
        {
            resultado.Fecha = entrada.FechaSeleccionada.Value.Date.Add(hora);
            resultado.Campo(nameof(entrada.FechaSeleccionada), resultado.Fecha)
                .NoFuturo("La fecha y hora no pueden ser futuras.");
        }

        var monto = resultado.Campo<string?>(nameof(entrada.TextoMonto), entrada.TextoMonto)
            .Requerido("Se debe ingresar un monto.")
            .DecimalValido(out var normalizado, "El monto debe ser un valor numérico válido.");
        if (!string.IsNullOrWhiteSpace(entrada.TextoMonto) &&
            resultado.Errores.All(e => e.Campo != nameof(entrada.TextoMonto)))
        {
            resultado.Campo(nameof(entrada.TextoMonto), normalizado)
                .ValorPositivo("El monto debe ser mayor que cero.")
                .ValorMaximo(99_999_999.99m, "El monto es demasiado grande.");
            resultado.Monto = normalizado;
        }
        return resultado;
    }
}