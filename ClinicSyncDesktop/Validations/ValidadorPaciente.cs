using System.Globalization;
using System.Text.RegularExpressions;

namespace ClinicSyncDesktop.Validations;

public record EntradaPaciente(
    string Identidad,
    string Nombres,
    string Apellidos,
    string TextoFechaNacimiento,
    string? Sexo,
    string Telefono);

public sealed class ResultadoValidacionPaciente : ResultadoValidacion
{
    public string IdentidadNormalizada { get; set; } = string.Empty;
    public string NombresNormalizados { get; set; } = string.Empty;
    public string ApellidosNormalizados { get; set; } = string.Empty;
    public string TelefonoNormalizado { get; set; } = string.Empty;
    public DateTime FechaNacimiento { get; set; }
}

public sealed class ValidadorPaciente : IValidador<EntradaPaciente, ResultadoValidacionPaciente>
{
    private const string PatronNombre = @"^[\p{L}]+(?:[ '\-][\p{L}]+)*$";
    private const string PatronIdentidad = @"^(\d{13}|\d{4}-\d{4}-\d{5}|\d{4} \d{4} \d{5})$";
    private const string PatronTelefono = @"^(\d{8}|\d{4}-\d{4})$";

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

    private static void ValidarNombre(ResultadoValidacion resultado, string campo,
        string valor, string requerido, string invalido)
    {
        resultado.Campo<string?>(campo, valor).Requerido(requerido);
        if (!string.IsNullOrWhiteSpace(valor) &&
            (valor.Length > 50 || !Regex.IsMatch(valor, PatronNombre)))
            resultado.RegistrarError(campo, invalido);
    }

    private static string NormalizarTexto(string valor) =>
        Regex.Replace(valor.Trim(), @"\s+", " ");
}