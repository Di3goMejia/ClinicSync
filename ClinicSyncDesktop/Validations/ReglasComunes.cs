using System.Globalization;
using System.Numerics;

namespace ClinicSyncDesktop.Validations;

/// <summary>
/// 
/// </summary>
public static class ReglasExtensiones
{
    /// <summary>
    /// Correoes the valido.
    /// </summary>
    /// <param name="campo">The campo.</param>
    /// <param name="mensaje">The mensaje.</param>
    /// <returns></returns>
    public static ReglaCampo<string?> CorreoValido(
        this ReglaCampo<string?> campo,
        string mensaje)
    {
        if (!string.IsNullOrWhiteSpace(campo.Valor) &&
            !System.Net.Mail.MailAddress.TryCreate(campo.Valor.Trim(), out _))
            campo.AgregarError(mensaje);

        return campo;
    }

    /// <summary>
    /// Codigoes the numerico.
    /// </summary>
    /// <param name="campo">The campo.</param>
    /// <param name="longitud">The longitud.</param>
    /// <param name="mensaje">The mensaje.</param>
    /// <returns></returns>
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

    /// <summary>
    /// Complejidads the password.
    /// </summary>
    /// <param name="campo">The campo.</param>
    /// <param name="mensaje">The mensaje.</param>
    /// <returns></returns>
    public static ReglaCampo<string?> ComplejidadPassword(
        this ReglaCampo<string?> campo,
        string mensaje)
    {
        if (!string.IsNullOrWhiteSpace(campo.Valor) &&
            (campo.Valor.Length < 8 || !campo.Valor.Any(char.IsLetter) || !campo.Valor.Any(char.IsDigit)))
            campo.AgregarError(mensaje);
        return campo;
    }

    /// <summary>
    /// Iguals a.
    /// </summary>
    /// <param name="campo">The campo.</param>
    /// <param name="esperado">The esperado.</param>
    /// <param name="mensaje">The mensaje.</param>
    /// <returns></returns>
    public static ReglaCampo<string?> IgualA(
        this ReglaCampo<string?> campo,
        string esperado,
        string mensaje)
    {
        if (campo.Valor != esperado)
            campo.AgregarError(mensaje);
        return campo;
    }

    /// <summary>
    /// Valors the no negativo.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="campo">The campo.</param>
    /// <param name="mensaje">The mensaje.</param>
    /// <returns></returns>
    public static ReglaCampo<T> ValorNoNegativo<T>(
        this ReglaCampo<T> campo,
        string mensaje) where T : INumber<T>
    {
        if (campo.Valor < T.Zero)
            campo.AgregarError(mensaje);

        return campo;
    }

    /// <summary>
    /// Noes the futuro.
    /// </summary>
    /// <param name="campo">The campo.</param>
    /// <param name="mensaje">The mensaje.</param>
    /// <returns></returns>
    public static ReglaCampo<DateTime> NoFuturo(
        this ReglaCampo<DateTime> campo,
        string mensaje)
    {
        if (campo.Valor > DateTime.Now)
            campo.AgregarError(mensaje);

        return campo;
    }

    /// <summary>
    /// Seleccionadoes the specified mensaje.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="campo">The campo.</param>
    /// <param name="mensaje">The mensaje.</param>
    /// <returns></returns>
    public static ReglaCampo<T?> Seleccionado<T>(
        this ReglaCampo<T?> campo,
        string mensaje) where T : class
    {
        if (campo.Valor is null)
            campo.AgregarError(mensaje);

        return campo;
    }

    /// <summary>
    /// Requeridoes the specified mensaje.
    /// </summary>
    /// <param name="campo">The campo.</param>
    /// <param name="mensaje">The mensaje.</param>
    /// <returns></returns>
    public static ReglaCampo<string?> Requerido(
        this ReglaCampo<string?> campo,
        string mensaje)
    {
        if (string.IsNullOrWhiteSpace(campo.Valor))
            campo.AgregarError(mensaje);

        return campo;
    }


    /// <summary>
    /// Longituds the minima.
    /// </summary>
    /// <param name="campo">The campo.</param>
    /// <param name="minimo">The minimo.</param>
    /// <param name="mensaje">The mensaje.</param>
    /// <returns></returns>
    public static ReglaCampo<string?> LongitudMinima(
        this ReglaCampo<string?> campo,
        int minimo,
        string mensaje)
    {
        if (!string.IsNullOrWhiteSpace(campo.Valor) && campo.Valor.Trim().Length < minimo)
            campo.AgregarError(mensaje);

        return campo;
    }

    /// <summary>
    /// Longituds the maxima.
    /// </summary>
    /// <param name="campo">The campo.</param>
    /// <param name="maximo">The maximo.</param>
    /// <param name="mensaje">The mensaje.</param>
    /// <returns></returns>
    public static ReglaCampo<string?> LongitudMaxima(
        this ReglaCampo<string?> campo,
        int maximo,
        string mensaje)
    {
        if (!string.IsNullOrWhiteSpace(campo.Valor) && campo.Valor.Trim().Length > maximo)
            campo.AgregarError(mensaje);

        return campo;
    }

    /// <summary>
    /// Valors the positivo.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="campo">The campo.</param>
    /// <param name="mensaje">The mensaje.</param>
    /// <returns></returns>
    public static ReglaCampo<T> ValorPositivo<T>(
        this ReglaCampo<T> campo,
        string mensaje) where T : INumber<T>
    {
        if (campo.Valor <= T.Zero)
            campo.AgregarError(mensaje);

        return campo;
    }

    /// <summary>
    /// Valors the maximo.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="campo">The campo.</param>
    /// <param name="maximo">The maximo.</param>
    /// <param name="mensaje">The mensaje.</param>
    /// <returns></returns>
    public static ReglaCampo<T> ValorMaximo<T>(
        this ReglaCampo<T> campo,
        T maximo,
        string mensaje) where T : INumber<T>
    {
        if (campo.Valor > maximo)
            campo.AgregarError(mensaje);

        return campo;
    }

    /// <summary>
    /// Maximoes the decimales.
    /// </summary>
    /// <param name="campo">The campo.</param>
    /// <param name="maxDecimales">The maximum decimales.</param>
    /// <param name="mensaje">The mensaje.</param>
    /// <returns></returns>
    public static ReglaCampo<decimal> MaximoDecimales(
        this ReglaCampo<decimal> campo,
        int maxDecimales,
        string mensaje)
    {
        if (decimal.Round(campo.Valor, maxDecimales) != campo.Valor)
            campo.AgregarError(mensaje);

        return campo;
    }

    /// <summary>
    /// Decimals the valido.
    /// </summary>
    /// <param name="campo">The campo.</param>
    /// <param name="valor">The valor.</param>
    /// <param name="mensaje">The mensaje.</param>
    /// <returns></returns>
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