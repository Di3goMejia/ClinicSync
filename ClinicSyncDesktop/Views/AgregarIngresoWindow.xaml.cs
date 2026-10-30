
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSyncDesktop.Views
{
    public partial class AgregarIngresoWindow : Window
    {
        private readonly IngresosApiService servicio = new();

        private List<PacienteIngreso> catalogoPacientes = new();
        private List<TratamientoIngreso> catalogoTratamientos = new();
        private List<MaterialIngreso> catalogoMateriales = new();

        private PacienteIngreso? pacienteSeleccionado;
        private List<TratamientoIngreso> tratamientosElegidos = new();
        private List<MaterialConsumido> materialesElegidos = new();

        private bool guardando;
        private bool inicializado;
        private bool cargandoDatos;

        public AgregarIngresoWindow()
        {
            InitializeComponent();

            DpFecha.SelectedDate = DateTime.Today;
            inicializado = true;

            Loaded += AgregarIngresoWindow_Loaded;
        }

        // CARGAR CATÁLOGOS
        private async void AgregarIngresoWindow_Loaded(
            object sender, RoutedEventArgs e)
        {
            if (cargandoDatos)
                return;

            cargandoDatos = true;
            ErrorGeneral.Text = "";

            try
            {
                var pacientes = await servicio.ObtenerPacientesAsync();

                catalogoPacientes = pacientes
                    .Where(p => p.Activo)
                    .OrderBy(p => p.NombreCompleto)
                    .ToList();

                // El catálogo no se filtra por fecha ni paciente.
                var tratamientos =
                    await servicio.ObtenerTratamientosAsync(
                        0, DateTime.Today);

                catalogoTratamientos = tratamientos
                    .GroupBy(t => t.IdTratamiento)
                    .Select(g => g.First())
                    .OrderBy(t => t.Nombre)
                    .ToList();

                var materiales = await servicio.ObtenerMaterialesAsync();

                catalogoMateriales = materiales
                    .Where(m => m.Activo)
                    .OrderBy(m => m.Nombre)
                    .ToList();
            }
            catch (Exception ex)
            {
                ErrorGeneral.Text =
                    "No se pudieron cargar los catálogos: " +
                    ex.Message;
            }
            finally
            {
                cargandoDatos = false;
            }
        }

        // POPUP DE PACIENTES
        private void SeleccionarPaciente_Click(
            object sender, RoutedEventArgs e)
        {
            ErrorPaciente.Text = "";

            if (catalogoPacientes.Count == 0)
            {
                ErrorPaciente.Text =
                    "No hay pacientes disponibles.";
                return;
            }

            var popup = new SeleccionIngresoWindow(
                catalogoPacientes,
                pacienteSeleccionado)
            {
                Owner = this
            };

            if (popup.ShowDialog() == true &&
                popup.PacienteElegido != null)
            {
                pacienteSeleccionado = popup.PacienteElegido;

                TxtPaciente.Text =
                    pacienteSeleccionado.NombreCompleto;
            }
        }

        // POPUP DE TRATAMIENTOS
        private void SeleccionarTratamientos_Click(
            object sender, RoutedEventArgs e)
        {
            ErrorTratamientos.Text = "";

            if (catalogoTratamientos.Count == 0)
            {
                ErrorTratamientos.Text =
                    "No hay tratamientos disponibles.";
                return;
            }

            var popup = new SeleccionIngresoWindow(
                catalogoTratamientos,
                tratamientosElegidos)
            {
                Owner = this
            };

            if (popup.ShowDialog() == true)
            {
                tratamientosElegidos =
                    popup.TratamientosElegidos.ToList();

                TxtTratamientos.Text =
                    tratamientosElegidos.Count == 0
                    ? "Seleccione tratamientos"
                    : string.Join(", ",
                        tratamientosElegidos.Select(t => t.Nombre));

                RecalcularTotal();
            }
        }

        // POPUP DE MATERIALES
        private void SeleccionarMateriales_Click(
            object sender, RoutedEventArgs e)
        {
            ErrorMaterial.Text = "";

            if (catalogoMateriales.Count == 0)
            {
                ErrorMaterial.Text =
                    "No hay materiales disponibles.";
                return;
            }

            var popup = new SeleccionIngresoWindow(
                catalogoMateriales,
                materialesElegidos)
            {
                Owner = this
            };

            if (popup.ShowDialog() == true)
            {
                materialesElegidos =
                    popup.MaterialesElegidos.ToList();

                TxtMateriales.Text =
                    materialesElegidos.Count == 0
                    ? "Seleccionar materiales"
                    : $"{materialesElegidos.Count} material(es)";
            }
        }

        // CÁLCULOS
        private decimal ObtenerSubtotal()
        {
            return tratamientosElegidos.Sum(t => t.Importe);
        }

        private bool LeerDescuento(out decimal descuento)
        {
            if (decimal.TryParse(
                TxtDescuento.Text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out descuento))
                return true;

            return decimal.TryParse(
                TxtDescuento.Text,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out descuento);
        }

        private void Recalcular_Click(
            object sender, TextChangedEventArgs e)
        {
            if (inicializado)
                RecalcularTotal();
        }

        private void RecalcularTotal()
        {
            decimal subtotal = ObtenerSubtotal();

            decimal descuento =
                LeerDescuento(out decimal valor) ? valor : 0m;

            TxtSubtotal.Text = $"L {subtotal:N2}";
            TxtTotal.Text =
                $"L {Math.Max(0m, subtotal - descuento):N2}";
        }

        // VALIDACIONES EN ROJO
        private bool ValidarFormulario()
        {
            ErrorPaciente.Text = "";
            ErrorFecha.Text = "";
            ErrorTratamientos.Text = "";
            ErrorMaterial.Text = "";
            ErrorDescuento.Text = "";
            ErrorMedioPago.Text = "";
            ErrorGeneral.Text = "";

            bool valido = true;

            if (pacienteSeleccionado == null)
            {
                ErrorPaciente.Text =
                    "Seleccione un paciente.";
                valido = false;
            }

            if (DpFecha.SelectedDate == null)
            {
                ErrorFecha.Text =
                    "Seleccione una fecha.";
                valido = false;
            }
            else if (DpFecha.SelectedDate.Value.Date > DateTime.Today)
            {
                ErrorFecha.Text =
                    "La fecha no puede ser futura.";
                valido = false;
            }

            if (tratamientosElegidos.Count == 0)
            {
                ErrorTratamientos.Text =
                    "Seleccione al menos un tratamiento.";
                valido = false;
            }

            if (!LeerDescuento(out decimal descuento))
            {
                ErrorDescuento.Text =
                    "Ingrese un descuento válido.";
                valido = false;
            }
            else if (descuento < 0 ||
                     descuento > ObtenerSubtotal())
            {
                ErrorDescuento.Text =
                    "El descuento debe estar entre 0 y el subtotal.";
                valido = false;
            }

            if (CmbMedioPago.SelectedItem is not ComboBoxItem medio ||
                (medio.Content?.ToString() != "Efectivo" &&
                 medio.Content?.ToString() != "Transferencia"))
            {
                ErrorMedioPago.Text =
                    "Seleccione Efectivo o Transferencia.";
                valido = false;
            }

            foreach (var consumido in materialesElegidos)
            {
                var material = catalogoMateriales.FirstOrDefault(
                    m => m.IdMaterial == consumido.IdMaterial);

                if (material == null ||
                    !material.Activo ||
                    consumido.Cantidad <= 0 ||
                    consumido.Cantidad > material.StockActual)
                {
                    ErrorMaterial.Text =
                        "Revise las cantidades y existencias.";
                    valido = false;
                    break;
                }
            }

            return valido;
        }

        // GUARDAR INGRESO
        private async void Guardar_Click(
            object sender, RoutedEventArgs e)
        {
            if (guardando || !ValidarFormulario())
                return;

            guardando = true;
            BtnGuardar.IsEnabled = false;

            try
            {
                var medio =
                    (ComboBoxItem)CmbMedioPago.SelectedItem;

                LeerDescuento(out decimal descuento);

                var request = new RegistrarIngresoRequest
                {
                    IdPaciente = pacienteSeleccionado!.IdPaciente,
                    Fecha = DpFecha.SelectedDate!.Value,
                    Descuento = descuento,
                    MedioPago = medio.Content?.ToString() ?? "",

                    // Compatible con la API utilizada anteriormente:
                    // aquí se envían IDs del catálogo de tratamientos.
                    IdsTratamientosVisita = tratamientosElegidos
                        .Select(t => t.IdTratamiento)
                        .Distinct()
                        .ToList(),

                    Materiales = materialesElegidos.ToList()
                };

                await servicio.GuardarIngresoAsync(request);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                ErrorGeneral.Text =
                    "No se pudo guardar el ingreso: " +
                    ex.Message;
            }
            finally
            {
                guardando = false;
                BtnGuardar.IsEnabled = true;
            }
        }

        // DESCARTAR UTILIZANDO LA VENTANA EXISTENTE
        private void Cerrar_Click(
            object sender, RoutedEventArgs e)
        {
            if (guardando)
                return;

            var confirmar = new DescartarPacienteWindow
            {
                Owner = this
            };

            if (confirmar.ShowDialog() == true)
            {
                DialogResult = false;
            }
        }
    }
}

