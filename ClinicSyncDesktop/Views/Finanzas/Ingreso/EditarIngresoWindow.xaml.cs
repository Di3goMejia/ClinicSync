
using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSyncDesktop.Views
{
    public partial class EditarIngresoWindow : Window
    {
        private readonly int idIngreso;
        private readonly IngresosApiService servicio = new();

        private List<PacienteIngreso> pacientes = new();
        private List<TratamientoIngreso> catalogoTratamientos = new();
        private List<TratamientoIngreso> tratamientosElegidos = new();

        private int idPacienteSeleccionado;
        private bool guardando;
        private bool cargando;

        public EditarIngresoWindow(int idIngreso)
        {
            InitializeComponent();

            this.idIngreso = idIngreso;

            Loaded += EditarIngresoWindow_Loaded;
        }

        private async void EditarIngresoWindow_Loaded(
            object sender, RoutedEventArgs e)
        {
            await CargarDatosAsync();
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


        private async Task CargarDatosAsync()
        {
            cargando = true;
            BtnGuardarCambios.IsEnabled = false;

            ErrorGeneral.Text = "";
            ErrorPaciente.Text = "";
            ErrorTratamientos.Text = "";

            try
            {
                // 1. Cargar primero los catálogos.
                var listaPacientes =
                    await servicio.ObtenerPacientesAsync();

                pacientes = listaPacientes?.ToList()
                            ?? new List<PacienteIngreso>();

                // 2. Obtener el ingreso seleccionado.
                var ingreso =
                    await servicio.ObtenerIngresoPorIdAsync(idIngreso);

                idPacienteSeleccionado = ingreso.IdPaciente;

                // 3. Mostrar los datos del ingreso.
                var paciente = pacientes.FirstOrDefault(
                    p => p.IdPaciente == ingreso.IdPaciente);

                var listaTratamientos =
                    await servicio.ObtenerTratamientosAsync(
                        paciente!.IdPaciente, DateTime.Today);

                catalogoTratamientos =
                    listaTratamientos?.ToList()
                    ?? new List<TratamientoIngreso>();

                TxtPaciente.Text = paciente != null
                    ? paciente.NombreCompleto
                    : ingreso.Paciente;

                DpFecha.SelectedDate = ingreso.Fecha;
                TxtHora.Text = ingreso.Fecha.ToString("HH:mm");

                TxtDescuento.Text =
                    ingreso.Descuento.ToString("0.00");

                TglActivo.IsChecked = ingreso.Activo;

                // 4. Seleccionar el medio de pago.
                foreach (ComboBoxItem item in CmbMedioPago.Items)
                {
                    if (item.Content?.ToString() == ingreso.MedioPago)
                    {
                        CmbMedioPago.SelectedItem = item;
                        break;
                    }
                }

                // 5. Recuperar tratamientos asociados.
                var idsTratamientos = ingreso.Tratamientos
                    .Select(t => t.IdTratamiento)
                    .ToHashSet();

                tratamientosElegidos = catalogoTratamientos
                    .Where(t => idsTratamientos.Contains(t.IdTratamiento))
                    .ToList();

                TxtTratamientos.Text =
                    string.Join(", ",
                        tratamientosElegidos.Select(t => t.Nombre));

                if (tratamientosElegidos.Count != idsTratamientos.Count)
                {
                    ErrorTratamientos.Text =
                        "Algunos tratamientos del ingreso no están " +
                        "disponibles en el catálogo actual.";
                }

                // 6. Comprobar si cargaron los catálogos.
                if (pacientes.Count == 0)
                {
                    ErrorPaciente.Text =
                        "No se encontraron pacientes disponibles.";
                }

                if (catalogoTratamientos.Count == 0)
                {
                    ErrorTratamientos.Text =
                        "No se encontraron tratamientos en el catálogo.";
                }
                BtnGuardarCambios.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ErrorGeneral.Text =
                    "Error al cargar la información: " + ex.Message;

                BtnGuardarCambios.IsEnabled = false;
            }
            finally
            {
                cargando = false;
            }
        }


        // SELECCIONAR PACIENTE

        private async void SeleccionarPaciente_Click(
            object sender, RoutedEventArgs e)
        {
            ErrorPaciente.Text = "";

            try
            {
                // Consultar nuevamente los pacientes.
                var resultado = await servicio.ObtenerPacientesAsync();

                pacientes = resultado?.ToList()
                            ?? new List<PacienteIngreso>();

                var disponibles = pacientes
                    .Where(p => p.Activo ||
                                p.IdPaciente == idPacienteSeleccionado)
                    .OrderBy(p => p.NombreCompleto)
                    .ToList();

                if (disponibles.Count == 0)
                {
                    ErrorPaciente.Text =
                        "No existen pacientes disponibles.";
                    return;
                }

                var ventana = new Window
                {
                    Title = "Seleccionar paciente",
                    Width = 390,
                    Height = 370,
                    Owner = this,
                    WindowStartupLocation =
                        WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize,
                    Background = System.Windows.Media.Brushes.White
                };

                var panel = new DockPanel
                {
                    Margin = new Thickness(15)
                };

                var lista = new ListBox
                {
                    DisplayMemberPath = "NombreCompleto",
                    ItemsSource = disponibles,
                    FontSize = 14
                };

                lista.SelectedItem = disponibles.FirstOrDefault(
                    p => p.IdPaciente == idPacienteSeleccionado);

                var botonAceptar = new Button
                {
                    Content = "Seleccionar",
                    Height = 36,
                    Margin = new Thickness(0, 10, 0, 0),
                    Background = new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(
                            145, 210, 221))
                };

                botonAceptar.Click += (_, _) =>
                {
                    if (lista.SelectedItem is PacienteIngreso seleccionado)
                    {
                        idPacienteSeleccionado = seleccionado.IdPaciente;

                        TxtPaciente.Text =
                            seleccionado.NombreCompleto;

                        ventana.DialogResult = true;
                    }
                    else
                    {
                        ErrorPaciente.Text = "Seleccione un paciente.";
                    }
                };

                DockPanel.SetDock(botonAceptar, Dock.Bottom);

                panel.Children.Add(botonAceptar);
                panel.Children.Add(lista);

                ventana.Content = panel;
                ventana.ShowDialog();
            }
            catch (Exception ex)
            {
                ErrorPaciente.Text =
                    "No se pudieron cargar los pacientes: " +
                    ex.Message;
            }
        }


        // SELECCIONAR TRATAMIENTOS

        private async void SeleccionarTratamientos_Click(
            object sender, RoutedEventArgs e)
        {
            ErrorTratamientos.Text = "";

            try
            {
                // Obtener el catálogo completo.
                var resultado =
                    await servicio.ObtenerTratamientosAsync(
                        idPacienteSeleccionado, DateTime.Today);

                catalogoTratamientos =
                    resultado?.ToList()
                    ?? new List<TratamientoIngreso>();

                if (catalogoTratamientos.Count == 0)
                {
                    ErrorTratamientos.Text =
                        "No existen tratamientos disponibles.";
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
                        string.Join(", ",
                            tratamientosElegidos.Select(t => t.Nombre));

                    Recalcular_Click(
                        TxtDescuento,
                        null!);
                }
            }
            catch (Exception ex)
            {
                ErrorTratamientos.Text =
                    "No se pudieron cargar los tratamientos: " +
                    ex.Message;
            }
        }


        // VALIDAR DESCUENTO AL MODIFICAR
        private void Recalcular_Click(
            object sender, TextChangedEventArgs e)
        {
            if (cargando || !IsLoaded)
                return;

            ErrorDescuento.Text = "";

            if (!LeerDescuento(out decimal descuento))
            {
                ErrorDescuento.Text =
                    "Ingrese un descuento numérico válido.";
                return;
            }

            decimal subtotal =
                tratamientosElegidos.Sum(t => t.Importe);

            if (descuento < 0 || descuento > subtotal)
            {
                ErrorDescuento.Text =
                    "El descuento debe estar entre 0 y el subtotal.";
            }
        }

        private bool LeerDescuento(out decimal descuento)
        {
            return decimal.TryParse(
                TxtDescuento.Text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out descuento)
                ||
                decimal.TryParse(
                    TxtDescuento.Text,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out descuento);
        }

        // VALIDACIONES EN TEXTO ROJO
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
            ErrorTratamientos.Text = "";
            ErrorFecha.Text = "";
            ErrorDescuento.Text = "";
            ErrorMedioPago.Text = "";
            ErrorGeneral.Text = "";

            bool valido = true;

            if (idPacienteSeleccionado <= 0)
            {
                ErrorPaciente.Text = "Seleccione un paciente.";
                valido = false;
            }

            if (tratamientosElegidos.Count == 0)
            {
                ErrorTratamientos.Text =
                    "Seleccione al menos un tratamiento.";
                valido = false;
            }

            if (DpFecha.SelectedDate == null)
            {
                ErrorFecha.Text = "Seleccione una fecha.";
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

            decimal subtotal =
                tratamientosElegidos.Sum(t => t.Importe);

            if (!LeerDescuento(out decimal descuento))
            {
                ErrorDescuento.Text =
                    "El descuento debe ser numérico.";
                valido = false;
            }
            else if (descuento < 0 || descuento > subtotal)
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

            return valido;
        }

        // GUARDAR CAMBIOS
        private async void GuardarCambios_Click(
            object sender, RoutedEventArgs e)
        {
            if (guardando || !ValidarFormulario())
                return;

            guardando = true;
            BtnGuardarCambios.IsEnabled = false;

            try
            {
                var medio =
                    (ComboBoxItem)CmbMedioPago.SelectedItem;

                LeerDescuento(out decimal descuento);
                TryObtenerFechaHora(out DateTime fechaHora);

                var request = new EditarIngresoRequest
                {
                    IdPaciente = idPacienteSeleccionado,
                    Fecha = fechaHora,
                    Descuento = descuento,
                    MedioPago = medio.Content?.ToString() ?? "",
                    Activo = TglActivo.IsChecked == true,

                    IdsTratamientos = tratamientosElegidos
                        .Select(t => t.IdTratamiento)
                        .Distinct()
                        .ToList()
                };

                await servicio.ActualizarIngresoAsync(
                    idIngreso, request);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                ErrorGeneral.Text =
                    "No se pudieron guardar los cambios: " + ex.Message;
            }
            finally
            {
                guardando = false;
                BtnGuardarCambios.IsEnabled = true;
            }
        }

        // CERRAR SIN GUARDAR
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

