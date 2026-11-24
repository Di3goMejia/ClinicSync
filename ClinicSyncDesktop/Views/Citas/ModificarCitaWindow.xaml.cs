using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Validations;
using ClinicSyncDesktop.Views;

namespace ClinicSyncDesktop.Views.Citas
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class ModificarCitaWindow : Window
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
        /// The identifier evento
        /// </summary>
        private readonly int idEvento;

        /// <summary>
        /// The identifier paciente seleccionado
        /// </summary>
        private int? idPacienteSeleccionado;
        /// <summary>
        /// The cerrada
        /// </summary>
        private bool cerrada;
        /// <summary>
        /// The indice color
        /// </summary>
        private int indiceColor;
        /// <summary>
        /// The guardando
        /// </summary>
        private bool guardando;
        /// <summary>
        /// The popup calendario
        /// </summary>
        private Popup? popupCalendario;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModificarCitaWindow"/> class.
        /// </summary>
        /// <param name="idEvento">The identifier evento.</param>
        public ModificarCitaWindow(int idEvento)
        {
            InitializeComponent();

            this.idEvento = idEvento;

            var horas = new List<string>();

            for (int h = 0; h < 24; h++)
            {
                horas.Add($"{h:00}:00");
                horas.Add($"{h:00}:30");
            }

            cmbHoraInicio.ItemsSource = horas;
            cmbHoraFin.ItemsSource = horas;

            Loaded += ModificarCitaWindow_Loaded;
            Closed += (_, _) => cerrada = true;
        }

        /// <summary>
        /// Handles the Loaded event of the ModificarCitaWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ModificarCitaWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                EventoAgenda cita = await servicio.ObtenerPorIdAsync(idEvento);

                if (cerrada)
                    return;

                idPacienteSeleccionado = cita.IdPaciente;

                if (idPacienteSeleccionado is int id)
                {
                    txtPacienteSeleccionado.Text = string.IsNullOrWhiteSpace(cita.Paciente)
                        ? $"Paciente #{id}"
                        : cita.Paciente;
                }

                txtTitulo.Text = cita.Titulo;
                txtDireccion.Text = cita.Direccion ?? "";
                txtNotas.Text = cita.Notas ?? "";

                int colorEncontrado = Array.IndexOf(Paleta, cita.Color);
                indiceColor = colorEncontrado >= 0 ? colorEncontrado : 0;

                btnColor.Background = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(Paleta[indiceColor]));

                dpInicio.SelectedDate = cita.FechaInicio.Date;
                dpFin.SelectedDate = cita.FechaFinal.Date;

                cmbHoraInicio.SelectedItem = cita.FechaInicio.ToString("HH:mm");
                cmbHoraFin.SelectedItem = cita.FechaFinal.ToString("HH:mm");

                chkTodoElDia.IsChecked = cita.TodoElDia;
                cmbHoraInicio.IsEnabled = !cita.TodoElDia;
                cmbHoraFin.IsEnabled = !cita.TodoElDia;

                LimpiarError();

                btnSeleccionarPaciente.IsEnabled = true;
                btnGuardarCambios.IsEnabled = true;
            }
            catch (Exception ex)
            {
                if (cerrada)
                    return;

                MessageBox.Show(
                    "No fue posible cargar los datos de la cita.\n\n" + ex.Message,
                    "ClinicSync",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
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
            btnGuardarCambios.IsEnabled = false;
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

                var catalogo = pacientes.Select(paciente => new PacienteIngreso
                {
                    IdPaciente = paciente.IdPaciente,
                    Nombres = paciente.Nombres,
                    Apellidos = paciente.Apellidos,
                    Activo = paciente.Activo
                }).ToList();

                var seleccionActual = catalogo.FirstOrDefault(
                    paciente => paciente.IdPaciente == idPacienteSeleccionado);

                var selector = new SeleccionIngresoWindow(catalogo, seleccionActual)
                {
                    Owner = this
                };

                if (selector.ShowDialog() == true &&
                    selector.PacienteElegido is PacienteIngreso elegido)
                {
                    idPacienteSeleccionado = elegido.IdPaciente;
                    txtPacienteSeleccionado.Text = elegido.NombreCompleto.Trim();
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
                    btnGuardarCambios.IsEnabled = true;
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
                "¿Seguro que desea descartar los cambios?")
            {
                Owner = this
            };

            if (advertencia.ShowDialog() == true)
                DialogResult = false;
        }

        /// <summary>
        /// Handles the Click event of the GuardarCambios control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void GuardarCambios_Click(object sender, RoutedEventArgs e)
        {
            if (guardando)
                return;

            LimpiarError();

            bool todoElDia = chkTodoElDia.IsChecked == true;
            int? idPaciente = idPacienteSeleccionado;

            var resultado = new ValidadorCita().Validar(new EntradaCita(
                idPaciente,
                txtTitulo.Text.Trim(),
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
                resultado, controles, MostrarError, LimpiarError) || idPaciente is null)
            {
                return;
            }

            DateTime inicio = resultado.Inicio;
            DateTime fin = resultado.Fin;

            guardando = true;

            try
            {
                DateTime finConsulta = todoElDia ? fin.AddDays(1) : fin;

                DisponibilidadAgenda disponibilidad =
                    await servicio.VerificarDisponibilidadAsync(
                        inicio, finConsulta, idEventoExcluir: idEvento);

                if (!disponibilidad.Disponible)
                {
                    MostrarError("El horario seleccionado no está disponible.");
                    return;
                }

                await servicio.ModificarAsync(
                    idEvento,
                    idPaciente.Value,
                    txtTitulo.Text.Trim(),
                    inicio,
                    fin,
                    Paleta[indiceColor],
                    txtDireccion.Text,
                    txtNotas.Text,
                    todoElDia);

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MostrarError("No fue posible modificar la cita: " + ex.Message);
            }
            finally
            {
                guardando = false;
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