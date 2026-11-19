namespace ClinicSyncDesktop.Validations;

public record EntradaUsuario(
    string Identidad, string Nombres, string Apellidos, string Telefono,
    DateTime? FechaNacimiento, object? Genero, object? Rol,
    string NombreUsuario, string Correo);

public class ValidadorUsuario : IValidador<EntradaUsuario, ResultadoValidacion>
{
    public ResultadoValidacion Validar(EntradaUsuario entrada)
    {
        var resultado = new ResultadoValidacion();
        ValidarDatosPersonales(entrada, resultado);
        ValidarCredenciales(entrada, resultado);
        return resultado;
    }

    public ResultadoValidacion ValidarDatosPersonales(EntradaUsuario entrada)
    {
        var resultado = new ResultadoValidacion();
        ValidarDatosPersonales(entrada, resultado);
        return resultado;
    }

    public ResultadoValidacion ValidarCredenciales(EntradaUsuario entrada)
    {
        var resultado = new ResultadoValidacion();
        ValidarCredenciales(entrada, resultado);
        return resultado;
    }

    private static void ValidarDatosPersonales(EntradaUsuario entrada, ResultadoValidacion resultado)
    {
        resultado.Campo<string?>(nameof(entrada.Identidad), entrada.Identidad).Requerido("Ingresa la identidad.");
        resultado.Campo<string?>(nameof(entrada.Nombres), entrada.Nombres).Requerido("Ingresa los nombres.");
        resultado.Campo<string?>(nameof(entrada.Apellidos), entrada.Apellidos).Requerido("Ingresa los apellidos.");
        resultado.Campo<string?>(nameof(entrada.Telefono), entrada.Telefono).Requerido("Ingresa el teléfono.");
        if (entrada.FechaNacimiento is null)
            resultado.RegistrarError(nameof(entrada.FechaNacimiento), "Selecciona la fecha de nacimiento.");
        if (entrada.Genero is null)
            resultado.RegistrarError(nameof(entrada.Genero), "Selecciona el género.");
        if (entrada.Rol is null)
            resultado.RegistrarError(nameof(entrada.Rol), "Selecciona un rol.");
        ValidarFecha(entrada, resultado);
    }

    private static void ValidarCredenciales(EntradaUsuario entrada, ResultadoValidacion resultado)
    {
        resultado.Campo<string?>(nameof(entrada.NombreUsuario), entrada.NombreUsuario).Requerido("Indica un nombre de usuario.");
        resultado.Campo<string?>(nameof(entrada.Correo), entrada.Correo)
            .Requerido("Indica un correo válido.")
            .CorreoValido("Indica un correo válido.");
    }

    private static void ValidarFecha(EntradaUsuario entrada, ResultadoValidacion resultado)
    {
        if (entrada.FechaNacimiento is DateTime fecha)
            resultado.Campo(nameof(entrada.FechaNacimiento), fecha).NoFuturo("La fecha de nacimiento no puede ser futura.");
    }
}