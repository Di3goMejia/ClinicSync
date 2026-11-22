using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Validations;

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

        private PacienteOpcion? pacienteSeleccionado;
        private Popup? popupCalendario;
        private int indiceColor;
        private bool guardando;
        private bool cerrada;

        public int IdCitaCreada { get; private set; }

        public AgregarCitaWindow()
        {
            InitializeComponent();

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

            Closed += (_, _) => cerrada = true;
        }

        private async void SeleccionarPaciente_Click(object sender, RoutedEventArgs e)
        {
            if (guardando || !btnSeleccionarPaciente.IsEnabled)
                return;

            btnSeleccionarPaciente.IsEnabled = false;
            btnAgregar.IsEnabled = false;
            LimpiarError();

            try
            {
                var pacientes = await servicio.ListarPacientesActivosAsync();

                if (cerrada)
                    return;

                if (pacientes.Count == 0)
                {
                    MostrarError("No hay pacientes activos para seleccionar.");
                    return;
                }

                var catalogo = pacientes.Select(p => new PacienteIngreso
                {
                    IdPaciente = p.IdPaciente,
                    Nombres = p.Nombres,
                    Apellidos = p.Apellidos,
                    Activo = p.Activo
                }).ToList();

                var seleccionActual = catalogo.FirstOrDefault(
                    p => p.IdPaciente == pacienteSeleccionado?.IdPaciente);

                var selector = new SeleccionIngresoWindow(catalogo, seleccionActual)
                {
                    Owner = this
                };

                if (selector.ShowDialog() == true &&
                    selector.PacienteElegido is PacienteIngreso elegido)
                {
                    var paciente = pacientes.First(
                        p => p.IdPaciente == elegido.IdPaciente);

                    pacienteSeleccionado = paciente;
                    txtPacienteSeleccionado.Text = paciente.NombreCompleto;
                }
            }
            catch (Exception ex)
            {
                if (!cerrada)
                    MostrarError("No se pudo abrir el selector de pacientes. " + ex.Message);
            }
            finally
            {
                if (!cerrada)
                {
                    btnSeleccionarPaciente.IsEnabled = true;
                    btnAgregar.IsEnabled = true;
                }
            }
        }

        private void Color_Click(object sender, RoutedEventArgs e)
        {
            indiceColor = (indiceColor + 1) % Paleta.Length;

            btnColor.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(Paleta[indiceColor]));
        }

        private static void MostrarFecha(TextBlock texto, DateTime? fecha)
        {
            if (fecha is null)
            {
                texto.Text = "Seleccione una fecha";
                texto.Foreground = new SolidColorBrush(Color.FromRgb(174, 176, 177));
            }
            else
            {
                texto.Text = fecha.Value.ToString("dd/MM/yyyy");
                texto.Foreground = new SolidColorBrush(Color.FromRgb(23, 71, 90));
            }
        }

        private void dpInicio_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            MostrarFecha(txtFechaInicio, dpInicio.SelectedDate);

            if (dpInicio.SelectedDate != null &&
                (dpFin.SelectedDate == null ||
                 dpFin.SelectedDate < dpInicio.SelectedDate))
            {
                dpFin.SelectedDate = dpInicio.SelectedDate;
            }
        }

        private void dpFin_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            MostrarFecha(txtFechaFin, dpFin.SelectedDate);
        }

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

        private void TodoElDia_Click(object sender, RoutedEventArgs e)
        {
            bool todoElDia = chkTodoElDia.IsChecked == true;
            cmbHoraInicio.IsEnabled = !todoElDia;
            cmbHoraFin.IsEnabled = !todoElDia;
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            if (guardando)
                return;

            var advertencia = new ConfirmarDescarteWindow(
                "¿Seguro que desea descartar esta cita?")
            {
                Owner = this
            };

            if (advertencia.ShowDialog() == true)
                DialogResult = false;
        }

        private async void Agregar_Click(object sender, RoutedEventArgs e)
        {
            if (guardando)
                return;

            LimpiarError();

            bool todoElDia = chkTodoElDia.IsChecked == true;
            var paciente = pacienteSeleccionado;
            string titulo = txtTitulo.Text.Trim();

            var resultado = new ValidadorCita().Validar(new EntradaCita(
                paciente,
                titulo,
                dpInicio.SelectedDate,
                dpFin.SelectedDate,
                todoElDia,
                cmbHoraInicio.SelectedItem as string,
                cmbHoraFin.SelectedItem as string));

            var controles = new Dictionary<string, Control>
            {
                [nameof(EntradaCita.Paciente)] = btnSeleccionarPaciente,
                [nameof(EntradaCita.Titulo)] = txtTitulo,
                [nameof(EntradaCita.FechaInicio)] = btnFechaInicio,
                [nameof(EntradaCita.FechaFin)] = btnFechaFin,
                [nameof(EntradaCita.HoraInicio)] = cmbHoraInicio,
                [nameof(EntradaCita.HoraFin)] = cmbHoraFin
            };

            if (!HelperValidacion.AplicarResultado(
                resultado, controles, MostrarError, LimpiarError) || paciente is null)
            {
                return;
            }

            DateTime inicio = resultado.Inicio;
            DateTime fin = resultado.Fin;
            string direccion = txtDireccion.Text;
            string notas = txtNotas.Text;
            string color = Paleta[indiceColor];

            guardando = true;

            if (sender is Button botonGuardar)
                botonGuardar.IsEnabled = false;

            try
            {
                DateTime finConsulta = todoElDia ? fin.AddDays(1) : fin;

                var disponibilidad = await servicio.VerificarDisponibilidadAsync(
                    inicio, finConsulta);

                if (!disponibilidad.Disponible)
                {
                    MostrarError(
                        "El horario seleccionado no está disponible. " +
                        "Por favor, selecciona otro horario.");
                    return;
                }

                IdCitaCreada = await servicio.RegistrarAsync(
                    paciente.IdPaciente,
                    titulo,
                    inicio,
                    fin,
                    color,
                    direccion,
                    notas,
                    todoElDia);

                DialogResult = true;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                MostrarError(ex.Message);
            }
            catch (Exception ex)
            {
                MostrarError("No se pudo agregar la cita.\n" + ex.Message);
            }
            finally
            {
                guardando = false;

                if (sender is Button boton)
                    boton.IsEnabled = true;
            }
        }

        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            txtError.Visibility = Visibility.Visible;
        }

        private void LimpiarError()
        {
            txtError.Text = string.Empty;
            txtError.Visibility = Visibility.Collapsed;
        }
    }
}