using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Validations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSyncDesktop.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class AgregarIngresoWindow : Window
    {
        /// <summary>
        /// The servicio
        /// </summary>
        private readonly IngresosApiService servicio = new();

        /// <summary>
        /// The catalogo pacientes
        /// </summary>
        private List<PacienteIngreso> catalogoPacientes = new();
        /// <summary>
        /// The catalogo tratamientos
        /// </summary>
        private List<TratamientoIngreso> catalogoTratamientos = new();
        /// <summary>
        /// The catalogo materiales
        /// </summary>
        private List<MaterialIngreso> catalogoMateriales = new();

        /// <summary>
        /// The paciente seleccionado
        /// </summary>
        private PacienteIngreso? pacienteSeleccionado;
        /// <summary>
        /// The tratamientos elegidos
        /// </summary>
        private List<TratamientoIngreso> tratamientosElegidos = new();
        /// <summary>
        /// The materiales elegidos
        /// </summary>
        private List<MaterialConsumido> materialesElegidos = new();

        /// <summary>
        /// The guardando
        /// </summary>
        private bool guardando;
        /// <summary>
        /// The inicializado
        /// </summary>
        private bool inicializado;
        /// <summary>
        /// The sincronizando fecha
        /// </summary>
        private bool sincronizandoFecha;
        /// <summary>
        /// The formato fecha
        /// </summary>
        private const string FormatoFecha = "yyyy'/'MM'/'dd";
        /// <summary>
        /// The cargando datos
        /// </summary>
        private bool cargandoDatos;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgregarIngresoWindow"/> class.
        /// </summary>
        public AgregarIngresoWindow()
        {
            InitializeComponent();

            DpFecha.DisplayDateEnd = DateTime.Today;
            DpFecha.SelectedDate = DateTime.Today;
            TxtFechaIngreso.Text = DateTime.Today.ToString(FormatoFecha, CultureInfo.InvariantCulture);

            inicializado = true;

            Loaded += AgregarIngresoWindow_Loaded;
        }

        // CARGAR CATÁLOGOS
        /// <summary>
        /// Handles the Loaded event of the AgregarIngresoWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// The version consulta
        /// </summary>
        private int versionConsulta = 0;
        /// <summary>
        /// Cargars the tratamientos paciente asynchronous.
        /// </summary>
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

        /// <summary>
        /// Handles the Click event of the SeleccionarPaciente control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
        /// <summary>
        /// Handles the Click event of the CalendarioIngreso control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void CalendarioIngreso_Click(object sender, RoutedEventArgs e)
        {
            if (!guardando)
                DpFecha.IsDropDownOpen = true;
        }

        /// <summary>
        /// Handles the TextChanged event of the TxtFechaIngreso control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void TxtFechaIngreso_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inicializado && !sincronizandoFecha)
                ErrorFecha.Text = "";
        }

        /// <summary>
        /// Handles the LostFocus event of the TxtFechaIngreso control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TxtFechaIngreso_LostFocus(object sender, RoutedEventArgs e) 
        {
            if (!TryObtenerFecha(out _))
                return;
        }

        /// <summary>
        /// Handles the SelectedDateChanged event of the DpFecha control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private async void DpFecha_SelectedDateChanged(
            object sender, SelectionChangedEventArgs e)
        {
            if (!inicializado)
                return;

            if (!sincronizandoFecha && DpFecha.SelectedDate is DateTime fecha)
            {
                sincronizandoFecha = true;
                try
                {
                    TxtFechaIngreso.Text = fecha.ToString(FormatoFecha, CultureInfo.InvariantCulture);
                }
                finally
                {
                    sincronizandoFecha = false;
                }
                ErrorFecha.Text = "";
            }

            await CargarTratamientosPacienteAsync();
        }

        // POPUP DE TRATAMIENTOS
        /// <summary>
        /// Handles the Click event of the SeleccionarTratamientos control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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
        /// <summary>
        /// Handles the Click event of the SeleccionarMateriales control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

            var seleccion = SeleccionarMaterialesWindow.ElegirParaIngreso(
                this, catalogoMateriales, materialesElegidos);

            if (seleccion != null)
            {
                materialesElegidos = seleccion;

                TxtMateriales.Text =
                    materialesElegidos.Count == 0
                    ? "Seleccionar materiales"
                    : $"{materialesElegidos.Count} material(es)";
            }
        }

        // CÁLCULOS
        /// <summary>
        /// Obteners the subtotal.
        /// </summary>
        /// <returns></returns>
        private decimal ObtenerSubtotal()
        {
            return tratamientosElegidos.Sum(t => t.Importe);
        }

        /// <summary>
        /// Leers the descuento.
        /// </summary>
        /// <param name="descuento">The descuento.</param>
        /// <returns></returns>
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


        /// <summary>
        /// Handles the Click event of the Recalcular control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Recalculars the total.
        /// </summary>
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
        /// <summary>
        /// Tries the obtener fecha.
        /// </summary>
        /// <param name="fecha">The fecha.</param>
        /// <returns></returns>
        private bool TryObtenerFecha(out DateTime fecha)
        {
            if (!DateTime.TryParseExact(
                    TxtFechaIngreso.Text.Trim(), FormatoFecha,
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha) ||
                fecha > DateTime.Today)
            {
                ErrorFecha.Text = "Ingrese una fecha válida en formato YYYY/MM/DD.";
                return false;
            }

            fecha = fecha.Date;
            if (DpFecha.SelectedDate != fecha)
            {
                sincronizandoFecha = true;
                try
                {
                    DpFecha.SelectedDate = fecha;
                }
                finally
                {
                    sincronizandoFecha = false;
                }
            }

            ErrorFecha.Text = "";
            return true;
        }

        private bool ValidarFormulario(out DateTime fecha)
        {
            ErrorPaciente.Text = "";
            ErrorFecha.Text = "";
            ErrorTratamientos.Text = "";
            ErrorMaterial.Text = "";
            ErrorDescuento.Text = "";
            ErrorMedioPago.Text = "";
            ErrorGeneral.Text = "";

            if (!TryObtenerFecha(out fecha))
                return false;

            var medioSeleccionado = CmbMedioPago.SelectedItem as ComboBoxItem;
            var resultadoValidacion = new ValidadorIngreso().Validar(new EntradaIngreso(
             pacienteSeleccionado, fecha, "00:00",
             TxtDescuento.Text, medioSeleccionado?.Content?.ToString(), ObtenerSubtotal()));
            if (!resultadoValidacion.EsValido)
            {
                var error = resultadoValidacion.Errores.FirstOrDefault();
                if (error?.Campo == nameof(EntradaIngreso.Paciente)) ErrorPaciente.Text = error.Mensaje;
                else if (error?.Campo is nameof(EntradaIngreso.Fecha) or nameof(EntradaIngreso.TextoHora)) ErrorFecha.Text = error.Mensaje;
                else if (error?.Campo == nameof(EntradaIngreso.TextoDescuento)) ErrorDescuento.Text = error.Mensaje;
                else if (error?.Campo == nameof(EntradaIngreso.MedioPago)) ErrorMedioPago.Text = error.Mensaje;
            }

            bool valido = resultadoValidacion.EsValido;

            if (tratamientosElegidos.Count == 0)
            {
                ErrorTratamientos.Text =
                    "Seleccione al menos un tratamiento.";
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
            if (guardando || !ValidarFormulario(out DateTime fecha))
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
                    Fecha = fecha,
                    Descuento = descuento,
                    MedioPago = medio.Content?.ToString() ?? "",

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

