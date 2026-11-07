using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using System.Windows.Controls.Primitives;

namespace ClinicSyncDesktop.Views
{
    public partial class AgregarCitaWindow : Window
    {
        private static readonly string[] Paleta =
        {
            "#8BC34A", "#E91E63", "#FF9800", "#3F51B5",
            "#009688", "#9C27B0", "#F44336", "#00BCD4"
        };

        private readonly AgendaApiService servicio = new();

        private int indiceColor = 0;
        private bool guardando = false;
        private Popup? popupCalendario;

        // Id de la cita registrada (0 si no se registró).
        public int IdCitaCreada { get; private set; }

        public AgregarCitaWindow()
        {
            InitializeComponent();

            // Horas cada 30 minutos: 00:00, 00:30, ... 23:30
            var horas = new List<string>();

            for (int h = 0; h < 24; h++)
            {
                horas.Add($"{h:00}:00");
                horas.Add($"{h:00}:30");
            }

            cmbHoraInicio.ItemsSource = horas;
            cmbHoraFin.ItemsSource = horas;

            dpInicio.DisplayDateStart = DateTime.Today;
            dpFin.DisplayDateStart = DateTime.Today;

            Loaded += AgregarCitaWindow_Loaded;
        }

        // CARGAR PACIENTES ACTIVOS
        private async void AgregarCitaWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                cmbPaciente.ItemsSource =
                    await servicio.ListarPacientesActivosAsync();
            }
            catch (Exception ex)
            {
                MostrarError("No se pudo cargar la lista de pacientes. " + ex.Message);
            }
        }

        // CAMBIAR EL COLOR DE LA CITA
        private void Color_Click(object sender, RoutedEventArgs e)
        {
            indiceColor = (indiceColor + 1) % Paleta.Length;

            btnColor.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(Paleta[indiceColor]));
        }

        // MOSTRAR LA FECHA EN EL CAMPO REDONDEADO
        private static void MostrarFecha(TextBlock texto, DateTime? fecha)
        {
            if (fecha is null)
            {
                texto.Text = "Seleccione una fecha";
                texto.Foreground = new SolidColorBrush(
                    Color.FromRgb(174, 176, 177));
            }
            else
            {
                texto.Text = fecha.Value.ToString("dd/MM/yyyy");
                texto.Foreground = new SolidColorBrush(
                    Color.FromRgb(23, 71, 90));
            }
        }

        // AL ELEGIR LA FECHA DE INICIO, LA FINAL SE LLENA SOLA
        private void dpInicio_SelectedDateChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            MostrarFecha(txtFechaInicio, dpInicio.SelectedDate);

            if (dpInicio.SelectedDate != null &&
                (dpFin.SelectedDate == null ||
                 dpFin.SelectedDate < dpInicio.SelectedDate))
            {
                dpFin.SelectedDate = dpInicio.SelectedDate;
            }
        }

        private void dpFin_SelectedDateChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            MostrarFecha(txtFechaFin, dpFin.SelectedDate);
        }

        // ABRIR EL CALENDARIO DESPLEGABLE
        private void Fecha_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button boton)
                return;

            if (popupCalendario != null && popupCalendario.IsOpen)
            {
                popupCalendario.IsOpen = false;
                return;
            }

            bool esFin = boton.Tag as string == "Fin";
            DatePicker destino = esFin ? dpFin : dpInicio;

            // Solo hoy y fechas futuras (la final, desde la de inicio).
            DateTime minimo = DateTime.Today;

            if (esFin && dpInicio.SelectedDate.HasValue)
                minimo = dpInicio.SelectedDate.Value.Date;

            var calendario = new System.Windows.Controls.Calendar
            {
                DisplayDateStart = minimo,
                DisplayMode = CalendarMode.Month,
                SelectionMode = CalendarSelectionMode.SingleDate,
                Background = Brushes.White
            };

            if (destino.SelectedDate.HasValue)
            {
                calendario.DisplayDate = destino.SelectedDate.Value;
                calendario.SelectedDate = destino.SelectedDate.Value;
            }

            var contenedor = new Border
            {
                Background = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(217, 221, 216)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(5),
                Child = calendario
            };

            popupCalendario = new Popup
            {
                PlacementTarget = boton,
                Placement = PlacementMode.Bottom,
                AllowsTransparency = true,
                StaysOpen = false,
                Child = contenedor
            };

            calendario.SelectedDatesChanged += (s, args) =>
            {
                if (calendario.SelectedDate.HasValue)
                {
                    destino.SelectedDate = calendario.SelectedDate.Value.Date;
                    popupCalendario!.IsOpen = false;
                }
            };

            popupCalendario.IsOpen = true;
        }

        // TODO EL DÍA: LAS HORAS NO SE USAN
        private void TodoElDia_Click(object sender, RoutedEventArgs e)
        {
            bool todoElDia = chkTodoElDia.IsChecked == true;

            cmbHoraInicio.IsEnabled = !todoElDia;
            cmbHoraFin.IsEnabled = !todoElDia;
        }

        // CERRAR Y CONFIRMAR DESCARTE
        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            if (guardando)
                return;

            ConfirmarDescarteWindow advertencia =
                new ConfirmarDescarteWindow(
                    "¿Seguro que desea descartar esta cita?")
                {
                    Owner = this
                };

            bool descartar = advertencia.ShowDialog() == true;

            if (descartar)
                DialogResult = false;
        }

        // AGREGAR CITA
        private async void Agregar_Click(object sender, RoutedEventArgs e)
        {
            if (guardando)
                return;

            LimpiarError();

            bool todoElDia = chkTodoElDia.IsChecked == true;

            // CAMPOS OBLIGATORIOS
            if (cmbPaciente.SelectedItem is not PacienteOpcion paciente ||
                string.IsNullOrWhiteSpace(txtTitulo.Text) ||
                dpInicio.SelectedDate is null ||
                dpFin.SelectedDate is null ||
                (!todoElDia &&
                 (cmbHoraInicio.SelectedItem is null ||
                  cmbHoraFin.SelectedItem is null)))
            {
                MostrarError("Se deben llenar los campos obligatorios.");
                return;
            }

            // ARMAR INICIO Y FIN
            DateTime inicio = dpInicio.SelectedDate.Value.Date;
            DateTime fin = dpFin.SelectedDate.Value.Date;

            if (!todoElDia)
            {
                inicio += TimeSpan.Parse((string)cmbHoraInicio.SelectedItem!);
                fin += TimeSpan.Parse((string)cmbHoraFin.SelectedItem!);
            }

            // EL FIN DEBE SER POSTERIOR AL INICIO
            bool ordenInvalido = todoElDia ? fin < inicio : fin <= inicio;

            if (ordenInvalido)
            {
                MostrarError("La fecha y hora final deben ser posteriores a las de inicio.");
                return;
            }

            guardando = true;

            if (sender is Button botonGuardar)
                botonGuardar.IsEnabled = false;


            try
            {
                // CONSULTAR DISPONIBILIDAD ANTES DE GUARDAR
                // (todo el día: el servidor cuenta hasta las 00:00 del día siguiente)
                DateTime finConsulta = todoElDia ? fin.AddDays(1) : fin;

                var disponibilidad =
                    await servicio.VerificarDisponibilidadAsync(inicio, finConsulta);

                if (!disponibilidad.Disponible)
                {
                    MostrarError(
                    "El horario seleccionado no está disponible. " +
                    "Por favor, selecciona otro horario.");
                    return;
                }

                // REGISTRAR
                IdCitaCreada = await servicio.RegistrarAsync(
                    paciente.IdPaciente,
                    txtTitulo.Text.Trim(),
                    inicio,
                    fin,
                    Paleta[indiceColor],
                    txtDireccion.Text,
                    txtNotas.Text,
                    todoElDia);

                DialogResult = true;
            }
            catch (HttpRequestException ex)
                when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                MostrarError(ex.Message);
            }
            catch (Exception ex)
            {
                MostrarError("La cita NO se agregó.\n" + ex.Message);
            }
            finally
            {
                guardando = false;

                if (sender is Button boton)
                    boton.IsEnabled = true;
            }
        }

        // MOSTRAR ERROR DE VALIDACIÓN
        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            txtError.Visibility = Visibility.Visible;
        }

        // LIMPIAR ERROR DE VALIDACIÓN
        private void LimpiarError()
        {
            txtError.Text = string.Empty;
            txtError.Visibility = Visibility.Collapsed;
        }

    }
}
