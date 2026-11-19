namespace ClinicSyncDesktop.Validations;

public class ErrorValidacion
{
    public string Campo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
}

public class ResultadoValidacion 
{
    public List<ErrorValidacion> Errores { get; } = [];
    public bool EsValido => Errores.Count == 0;

    public void RegistrarError(string campo, string mensaje)
    {
        Errores.Add(new ErrorValidacion { Campo = campo, Mensaje = mensaje });
    }
}

public interface IValidador<in TEntrada, out TResultado>
    where TResultado : ResultadoValidacion
{
    TResultado Validar(TEntrada entrada);
}

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
