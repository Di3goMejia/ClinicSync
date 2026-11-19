using System.Globalization;
using System.Numerics;

namespace ClinicSyncDesktop.Validations;

public static class ReglasExtensiones
{
    public static ReglaCampo<string?> CorreoValido(
        this ReglaCampo<string?> campo,
        string mensaje)
    {
        if (!string.IsNullOrWhiteSpace(campo.Valor) &&
            !System.Net.Mail.MailAddress.TryCreate(campo.Valor.Trim(), out _))
            campo.AgregarError(mensaje);

        return campo;
    }

    public static ReglaCampo<string?> CodigoNumerico(
        this ReglaCampo<string?> campo,
        int longitud,
        string mensaje)
    {
        if (!string.IsNullOrWhiteSpace(campo.Valor) &&
            (campo.Valor.Length != longitud || !campo.Valor.All(char.IsDigit)))
            campo.AgregarError(mensaje);
        return campo;
    }

    public static ReglaCampo<string?> ComplejidadPassword(
        this ReglaCampo<string?> campo,
        string mensaje)
    {
        if (!string.IsNullOrWhiteSpace(campo.Valor) &&
            (campo.Valor.Length < 8 || !campo.Valor.Any(char.IsLetter) || !campo.Valor.Any(char.IsDigit)))
            campo.AgregarError(mensaje);
        return campo;
    }

    public static ReglaCampo<string?> IgualA(
        this ReglaCampo<string?> campo,
        string esperado,
        string mensaje)
    {
        if (campo.Valor != esperado)
            campo.AgregarError(mensaje);
        return campo;
    }

    public static ReglaCampo<T> ValorNoNegativo<T>(
        this ReglaCampo<T> campo,
        string mensaje) where T : INumber<T>
    {
        if (campo.Valor < T.Zero)
            campo.AgregarError(mensaje);

        return campo;
    }

    public static ReglaCampo<DateTime> NoFuturo(
        this ReglaCampo<DateTime> campo,
        string mensaje)
    {
        if (campo.Valor > DateTime.Now)
            campo.AgregarError(mensaje);

        return campo;
    }

    public static ReglaCampo<T?> Seleccionado<T>(
        this ReglaCampo<T?> campo,
        string mensaje) where T : class
    {
        if (campo.Valor is null)
            campo.AgregarError(mensaje);

        return campo;
    }

    public static ReglaCampo<string?> Requerido(
        this ReglaCampo<string?> campo,
        string mensaje)
    {
        if (string.IsNullOrWhiteSpace(campo.Valor))
            campo.AgregarError(mensaje);

        return campo;
    }


    public static ReglaCampo<string?> LongitudMinima(
        this ReglaCampo<string?> campo,
        int minimo,
        string mensaje)
    {
        if (!string.IsNullOrWhiteSpace(campo.Valor) && campo.Valor.Trim().Length < minimo)
            campo.AgregarError(mensaje);

        return campo;
    }

    public static ReglaCampo<string?> LongitudMaxima(
        this ReglaCampo<string?> campo,
        int maximo,
        string mensaje)
    {
        if (!string.IsNullOrWhiteSpace(campo.Valor) && campo.Valor.Trim().Length > maximo)
            campo.AgregarError(mensaje);

        return campo;
    }

    public static ReglaCampo<T> ValorPositivo<T>(
        this ReglaCampo<T> campo,
        string mensaje) where T : INumber<T>
    {
        if (campo.Valor <= T.Zero)
            campo.AgregarError(mensaje);

        return campo;
    }

    public static ReglaCampo<T> ValorMaximo<T>(
        this ReglaCampo<T> campo,
        T maximo,
        string mensaje) where T : INumber<T>
    {
        if (campo.Valor > maximo)
            campo.AgregarError(mensaje);

        return campo;
    }

    public static ReglaCampo<decimal> MaximoDecimales(
        this ReglaCampo<decimal> campo,
        int maxDecimales,
        string mensaje)
    {
        if (decimal.Round(campo.Valor, maxDecimales) != campo.Valor)
            campo.AgregarError(mensaje);

        return campo;
    }

    public static ReglaCampo<string?> DecimalValido(
        this ReglaCampo<string?> campo,
        out decimal valor,
        string mensaje = "El valor ingresado no es un número decimal válido.")
    {
        valor = 0m;

        if (string.IsNullOrWhiteSpace(campo.Valor))
            return campo;

        string normalizado = campo.Valor.Replace(',', '.');

        if (!decimal.TryParse(
            normalizado,
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out valor))
        {
            campo.AgregarError(mensaje);
        }

        return campo;
    }
}