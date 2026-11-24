namespace ClinicSyncDesktop.Validations;

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.IEquatable&lt;ClinicSyncDesktop.Validations.EntradaNuevaContrasena&gt;" />
public record EntradaNuevaContrasena(string Nueva, string Confirmacion);

/// <summary>
/// 
/// </summary>
public sealed class ValidadorNuevaContrasena
{
    /// <summary>
    /// Validars the nueva.
    /// </summary>
    /// <param name="nueva">The nueva.</param>
    /// <returns></returns>
    public ResultadoValidacion ValidarNueva(string nueva)
    {
        var resultado = new ResultadoValidacion();
        resultado.Campo<string?>(nameof(EntradaNuevaContrasena.Nueva), nueva)
            .Requerido("La nueva contraseña es obligatoria.");
        if (!string.IsNullOrWhiteSpace(nueva))
        {
            if (nueva.Length < 8)
                resultado.RegistrarError(nameof(EntradaNuevaContrasena.Nueva), "La contraseña debe tener 8 caracteres o más.");
            else if (!nueva.Any(char.IsLetter) || !nueva.Any(char.IsDigit))
                resultado.RegistrarError(nameof(EntradaNuevaContrasena.Nueva), "La contraseña debe contener letras y números.");
        }
        return resultado;
    }

    /// <summary>
    /// Validars the confirmacion.
    /// </summary>
    /// <param name="entrada">The entrada.</param>
    /// <returns></returns>
    public ResultadoValidacion ValidarConfirmacion(EntradaNuevaContrasena entrada)
    {
        var resultado = new ResultadoValidacion();
        resultado.Campo<string?>(nameof(entrada.Confirmacion), entrada.Confirmacion)
            .Requerido("Debe confirmar la contraseña.")
            .IgualA(entrada.Nueva, "Las contraseñas no coinciden.");
        return resultado;
    }
}
