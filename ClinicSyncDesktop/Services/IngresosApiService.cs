using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Models;
using System;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;


namespace ClinicSyncDesktop.Services
{
    public class IngresosApiService
    {


        public async Task ActualizarIngresoAsync(
            int idIngreso,
            EditarIngresoRequest request)
        {
            if (idIngreso <= 0)
                throw new ArgumentException(
                    "El identificador del ingreso no es válido.");

            using HttpResponseMessage respuesta =
                await ClienteApi.PutAutenticadoAsync(
                    $"api/finanzas/ingresos/{idIngreso}",
                    request);

            if (!respuesta.IsSuccessStatusCode)
            {
                string detalle =
                    await respuesta.Content.ReadAsStringAsync();

                throw new InvalidOperationException(
                    $"No se pudieron guardar los cambios. " +
                    $"HTTP {(int)respuesta.StatusCode}: {detalle}");
            }
        }


        public async Task<EditarIngresoDetalle> ObtenerIngresoPorIdAsync(
            int idIngreso)
        {
            if (idIngreso <= 0)
                throw new ArgumentException(
                    "El identificador del ingreso no es válido.");

            var ingreso = await ClienteApi.ObtenerAutenticadoAsync<
                EditarIngresoDetalle>(
                    $"api/finanzas/ingresos/{idIngreso}");

            if (ingreso == null)
                throw new InvalidOperationException(
                    "No se encontró el ingreso solicitado.");

            return ingreso;
        }

        public Task<PacienteIngreso[]> ObtenerPacientesAsync()
        {
            return ClienteApi.ObtenerAutenticadoAsync<
                PacienteIngreso[]>("api/pacientes");
        }

        public Task<TratamientoIngreso[]>
            ObtenerTratamientosAsync(int idPaciente, DateTime fecha)
        {
            string ruta =
                $"api/finanzas/tratamientos-disponibles" +
                $"?idPaciente={idPaciente}" +
                $"&fecha={fecha:yyyy-MM-dd}";

            return ClienteApi.ObtenerAutenticadoAsync<
                TratamientoIngreso[]>(ruta);
        }

        public Task<MaterialIngreso[]> ObtenerMaterialesAsync()
        {
            return ClienteApi.ObtenerAutenticadoAsync<
                MaterialIngreso[]>("api/finanzas/materiales");
        }

        public Task<IngresoListado[]> ObtenerIngresosAsync()
        {
            return ClienteApi.ObtenerAutenticadoAsync<
                IngresoListado[]>("api/finanzas/ingresos");
        }

        public async Task GuardarIngresoAsync(
            RegistrarIngresoRequest request)
        {
            using HttpResponseMessage respuesta =
                await ClienteApi.PostAutenticadoAsync(
                    "api/finanzas/ingresos", request);

            if (!respuesta.IsSuccessStatusCode)
            {
                string detalle =
                    await respuesta.Content.ReadAsStringAsync();

                throw new InvalidOperationException(
                    $"HTTP {(int)respuesta.StatusCode} " +
                    $"({respuesta.ReasonPhrase}). " +
                    $"Detalle: {detalle}");
            }
        }

    }
}

