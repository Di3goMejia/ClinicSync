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
    public partial class ModificarCitaWindow : Window
    {
        private static readonly string[] Paleta =
        {
            "#8BC34A", "#E91E63", "#FF9800", "#3F51B5",
            "#009688", "#9C27B0", "#F44336", "#00BCD4"
        };

        private readonly AgendaApiService servicio = new();
        private readonly int idEvento;

        private int? idPacienteSeleccionado;
        private bool cerrada;
        private int indiceColor;
        private bool guardando;
        private Popup? popupCalendario;

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

                // En citas de todo el día la base guarda el fin como el día siguiente a las 00:00.
                // Se resta un día para mostrar el último día real de la cita.
                dpFin.SelectedDate = cita.TodoElDia
                    ? cita.FechaFinal.Date.AddDays(-1)
                    : cita.FechaFinal.Date;

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
                "¿Seguro que desea descartar los cambios?")
            {
                Owner = this
            };

            if (advertencia.ShowDialog() == true)
                DialogResult = false;
        }

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

            DateTime inicio = dpInicio.SelectedDate.Value.Date;
            DateTime fin = dpFin.SelectedDate.Value.Date;

            // AGREGAR HORAS SI NO ES TODO EL DÍA
            if (!todoElDia)
            {
                if (cmbHoraInicio.SelectedItem == null ||
                    cmbHoraFin.SelectedItem == null)
                {
                    MostrarError(
                        "Debe seleccionar la hora de inicio y finalización.");
                    return;
                }

                inicio = inicio.Add(
                    TimeSpan.Parse(cmbHoraInicio.SelectedItem.ToString()!));

                fin = fin.Add(
                    TimeSpan.Parse(cmbHoraFin.SelectedItem.ToString()!));
            }

            // VALIDAR ORDEN DE FECHAS
            bool ordenInvalido = todoElDia
                ? fin < inicio
                : fin <= inicio;

            if (ordenInvalido)
            {
                MostrarError(
                    "La fecha y hora final deben ser posteriores al inicio.");
                return;
            }

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