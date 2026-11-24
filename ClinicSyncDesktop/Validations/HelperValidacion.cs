using System.Windows.Controls;

namespace ClinicSyncDesktop.Validations;

/// <summary>
/// 
/// </summary>
public static class HelperValidacion
{
    /// <summary>
    /// Aplicars the resultado.
    /// </summary>
    /// <param name="resultado">The resultado.</param>
    /// <param name="controles">The controles.</param>
    /// <param name="mostrarError">The mostrar error.</param>
    /// <param name="ocultarError">The ocultar error.</param>
    /// <returns></returns>
    public static bool AplicarResultado(
        ResultadoValidacion resultado,
        IReadOnlyDictionary<string, Control> controles,
        Action<string> mostrarError,
        Action ocultarError
    )
    {
        ocultarError();

        if (resultado.EsValido)
            return true;

        var primerError = resultado.Errores.FirstOrDefault();
        if (primerError is not null)
        {
            mostrarError(primerError.Mensaje);

            if (controles.TryGetValue(primerError.Campo, out var control))
            {
                control.Focus();
            }
        }

        return false;

    }
}
