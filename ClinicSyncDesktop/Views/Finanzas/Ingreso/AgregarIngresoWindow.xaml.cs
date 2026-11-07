
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
            TxtHora.Text = DateTime.Now.ToString("HH:mm");
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

        private int versionConsulta = 0;
        private async Task CargarTratamientosPacienteAsync()
        {
            int versionActual = ++versionConsulta;

            catalogoTratamientos.Clear();
            tratamientosElegidos.Clear();

            TxtTratamientos.Text = "Seleccione tratamientos";
            ErrorTratamientos.Text = "";

            RecalcularTotal();

            if (pacienteSeleccionado == null ||
                DpFecha.SelectedDate == null)
                return;

            int idPaciente = pacienteSeleccionado.IdPaciente;
            DateTime fecha = DpFecha.SelectedDate.Value.Date;

            try
            {
                var resultado =
                    await servicio.ObtenerTratamientosAsync(
                        idPaciente, fecha);

                // Evitar que una consulta anterior sobrescriba
                // los resultados del paciente o fecha actual.
                if (versionActual != versionConsulta)
                    return;

                catalogoTratamientos = resultado
                    .OrderBy(t => t.Nombre)
                    .ToList();

                if (catalogoTratamientos.Count == 0)
                {
                    ErrorTratamientos.Text =
                        "No existen tratamientos registrados " +
                        "para este paciente en la fecha seleccionada.";
                }
            }
            catch (Exception ex)
            {
                if (versionActual != versionConsulta)
                    return;

                ErrorTratamientos.Text =
                    "No se pudieron consultar los tratamientos: " +
                    ex.Message;
            }
        }


        // POPUP DE PACIENTES

        private async void SeleccionarPaciente_Click(
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

                await CargarTratamientosPacienteAsync();
            }
        }
        private async void DpFecha_SelectedDateChanged(
            object sender, SelectionChangedEventArgs e)
        {
            if (!inicializado)
                return;

            await CargarTratamientosPacienteAsync();
        }


        // POPUP DE TRATAMIENTOS
        private void SeleccionarTratamientos_Click(
            object sender, RoutedEventArgs e)
        {
            ErrorTratamientos.Text = "";

            if (pacienteSeleccionado == null)
            {
                ErrorTratamientos.Text =
                    "Primero seleccione un paciente.";
                return;
            }

            if (DpFecha.SelectedDate == null)
            {
                ErrorTratamientos.Text =
                    "Primero seleccione una fecha.";
                return;
            }

            if (catalogoTratamientos.Count == 0)
            {
                ErrorTratamientos.Text =
                    "No existen tratamientos registrados " +
                    "para este paciente en la fecha seleccionada.";
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
            if (!inicializado)
                return;

            ErrorDescuento.Text = "";

            if (string.IsNullOrWhiteSpace(TxtDescuento.Text))
            {
                ErrorDescuento.Text =
                    "Ingrese un descuento válido.";
            }
            else if (!LeerDescuento(out decimal descuento))
            {
                ErrorDescuento.Text =
                    "Ingrese un descuento válido.";
            }
            else if (descuento < 0)
            {
                ErrorDescuento.Text =
                    "El descuento no puede ser menor que 0.";
            }
            else if (descuento > ObtenerSubtotal())
            {
                ErrorDescuento.Text =
                    "El descuento no puede superar el subtotal.";
            }

            RecalcularTotal();
        }

        private void RecalcularTotal()
        {
            decimal subtotal = ObtenerSubtotal();

            decimal descuento = 0;

            if (LeerDescuento(out decimal valor) &&
                valor >= 0 &&
                valor <= subtotal)
            {
                descuento = valor;
            }

            decimal total = subtotal - descuento;

            TxtSubtotal.Text = $"L {subtotal:N2}";
            TxtTotal.Text = $"L {total:N2}";
        }


        // VALIDACIONES EN ROJO
        private bool TryObtenerFechaHora(out DateTime fecha)
        {
            fecha = default;

            if (DpFecha.SelectedDate is not DateTime fechaSeleccionada)
                return false;

            if (!TimeSpan.TryParseExact(
                    TxtHora.Text.Trim(),
                    @"hh\:mm",
                    CultureInfo.InvariantCulture,
                    out TimeSpan hora))
            {
                return false;
            }

            fecha = fechaSeleccionada.Date.Add(hora);
            return true;
        }

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
            else if (!TryObtenerFechaHora(out DateTime fechaHora))
            {
                ErrorFecha.Text = "Ingrese una hora válida con formato HH:mm.";
                valido = false;
            }
            else if (fechaHora > DateTime.Now)
            {
                ErrorFecha.Text = "La fecha y hora no pueden ser futuras.";
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

            else if (descuento < 0)
            {
                ErrorDescuento.Text =
                    "El descuento no puede ser menor que 0.";
                valido = false;
            }
            else if (descuento > ObtenerSubtotal())
            {
                ErrorDescuento.Text =
                    "El descuento no puede superar el subtotal.";
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
                TryObtenerFechaHora(out DateTime fechaHora);

                var request = new RegistrarIngresoRequest
                {
                    IdPaciente = pacienteSeleccionado!.IdPaciente,
                    Fecha = fechaHora,
                    Descuento = descuento,
                    MedioPago = medio.Content?.ToString() ?? "",

                    // Compatible con la API utilizada anteriormente:
                    // aquí se envían IDs del catálogo de tratamientos.

                    IdsTratamientosVisita = tratamientosElegidos
                   .Select(t => t.IdTratamientoVisita)
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

            var confirmar = new ConfirmarDescarteWindow
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

