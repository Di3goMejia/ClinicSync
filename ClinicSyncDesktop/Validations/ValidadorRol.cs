namespace ClinicSyncDesktop.Validations;

/// <summary>
/// 
/// </summary>
/// <seealso cref="System.IEquatable&lt;ClinicSyncDesktop.Validations.EntradaRol&gt;" />
public record EntradaRol(string Nombre, string? Descripcion);

/// <summary>
/// 
/// </summary>
/// <seealso cref="ClinicSyncDesktop.Validations.IValidador&lt;ClinicSyncDesktop.Validations.EntradaRol, ClinicSyncDesktop.Validations.ResultadoValidacion&gt;" />
public class ValidadorRol: IValidador<EntradaRol, ResultadoValidacion>
{
    /// <summary>
    /// Validars the specified entrada.
    /// </summary>
    /// <param name="entrada">The entrada.</param>
    /// <returns></returns>
    public ResultadoValidacion Validar(EntradaRol entrada)
    {
        var resultado = new ResultadoValidacion();

        // NOMBRE
        resultado.Campo<string?>(nameof(entrada.Nombre), entrada.Nombre)
            .Requerido("El nombre del rol es obligatorio.")
            .LongitudMinima(2, "El nombre del rol debe tener entre 2 y 100 caracteres.")
            .LongitudMaxima(100, "El nombre del rol debe tener entre 2 y 100 caracteres.");

        return resultado;
    }

    /// <summary>
    /// Eses the administrador.
    /// </summary>
    /// <param name="rol">The rol.</param>
    /// <returns></returns>
    public static bool EsAdministrador(string? rol)
    {
        return string.Equals(
            rol?.Trim(),
            "ADMINISTRADOR",
            System.StringComparison.OrdinalIgnoreCase);
    }
}
