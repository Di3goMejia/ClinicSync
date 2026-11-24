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
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class AgregarCitaWindow : Window
    {
        /// <summary>
        /// The paleta
        /// </summary>
        private static readonly string[] Paleta =
        {
            "#8BC34A", "#E91E63", "#FF9800", "#3F51B5",
            "#009688", "#9C27B0", "#F44336", "#00BCD4"
        };

        /// <summary>
        /// The servicio
        /// </summary>
        private readonly AgendaApiService servicio = new();

        /// <summary>
        /// The paciente seleccionado
        /// </summary>
        private PacienteOpcion? pacienteSeleccionado;
        /// <summary>
        /// The popup calendario
        /// </summary>
        private Popup? popupCalendario;
        /// <summary>
        /// The indice color
        /// </summary>
        private int indiceColor;
        /// <summary>
        /// The guardando
        /// </summary>
        private bool guardando;
        /// <summary>
        /// The cerrada
        /// </summary>
        private bool cerrada;

        /// <summary>
        /// Gets the identifier cita creada.
        /// </summary>
        /// <value>
        /// The identifier cita creada.
        /// </value>
        public int IdCitaCreada { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgregarCitaWindow"/> class.
        /// </summary>
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

        /// <summary>
        /// Handles the Click event of the SeleccionarPaciente control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the Click event of the Color control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Color_Click(object sender, RoutedEventArgs e)
        {
            indiceColor = (indiceColor + 1) % Paleta.Length;

            btnColor.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(Paleta[indiceColor]));
        }

        /// <summary>
        /// Mostrars the fecha.
        /// </summary>
        /// <param name="texto">The texto.</param>
        /// <param name="fecha">The fecha.</param>
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

        /// <summary>
        /// Handles the SelectedDateChanged event of the dpInicio control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the SelectedDateChanged event of the dpFin control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void dpFin_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            MostrarFecha(txtFechaFin, dpFin.SelectedDate);
        }

        /// <summary>
        /// Handles the Click event of the Fecha control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the Click event of the TodoElDia control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void TodoElDia_Click(object sender, RoutedEventArgs e)
        {
            bool todoElDia = chkTodoElDia.IsChecked == true;
            cmbHoraInicio.IsEnabled = !todoElDia;
            cmbHoraFin.IsEnabled = !todoElDia;
        }

        /// <summary>
        /// Handles the Click event of the Cerrar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the Click event of the Agregar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Mostrars the error.
        /// </summary>
        /// <param name="mensaje">The mensaje.</param>
        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            txtError.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// Limpiars the error.
        /// </summary>
        private void LimpiarError()
        {
            txtError.Text = string.Empty;
            txtError.Visibility = Visibility.Collapsed;
        }
    }
}