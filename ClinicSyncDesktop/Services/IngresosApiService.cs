using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Models;
using System;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;


namespace ClinicSyncDesktop.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class IngresosApiService
    {


        /// <summary>
        /// Actualizars the ingreso asynchronous.
        /// </summary>
        /// <param name="idIngreso">The identifier ingreso.</param>
        /// <param name="request">The request.</param>
        /// <exception cref="ArgumentException">El identificador del ingreso no es válido.</exception>
        /// <exception cref="InvalidOperationException">No se pudieron guardar los cambios. " +
        ///                     $"HTTP {(int)respuesta.StatusCode}: {detalle}</exception>
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


        /// <summary>
        /// Obteners the ingreso por identifier asynchronous.
        /// </summary>
        /// <param name="idIngreso">The identifier ingreso.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">El identificador del ingreso no es válido.</exception>
        /// <exception cref="InvalidOperationException">No se encontró el ingreso solicitado.</exception>
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

        /// <summary>
        /// Obteners the pacientes asynchronous.
        /// </summary>
        /// <returns></returns>
        public Task<PacienteIngreso[]> ObtenerPacientesAsync()
        {
            return ClienteApi.ObtenerAutenticadoAsync<
                PacienteIngreso[]>("api/pacientes");
        }

        /// <summary>
        /// Obteners the tratamientos asynchronous.
        /// </summary>
        /// <param name="idPaciente">The identifier paciente.</param>
        /// <param name="fecha">The fecha.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Obteners the tratamientos catalogo asynchronous.
        /// </summary>
        /// <returns></returns>
        public Task<TratamientoIngreso[]> ObtenerTratamientosCatalogoAsync()
        {
            return ClienteApi.ObtenerAutenticadoAsync<TratamientoIngreso[]>(
                "api/finanzas/tratamientos-catalogo");
        }

        /// <summary>
        /// Obteners the materiales asynchronous.
        /// </summary>
        /// <returns></returns>
        public Task<MaterialIngreso[]> ObtenerMaterialesAsync()
        {
            return ClienteApi.ObtenerAutenticadoAsync<
                MaterialIngreso[]>("api/finanzas/materiales");
        }

        /// <summary>
        /// Obteners the ingresos asynchronous.
        /// </summary>
        /// <returns></returns>
        public Task<IngresoListado[]> ObtenerIngresosAsync()
        {
            return ClienteApi.ObtenerAutenticadoAsync<
                IngresoListado[]>("api/finanzas/ingresos");
        }

        /// <summary>
        /// Guardars the ingreso asynchronous.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <exception cref="InvalidOperationException">HTTP {(int)respuesta.StatusCode} " +
        ///                     $"({respuesta.ReasonPhrase}). " +
        ///                     $"Detalle: {detalle}</exception>
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

