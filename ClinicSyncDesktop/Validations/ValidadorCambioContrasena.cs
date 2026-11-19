namespace ClinicSyncDesktop.Validations;

public record EntradaCambioContrasena(string Actual, string Nueva, string Confirmacion);

public sealed class ValidadorCambioContrasena : IValidador<EntradaCambioContrasena, ResultadoValidacion>
{
    public ResultadoValidacion Validar(EntradaCambioContrasena entrada)
    {
        var resultado = new ResultadoValidacion();
        resultado.Campo<string?>(nameof(entrada.Actual), entrada.Actual)
            .Requerido("Ingresa la contraseña actual.");
        resultado.Campo<string?>(nameof(entrada.Nueva), entrada.Nueva)
            .Requerido("Ingresa una contraseña nueva.");

        if (!string.IsNullOrEmpty(entrada.Nueva))
        {
            if (entrada.Nueva.Length < 8 || !entrada.Nueva.Any(char.IsUpper) ||
                !entrada.Nueva.Any(char.IsLower) || !entrada.Nueva.Any(char.IsDigit) ||
                !entrada.Nueva.Any(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)))
                resultado.RegistrarError(nameof(entrada.Nueva), "La contraseña no cumple los requisitos.");
            if (entrada.Actual == entrada.Nueva)
                resultado.RegistrarError(nameof(entrada.Nueva), "La contraseña nueva debe ser diferente.");
        }

        resultado.Campo<string?>(nameof(entrada.Confirmacion), entrada.Confirmacion)
            .Requerido("Confirma la contraseña nueva.")
            .IgualA(entrada.Nueva, "Las contraseñas no coinciden.");
        return resultado;
    }
}