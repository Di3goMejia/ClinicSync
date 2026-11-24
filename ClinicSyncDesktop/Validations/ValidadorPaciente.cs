using System.Globalization;
using System.Text.RegularExpressions;

namespace ClinicSyncDesktop.Validations;

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.IEquatable&lt;ClinicSyncDesktop.Validations.EntradaPaciente&gt;" />
public record EntradaPaciente(
    string Identidad,
    string Nombres,
    string Apellidos,
    string TextoFechaNacimiento,
    string? Sexo,
    string Telefono);

/// <summary>
/// 
/// </summary>
/// <seealso cref="ClinicSyncDesktop.Validations.ResultadoValidacion" />
public sealed class ResultadoValidacionPaciente : ResultadoValidacion
{
    /// <summary>
    /// Gets or sets the identidad normalizada.
    /// </summary>
    /// <value>
    /// The identidad normalizada.
    /// </value>
    public string IdentidadNormalizada { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the nombres normalizados.
    /// </summary>
    /// <value>
    /// The nombres normalizados.
    /// </value>
    public string NombresNormalizados { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the apellidos normalizados.
    /// </summary>
    /// <value>
    /// The apellidos normalizados.
    /// </value>
    public string ApellidosNormalizados { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the telefono normalizado.
    /// </summary>
    /// <value>
    /// The telefono normalizado.
    /// </value>
    public string TelefonoNormalizado { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the fecha nacimiento.
    /// </summary>
    /// <value>
    /// The fecha nacimiento.
    /// </value>
    public DateTime FechaNacimiento { get; set; }
}

/// <summary>
/// 
/// </summary>
/// <seealso cref="ClinicSyncDesktop.Validations.IValidador&lt;ClinicSyncDesktop.Validations.EntradaPaciente, ClinicSyncDesktop.Validations.ResultadoValidacionPaciente&gt;" />
public sealed class ValidadorPaciente : IValidador<EntradaPaciente, ResultadoValidacionPaciente>
{
    /// <summary>
    /// The patron nombre
    /// </summary>
    private const string PatronNombre = @"^[\p{L}]+(?:[ '\-][\p{L}]+)*$";
    /// <summary>
    /// The patron identidad
    /// </summary>
    private const string PatronIdentidad = @"^(\d{13}|\d{4}-\d{4}-\d{5}|\d{4} \d{4} \d{5})$";
    /// <summary>
    /// The patron telefono
    /// </summary>
    private const string PatronTelefono = @"^(\d{8}|\d{4}-\d{4})$";

    /// <summary>
    /// Validars the specified entrada.
    /// </summary>
    /// <param name="entrada">The entrada.</param>
    /// <returns></returns>
    public ResultadoValidacionPaciente Validar(EntradaPaciente entrada)
    {
        var resultado = new ResultadoValidacionPaciente
        {
            IdentidadNormalizada = Regex.Replace(entrada.Identidad.Trim(), @"[ -]", ""),
            NombresNormalizados = NormalizarTexto(entrada.Nombres),
            ApellidosNormalizados = NormalizarTexto(entrada.Apellidos),
            TelefonoNormalizado = entrada.Telefono.Trim().Replace("-", "")
        };

        resultado.Campo<string?>(nameof(entrada.Identidad), entrada.Identidad)
            .Requerido("Ingresa una identidad.");
        if (!string.IsNullOrWhiteSpace(entrada.Identidad) &&
            !Regex.IsMatch(entrada.Identidad.Trim(), PatronIdentidad))
            resultado.RegistrarError(nameof(entrada.Identidad), "Ingresa una identidad válida de 13 dígitos.");

        ValidarNombre(resultado, nameof(entrada.Nombres), resultado.NombresNormalizados,
            "Ingresa los nombres del paciente.", "Ingresa nombres válidos.");
        ValidarNombre(resultado, nameof(entrada.Apellidos), resultado.ApellidosNormalizados,
            "Ingresa los apellidos del paciente.", "Ingresa apellidos válidos.");

        resultado.Campo<string?>(nameof(entrada.TextoFechaNacimiento), entrada.TextoFechaNacimiento)
            .Requerido("Ingresa una fecha de nacimiento.");
        if (!DateTime.TryParseExact(entrada.TextoFechaNacimiento.Trim(), "yyyy/MM/dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
            resultado.RegistrarError(nameof(entrada.TextoFechaNacimiento), "Ingresa una fecha válida (YYYY/MM/DD).");
        else
        {
            resultado.FechaNacimiento = fecha.Date;
            if (fecha.Year < 1900 || fecha.Date > DateTime.Today)
                resultado.RegistrarError(nameof(entrada.TextoFechaNacimiento), "Ingresa una fecha de nacimiento válida.");
        }

        if (entrada.Sexo is not ("F" or "M"))
            resultado.RegistrarError(nameof(entrada.Sexo), "Selecciona una opción de sexo.");

        resultado.Campo<string?>(nameof(entrada.Telefono), entrada.Telefono)
            .Requerido("Ingresa un teléfono.");
        if (!string.IsNullOrWhiteSpace(entrada.Telefono) &&
            !Regex.IsMatch(entrada.Telefono.Trim(), PatronTelefono))
            resultado.RegistrarError(nameof(entrada.Telefono), "Ingresa un teléfono válido de 8 dígitos.");

        return resultado;
    }

    /// <summary>
    /// Validars the nombre.
    /// </summary>
    /// <param name="resultado">The resultado.</param>
    /// <param name="campo">The campo.</param>
    /// <param name="valor">The valor.</param>
    /// <param name="requerido">The requerido.</param>
    /// <param name="invalido">The invalido.</param>
    private static void ValidarNombre(ResultadoValidacion resultado, string campo,
        string valor, string requerido, string invalido)
    {
        resultado.Campo<string?>(campo, valor).Requerido(requerido);
        if (!string.IsNullOrWhiteSpace(valor) &&
            (valor.Length > 50 || !Regex.IsMatch(valor, PatronNombre)))
            resultado.RegistrarError(campo, invalido);
    }

    /// <summary>
    /// Normalizars the texto.
    /// </summary>
    /// <param name="valor">The valor.</param>
    /// <returns></returns>
    private static string NormalizarTexto(string valor) =>
        Regex.Replace(valor.Trim(), @"\s+", " ");
}