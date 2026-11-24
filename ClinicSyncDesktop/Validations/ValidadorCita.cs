namespace ClinicSyncDesktop.Validations;

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.IEquatable&lt;ClinicSyncDesktop.Validations.EntradaCita&gt;" />
public record EntradaCita(
    object? Paciente,
    string Titulo,
    DateTime? FechaInicio,
    DateTime? FechaFin,
    bool TodoElDia,
    string? HoraInicio,
    string? HoraFin);

/// <summary>
/// 
/// </summary>
/// <seealso cref="ClinicSyncDesktop.Validations.ResultadoValidacion" />
public sealed class ResultadoValidacionCita : ResultadoValidacion
{
    /// <summary>
    /// Gets or sets the inicio.
    /// </summary>
    /// <value>
    /// The inicio.
    /// </value>
    public DateTime Inicio { get; set; }
    /// <summary>
    /// Gets or sets the fin.
    /// </summary>
    /// <value>
    /// The fin.
    /// </value>
    public DateTime Fin { get; set; }
}

/// <summary>
/// 
/// </summary>
/// <seealso cref="ClinicSyncDesktop.Validations.IValidador&lt;ClinicSyncDesktop.Validations.EntradaCita, ClinicSyncDesktop.Validations.ResultadoValidacionCita&gt;" />
public sealed class ValidadorCita : IValidador<EntradaCita, ResultadoValidacionCita>
{
    /// <summary>
    /// Validars the specified entrada.
    /// </summary>
    /// <param name="entrada">The entrada.</param>
    /// <returns></returns>
    public ResultadoValidacionCita Validar(EntradaCita entrada)
    {
        var resultado = new ResultadoValidacionCita();
        if (entrada.Paciente is null)
            resultado.RegistrarError(nameof(entrada.Paciente), "Se deben llenar los campos obligatorios.");
        resultado.Campo<string?>(nameof(entrada.Titulo), entrada.Titulo)
            .Requerido("Se deben llenar los campos obligatorios.");
        if (entrada.FechaInicio is null)
            resultado.RegistrarError(nameof(entrada.FechaInicio), "Se deben llenar los campos obligatorios.");
        if (entrada.FechaFin is null)
            resultado.RegistrarError(nameof(entrada.FechaFin), "Se deben llenar los campos obligatorios.");

        if (entrada.FechaInicio is not DateTime fechaInicio ||
            entrada.FechaFin is not DateTime fechaFin)
            return resultado;

        resultado.Inicio = fechaInicio.Date;
        resultado.Fin = fechaFin.Date;
        if (!entrada.TodoElDia)
        {
            if (!TimeSpan.TryParseExact(entrada.HoraInicio, @"hh\:mm",
                System.Globalization.CultureInfo.InvariantCulture, out var horaInicio))
                resultado.RegistrarError(nameof(entrada.HoraInicio), "Se deben llenar los campos obligatorios.");
            else
                resultado.Inicio = resultado.Inicio.Add(horaInicio);
            if (!TimeSpan.TryParseExact(entrada.HoraFin, @"hh\:mm",
                System.Globalization.CultureInfo.InvariantCulture, out var horaFin))
                resultado.RegistrarError(nameof(entrada.HoraFin), "Se deben llenar los campos obligatorios.");
            else
                resultado.Fin = resultado.Fin.Add(horaFin);
        }

        if (resultado.Fin < resultado.Inicio ||
            (!entrada.TodoElDia && resultado.Fin <= resultado.Inicio))
            resultado.RegistrarError(nameof(entrada.FechaFin),
                "La fecha y hora final deben ser posteriores a las de inicio.");
        return resultado;
    }
}