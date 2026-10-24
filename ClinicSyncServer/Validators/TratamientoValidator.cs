namespace ClinicSyncServer.Validators
{
    public static class TratamientoValidator
    {
        public static string? Validar(
            string? nombre,
            decimal precio,
            string? descripcion = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return "El nombre del tratamiento es obligatorio.";
            }

            if (nombre.Trim().Length > 100)
            {
                return "El nombre no puede superar los 100 caracteres.";
            }

            if (precio < 0)
            {
                return "El precio no puede ser negativo.";
            }

            if (precio > 99999999.99m)
            {
                return "El precio supera el límite permitido.";
            }

            if (descripcion?.Length > 200)
            {
                return "La descripción no puede superar los 200 caracteres.";
            }

            return null;
        }
    }
}
