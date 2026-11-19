namespace ClinicSyncDesktop.Validations;

public record EntradaRol(string Nombre, string? Descripcion);

public class ValidadorRol: IValidador<EntradaRol, ResultadoValidacion>
{
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
}
