using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Validations;
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
        private const string FormatoFecha = "yyyy'/'MM'/'dd";

        private readonly int idIngreso;
        private readonly IngresosApiService servicio = new();

        private List<TratamientoIngreso> tratamientosOriginales = new();
        private List<TratamientoIngreso> tratamientosElegidos = new();
        private List<MaterialConsumido> materialesOriginales = new();
        private List<MaterialConsumido> materialesElegidos = new();

        private int idPacienteOriginal;
        private int idPacienteSeleccionado;
        private DateTime fechaOriginal;
        private DateTime? fechaSeleccionada;
        private bool ingresoDeVisita;
        private bool ingresoEstabaActivo;
        private bool datosCargados;
        private bool sincronizandoFecha;
        private bool guardando;
        private bool abriendoSelector;

        public EditarIngresoWindow(int idIngreso)
        {
            this.idIngreso = idIngreso;
            InitializeComponent();
            Loaded += EditarIngresoWindow_Loaded;
        }

        private async void EditarIngresoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= EditarIngresoWindow_Loaded;

            try
            {
                var ingreso = await ObtenerIngresoPorIdAsync(idIngreso);

                fechaOriginal = ingreso.Fecha.Date;
                idPacienteOriginal = ingreso.IdPaciente;
                idPacienteSeleccionado = ingreso.IdPaciente;
                ingresoEstabaActivo = ingreso.Activo;
                ingresoDeVisita = ingreso.Tratamientos.Any(t => t.IdTratamientoVisita > 0);

                tratamientosOriginales = ingreso.Tratamientos.ToList();
                tratamientosElegidos = ingreso.Tratamientos.ToList();
                materialesOriginales = ingreso.Materiales.ToList();
                materialesElegidos = ingreso.Materiales.ToList();

                TxtPaciente.Text = ingreso.Paciente;
                EstablecerFecha(fechaOriginal);
                TxtDescuento.Text = ingreso.Descuento.ToString("0.00");
                TglActivo.IsChecked = ingreso.Activo;

                CmbMedioPago.SelectedItem = CmbMedioPago.Items
                    .OfType<ComboBoxItem>()
                    .FirstOrDefault(item =>
                        string.Equals(
                            item.Content?.ToString(),
                            ingreso.MedioPago,
                            StringComparison.OrdinalIgnoreCase));

                MostrarTratamientos();
                MostrarMateriales();
                ActualizarTotales(ingreso.Subtotal, ingreso.Descuento);

                datosCargados = true;
                BtnGuardarCambios.IsEnabled = true;
            }
            catch (Exception ex)
            {
                ErrorGeneral.Text = "Error al cargar la información: " + ex.Message;
            }
        }

        public Task<EditarIngresoDetalle> ObtenerIngresoPorIdAsync(int idIngreso)
        {
            return servicio.ObtenerIngresoPorIdAsync(idIngreso);
        }

        private async void SeleccionarPaciente_Click(object sender, RoutedEventArgs e)
        {
            if (!datosCargados || guardando || abriendoSelector)
                return;

            ErrorPaciente.Text = "";
            abriendoSelector = true;

            try
            {
                var disponibles = (await servicio.ObtenerPacientesAsync())
                    .Where(p => p.Activo || p.IdPaciente == idPacienteSeleccionado)
                    .OrderBy(p => p.NombreCompleto)
                    .ToList();

                if (disponibles.Count == 0)
                {
                    ErrorPaciente.Text = "No existen pacientes disponibles.";
                    return;
                }

                var actual = disponibles.FirstOrDefault(
                    p => p.IdPaciente == idPacienteSeleccionado);

                var selector = new SeleccionIngresoWindow(disponibles, actual)
                {
                    Owner = this
                };

                if (selector.ShowDialog() == true && selector.PacienteElegido is { } paciente)
                {
                    bool cambioPaciente = idPacienteSeleccionado != paciente.IdPaciente;
                    idPacienteSeleccionado = paciente.IdPaciente;
                    TxtPaciente.Text = paciente.NombreCompleto;

                    if (cambioPaciente && ingresoDeVisita)
                        LimpiarTratamientosSeleccionados();
                }
            }
            catch (Exception ex)
            {
                ErrorPaciente.Text = "No se pudieron cargar los pacientes: " + ex.Message;
            }
            finally
            {
                abriendoSelector = false;
            }
        }

        private async void SeleccionarTratamientos_Click(object sender, RoutedEventArgs e)
        {
            if (!datosCargados || guardando || abriendoSelector)
                return;

            ErrorTratamientos.Text = "";

            if (idPacienteSeleccionado <= 0)
            {
                ErrorPaciente.Text = "Seleccione un paciente.";
                return;
            }

            if (!ValidarFechaEscrita(out DateTime fecha))
                return;

            int idPaciente = idPacienteSeleccionado;
            abriendoSelector = true;

            try
            {
                var disponibles = ingresoDeVisita
                    ? await servicio.ObtenerTratamientosAsync(idPaciente, fecha)
                    : await servicio.ObtenerTratamientosCatalogoAsync();

                if (ingresoDeVisita &&
                    (!TryLeerFecha(out DateTime fechaActual) || fechaActual != fecha))
                {
                    return;
                }

                bool mostrarOriginales = !ingresoDeVisita ||
                    (idPaciente == idPacienteOriginal && fecha == fechaOriginal);

                IEnumerable<TratamientoIngreso> originales = mostrarOriginales
                    ? tratamientosOriginales
                    : Enumerable.Empty<TratamientoIngreso>();

                var catalogo = tratamientosElegidos
                    .Concat(originales)
                    .Concat(disponibles)
                    .GroupBy(t => ingresoDeVisita ? t.IdTratamientoVisita : t.IdTratamiento)
                    .Select(grupo => grupo.First())
                    .ToList();

                if (catalogo.Count == 0)
                {
                    ErrorTratamientos.Text = "No hay tratamientos disponibles para la selección actual.";
                    return;
                }

                var selector = new SeleccionIngresoWindow(catalogo, tratamientosElegidos)
                {
                    Owner = this
                };

                if (selector.ShowDialog() == true)
                {
                    tratamientosElegidos = selector.TratamientosElegidos.ToList();
                    MostrarTratamientos();
                    RecalcularDescuento();
                }
            }
            catch (Exception ex)
            {
                ErrorTratamientos.Text = "No se pudieron cargar los tratamientos: " + ex.Message;
            }
            finally
            {
                abriendoSelector = false;
            }
        }

        private async void SeleccionarMateriales_Click(object sender, RoutedEventArgs e)
        {
            if (!datosCargados || guardando || abriendoSelector)
                return;

            ErrorMaterial.Text = "";
            abriendoSelector = true;

            try
            {
                var catalogo = await servicio.ObtenerMaterialesAsync();
                var originales = materialesOriginales.ToDictionary(m => m.IdMaterial);

                var disponibles = catalogo
                    .Where(m => m.Activo)
                    .Select(m => new MaterialIngreso
                    {
                        IdMaterial = m.IdMaterial,
                        Nombre = m.Nombre,
                        Activo = true,
                        StockActual = m.StockActual +
                            (ingresoEstabaActivo &&
                             originales.TryGetValue(m.IdMaterial, out var anterior)
                                ? anterior.Cantidad
                                : 0m)
                    })
                    .ToList();

                disponibles.AddRange(materialesOriginales
                    .Where(m => disponibles.All(d => d.IdMaterial != m.IdMaterial))
                    .Select(m => new MaterialIngreso
                    {
                        IdMaterial = m.IdMaterial,
                        Nombre = m.Nombre,
                        Activo = true,
                        StockActual = m.Cantidad
                    }));

                var seleccion = SeleccionarMaterialesWindow.ElegirParaIngreso(
                    this, disponibles, materialesElegidos,
                    catalogo.ToDictionary(m => m.IdMaterial, m => m.StockActual));


                if (seleccion != null)
                {
                    materialesElegidos = seleccion;
                    MostrarMateriales();
                }
            }
            catch (Exception ex)
            {
                ErrorMaterial.Text = "No se pudieron cargar los materiales: " + ex.Message;
            }
            finally
            {
                abriendoSelector = false;
            }
        }

        private void MostrarTratamientos()
        {
            TxtTratamientos.Text = tratamientosElegidos.Count == 0
                ? "Seleccione tratamientos"
                : string.Join(", ", tratamientosElegidos.Select(t => t.Nombre));

            TxtTratamientos.ToolTip = TxtTratamientos.Text;
        }

        private void MostrarMateriales()
        {
            TxtMateriales.Text = materialesElegidos.Count == 0
                ? "Seleccionar materiales"
                : string.Join(", ",
                    materialesElegidos.Select(m => $"{m.Nombre} ({m.Cantidad:0.##})"));

            TxtMateriales.ToolTip = TxtMateriales.Text;
        }

        private void CalendarioIngreso_Click(object sender, RoutedEventArgs e)
        {
            if (datosCargados && !guardando)
                DpFecha.IsDropDownOpen = true;
        }

        private void DpFecha_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (datosCargados && !sincronizandoFecha &&
                DpFecha.SelectedDate is DateTime fecha)
            {
                EstablecerFecha(fecha);
            }
        }

        private void TxtFechaIngreso_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!datosCargados || sincronizandoFecha)
                return;

            ErrorFecha.Text = "";

            if (TryLeerFecha(out DateTime fecha) && fecha <= DateTime.Today)
                EstablecerFecha(fecha);
        }

        private void TxtFechaIngreso_LostFocus(object sender, RoutedEventArgs e)
        {
            if (datosCargados && TxtFechaIngreso.Text.Trim().Length > 0)
                ValidarFechaEscrita(out _);
        }

        private bool TryLeerFecha(out DateTime fecha)
        {
            return DateTime.TryParseExact(
                TxtFechaIngreso.Text.Trim(),
                FormatoFecha,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out fecha);
        }

        private bool ValidarFechaEscrita(out DateTime fecha)
        {
            if (!TryLeerFecha(out fecha))
            {
                ErrorFecha.Text = "Ingrese una fecha válida en formato YYYY/MM/DD.";
                return false;
            }

            if (fecha > DateTime.Today)
            {
                ErrorFecha.Text = "La fecha del ingreso no puede ser futura.";
                return false;
            }

            EstablecerFecha(fecha);
            return true;
        }

        private void EstablecerFecha(DateTime fecha)
        {
            fecha = fecha.Date;
            bool cambioFecha = fechaSeleccionada != fecha;
            fechaSeleccionada = fecha;

            sincronizandoFecha = true;
            try
            {
                if (DpFecha.SelectedDate != fecha)
                    DpFecha.SelectedDate = fecha;

                string texto = fecha.ToString(FormatoFecha, CultureInfo.InvariantCulture);
                if (TxtFechaIngreso.Text != texto)
                    TxtFechaIngreso.Text = texto;
            }
            finally
            {
                sincronizandoFecha = false;
            }

            ErrorFecha.Text = "";

            if (datosCargados && cambioFecha && ingresoDeVisita)
                LimpiarTratamientosSeleccionados();
        }

        private void LimpiarTratamientosSeleccionados()
        {
            tratamientosElegidos.Clear();
            MostrarTratamientos();
            ActualizarTotales(0m, 0m);
            ErrorTratamientos.Text = "";
            ErrorDescuento.Text = "";
        }

        private void Recalcular_Click(object sender, TextChangedEventArgs e)
        {
            if (datosCargados)
                RecalcularDescuento();
        }

        private void RecalcularDescuento()
        {
            ErrorDescuento.Text = "";
            decimal subtotal = tratamientosElegidos.Sum(t => t.Importe);

            if (!LeerDescuento(out decimal descuento))
            {
                ErrorDescuento.Text = "Ingrese un descuento numérico válido.";
                ActualizarTotales(subtotal, 0m);
                return;
            }

            if (descuento < 0 || descuento > subtotal)
            {
                ErrorDescuento.Text = "El descuento debe estar entre 0 y el subtotal.";
                ActualizarTotales(subtotal, 0m);
                return;
            }

            ActualizarTotales(subtotal, descuento);
        }

        private bool LeerDescuento(out decimal descuento)
        {
            return decimal.TryParse(
                       TxtDescuento.Text,
                       NumberStyles.Number,
                       CultureInfo.CurrentCulture,
                       out descuento)
                || decimal.TryParse(
                       TxtDescuento.Text,
                       NumberStyles.Number,
                       CultureInfo.InvariantCulture,
                       out descuento);
        }

        private void ActualizarTotales(decimal subtotal, decimal descuento)
        {
            TxtSubtotal.Text = $"L {subtotal:N2}";
            TxtTotal.Text = $"L {subtotal - descuento:N2}";
        }

        private bool ValidarFormulario(out DateTime fecha)
        {
            ErrorPaciente.Text = "";
            ErrorTratamientos.Text = "";
            ErrorFecha.Text = "";
            ErrorDescuento.Text = "";
            ErrorMedioPago.Text = "";
            ErrorGeneral.Text = "";

            if (!ValidarFechaEscrita(out fecha))
                return false;

            var medio = CmbMedioPago.SelectedItem as ComboBoxItem;

            // El validador compartido recibe un valor fijo porque el formulario usa solo la fecha.
            var resultado = new ValidadorIngreso().Validar(new EntradaIngreso(
                idPacienteSeleccionado > 0 ? new object() : null,
                fecha,
                "00:00",
                TxtDescuento.Text,
                medio?.Content?.ToString(),
                tratamientosElegidos.Sum(t => t.Importe)));

            var error = resultado.Errores.FirstOrDefault();

            if (error?.Campo == nameof(EntradaIngreso.Paciente))
                ErrorPaciente.Text = error.Mensaje;
            else if (error?.Campo == nameof(EntradaIngreso.Fecha))
                ErrorFecha.Text = error.Mensaje;
            else if (error?.Campo == nameof(EntradaIngreso.TextoDescuento))
                ErrorDescuento.Text = error.Mensaje;
            else if (error?.Campo == nameof(EntradaIngreso.MedioPago))
                ErrorMedioPago.Text = error.Mensaje;
            else if (error != null)
                ErrorGeneral.Text = error.Mensaje;

            if (tratamientosElegidos.Count == 0)
            {
                ErrorTratamientos.Text = "Seleccione al menos un tratamiento.";
                return false;
            }

            return resultado.EsValido;
        }

        private async void GuardarCambios_Click(object sender, RoutedEventArgs e)
        {
            if (guardando || abriendoSelector || !datosCargados ||
                !ValidarFormulario(out DateTime fecha))
            {
                return;
            }

            var medio = (ComboBoxItem)CmbMedioPago.SelectedItem;
            LeerDescuento(out decimal descuento);

            var request = new EditarIngresoRequest
            {
                IdPaciente = idPacienteSeleccionado,
                Fecha = fecha.Date,
                Descuento = descuento,
                MedioPago = medio.Content?.ToString() ?? "",
                Activo = TglActivo.IsChecked == true,
                Materiales = materialesElegidos.ToList(),
                IdsTratamientos = tratamientosElegidos
                    .Select(t => ingresoDeVisita ? t.IdTratamientoVisita : t.IdTratamiento)
                    .Distinct()
                    .ToList()
            };

            guardando = true;
            BtnGuardarCambios.IsEnabled = false;

            try
            {
                await servicio.ActualizarIngresoAsync(idIngreso, request);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                ErrorGeneral.Text = "No se pudieron guardar los cambios: " + ex.Message;
            }
            finally
            {
                guardando = false;
                BtnGuardarCambios.IsEnabled = true;
            }
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            if (guardando || abriendoSelector)
                return;

            var confirmar = new ConfirmarDescarteWindow { Owner = this };

            if (confirmar.ShowDialog() == true)
                DialogResult = false;
        }
    }
}