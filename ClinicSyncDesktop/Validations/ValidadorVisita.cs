namespace ClinicSyncDesktop.Validations;

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.IEquatable&lt;ClinicSyncDesktop.Validations.EntradaVisita&gt;" />
public record EntradaVisita(int IdEmpleado, string Doctor, string TextoFecha, string Motivo);

/// <summary>
/// 
/// </summary>
/// <seealso cref="ClinicSyncDesktop.Validations.IValidador&lt;ClinicSyncDesktop.Validations.EntradaVisita, ClinicSyncDesktop.Validations.ResultadoValidacion&gt;" />
public class ValidadorVisita : IValidador<EntradaVisita, ResultadoValidacion>
{
    /// <summary>
    /// Validars the specified entrada.
    /// </summary>
    /// <param name="entrada">The entrada.</param>
    /// <returns></returns>
    public ResultadoValidacion Validar(EntradaVisita entrada)
    {
        var resultado = new ResultadoValidacion();
        if (entrada.IdEmpleado <= 0)
            resultado.RegistrarError(nameof(entrada.IdEmpleado), "No se pudo identificar al empleado de la sesión.");
        resultado.Campo<string?>(nameof(entrada.Doctor), entrada.Doctor)
            .Requerido("No se pudo obtener el nombre del empleado.");
        resultado.Campo<string?>(nameof(entrada.TextoFecha), entrada.TextoFecha)
            .Requerido("Ingresa una fecha válida (YYYY/MM/DD).");
        if (!DateTime.TryParseExact(entrada.TextoFecha.Trim(), "yyyy/MM/dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var fecha))
            resultado.RegistrarError(nameof(entrada.TextoFecha), "Ingresa una fecha válida (YYYY/MM/DD).");
        else if (fecha.Date > DateTime.Today)
            resultado.RegistrarError(nameof(entrada.TextoFecha), "La fecha de la visita no puede ser posterior a hoy.");
        resultado.Campo<string?>(nameof(entrada.Motivo), entrada.Motivo)
            .Requerido("Ingresa el motivo de la visita.");
        return resultado;
    }
}