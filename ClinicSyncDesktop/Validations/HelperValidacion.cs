using System.Windows.Controls;

namespace ClinicSyncDesktop.Validations;

public static class HelperValidacion
{
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
