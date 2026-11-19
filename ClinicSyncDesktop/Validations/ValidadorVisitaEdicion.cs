namespace ClinicSyncDesktop.Validations;

public record EntradaVisitaEdicion(string TextoFecha, string Motivo);

public sealed class ValidadorVisitaEdicion : IValidador<EntradaVisitaEdicion, ResultadoValidacion>
{
    public ResultadoValidacion Validar(EntradaVisitaEdicion entrada)
    {
        var resultado = new ResultadoValidacion();
        resultado.Campo<string?>(nameof(entrada.Motivo), entrada.Motivo).Requerido("Debes ingresar el motivo de la visita.");
        if (!DateTime.TryParseExact(entrada.TextoFecha.Trim(), "yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var fecha))
            resultado.RegistrarError(nameof(entrada.TextoFecha), "La fecha debe tener el formato año/mes/día.");
        else if (fecha.Date > DateTime.Today)
            resultado.RegistrarError(nameof(entrada.TextoFecha), "La fecha de la visita no puede ser posterior a la fecha actual.");
        return resultado;
    }
}