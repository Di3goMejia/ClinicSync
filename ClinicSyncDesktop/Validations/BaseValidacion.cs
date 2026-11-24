namespace ClinicSyncDesktop.Validations;

/// <summary>
/// 
/// </summary>
public class ErrorValidacion
{
    /// <summary>
    /// Gets or sets the campo.
    /// </summary>
    /// <value>
    /// The campo.
    /// </value>
    public string Campo { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the mensaje.
    /// </summary>
    /// <value>
    /// The mensaje.
    /// </value>
    public string Mensaje { get; set; } = string.Empty;
}

/// <summary>
/// 
/// </summary>
public class ResultadoValidacion 
{
    /// <summary>
    /// Gets the errores.
    /// </summary>
    /// <value>
    /// The errores.
    /// </value>
    public List<ErrorValidacion> Errores { get; } = [];
    /// <summary>
    /// Gets a value indicating whether [es valido].
    /// </summary>
    /// <value>
    ///   <c>true</c> if [es valido]; otherwise, <c>false</c>.
    /// </value>
    public bool EsValido => Errores.Count == 0;

    /// <summary>
    /// Registrars the error.
    /// </summary>
    /// <param name="campo">The campo.</param>
    /// <param name="mensaje">The mensaje.</param>
    public void RegistrarError(string campo, string mensaje)
    {
        Errores.Add(new ErrorValidacion { Campo = campo, Mensaje = mensaje });
    }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TEntrada">The type of the entrada.</typeparam>
/// <typeparam name="TResultado">The type of the resultado.</typeparam>
public interface IValidador<in TEntrada, out TResultado>
    where TResultado : ResultadoValidacion
{
    /// <summary>
    /// Validars the specified entrada.
    /// </summary>
    /// <param name="entrada">The entrada.</param>
    /// <returns></returns>
    TResultado Validar(TEntrada entrada);
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct ReglaCampo<T>(ResultadoValidacion resultado, string nombreCampo, T valor)
{
    public ResultadoValidacion Resultado { get; } = resultado;
    public string NombreCampo { get; } = nombreCampo;
    public T Valor { get; } = valor;

    public ReglaCampo<T> AgregarError(string mensaje)
    {
        Resultado.RegistrarError(NombreCampo, mensaje);
        return this;
    }
}

public static class ValidacionExtensions
{
    public static ReglaCampo<T> Campo<T>(
        this ResultadoValidacion resultado,
        string nombreCampo,
        T valor
    ) 
        => new(resultado, nombreCampo, valor);
}
