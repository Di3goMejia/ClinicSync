namespace ClinicSyncDesktop.Api.Tratamientos
{
    public class ClienteTratamientos(ClienteApi api)
    {
        public async Task<IEnumerable<Tratamiento>?> ListarAsync(string? buscar = null, bool? activo = null)
        {
            var ruta = QueryString.Crear("api/tratamientos",
                [
                    new("buscar", buscar),
                    new("activo", activo.ToString())
                ]
            );

            return await api.GetAsync<IEnumerable<Tratamiento>>(ruta);
        }

        public async Task<RegistrarTratamientoResponse?> InsertarAsync(RegistrarTratamientoRequest req) =>
            await api.PostAsync<RegistrarTratamientoResponse>("api/tratamientos", req);

        public async Task<Tratamiento?> ObtenerPorIdAsync(int id) =>
            await api.GetAsync<Tratamiento>($"api/tratamientos/{id}");

        public async Task<Tratamiento?> ModificarAsync(int id, RegistrarTratamientoRequest req) =>
            await api.PutAsync<Tratamiento>($"api/tratamientos/{id}", req);

        public async Task CambiarEstadoAsync(int id, bool activo) =>
            await api.PatchAsync<object>($"api/tratamientos/{id}/estado", 
                new CambiarEstadoTratamientoRequest(activo));
    }
}
