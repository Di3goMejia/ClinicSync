using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;

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

        private int indiceColor = 0;
        private bool guardando = false;
        private Popup? popupCalendario;

        public ModificarCitaWindow(int idEvento)
        {
            InitializeComponent();

            this.idEvento = idEvento;

            // Cargar horas cada 30 minutos.
            var horas = new List<string>();

            for (int h = 0; h < 24; h++)
            {
                horas.Add($"{h:00}:00");
                horas.Add($"{h:00}:30");
            }

            cmbHoraInicio.ItemsSource = horas;
            cmbHoraFin.ItemsSource = horas;

            Loaded += ModificarCitaWindow_Loaded;
        }

        // CARGAR INFORMACIÓN DE LA CITA
        private async void ModificarCitaWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                // CARGAR PACIENTES ACTIVOS
                cmbPaciente.ItemsSource =
                    await servicio.ListarPacientesActivosAsync();

                // OBTENER LA CITA SELECCIONADA
                EventoAgenda cita =
                    await servicio.ObtenerPorIdAsync(idEvento);

                // SELECCIONAR PACIENTE
                if (cita.IdPaciente.HasValue)
                {
                    foreach (PacienteOpcion paciente in
                             (List<PacienteOpcion>)cmbPaciente.ItemsSource)
                    {
                        if (paciente.IdPaciente == cita.IdPaciente.Value)
                        {
                            cmbPaciente.SelectedItem = paciente;
                            break;
                        }
                    }
                }

                // CARGAR INFORMACIÓN GENERAL
                txtTitulo.Text = cita.Titulo;
                txtDireccion.Text = cita.Direccion ?? "";
                txtNotas.Text = cita.Notas ?? "";

                // CARGAR COLOR
                int colorEncontrado = Array.IndexOf(Paleta, cita.Color);

                indiceColor = colorEncontrado >= 0 ? colorEncontrado : 0;

                btnColor.Background = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString(
                        Paleta[indiceColor]));

                // CARGAR FECHAS
                dpInicio.SelectedDate = cita.FechaInicio.Date;
                dpFin.SelectedDate = cita.FechaFinal.Date;

                // CARGAR HORAS
                cmbHoraInicio.SelectedItem =
                    cita.FechaInicio.ToString("HH:mm");

                cmbHoraFin.SelectedItem =
                    cita.FechaFinal.ToString("HH:mm");

                // CARGAR OPCIÓN TODO EL DÍA
                chkTodoElDia.IsChecked = cita.TodoElDia;

                cmbHoraInicio.IsEnabled = !cita.TodoElDia;
                cmbHoraFin.IsEnabled = !cita.TodoElDia;

                LimpiarError();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No fue posible cargar los datos de la cita.\n\n" + ex.Message,
                    "ClinicSync",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                Close();
            }
        }


        // CAMBIAR COLOR
        private void Color_Click(object sender, RoutedEventArgs e)
        {
            indiceColor = (indiceColor + 1) % Paleta.Length;

            btnColor.Background = new SolidColorBrush(
                (Color)ColorConverter.ConvertFromString(
                    Paleta[indiceColor]));
        }

        // MOSTRAR FECHA
        private static void MostrarFecha(
            TextBlock texto,
            DateTime? fecha)
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

        // CAMBIO DE FECHA DE INICIO
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

        // CAMBIO DE FECHA FINAL
        private void dpFin_SelectedDateChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            MostrarFecha(txtFechaFin, dpFin.SelectedDate);
        }

        // ABRIR CALENDARIO
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
                BorderBrush = new SolidColorBrush(
                    Color.FromRgb(217, 221, 216)),
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
                    destino.SelectedDate =
                        calendario.SelectedDate.Value.Date;

                    popupCalendario!.IsOpen = false;
                }
            };

            popupCalendario.IsOpen = true;
        }

        // TODO EL DÍA
        private void TodoElDia_Click(object sender, RoutedEventArgs e)
        {
            bool todoElDia = chkTodoElDia.IsChecked == true;

            cmbHoraInicio.IsEnabled = !todoElDia;
            cmbHoraFin.IsEnabled = !todoElDia;
        }

        // CERRAR VENTANA
        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            if (guardando)
                return;

            ConfirmarDescarteWindow advertencia =
                new ConfirmarDescarteWindow(
                    "¿Seguro que desea descartar los cambios?")
                {
                    Owner = this
                };

            if (advertencia.ShowDialog() == true)
                DialogResult = false;
        }

        // GUARDAR CAMBIOS
        private async void GuardarCambios_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (guardando)
                return;

            LimpiarError();

            // VALIDAR PACIENTE
            if (cmbPaciente.SelectedItem is not PacienteOpcion paciente)
            {
                MostrarError("Debe seleccionar un paciente.");
                return;
            }

            // VALIDAR TÍTULO
            if (string.IsNullOrWhiteSpace(txtTitulo.Text))
            {
                MostrarError("Debe ingresar el título de la cita.");
                return;
            }

            // VALIDAR FECHAS
            if (dpInicio.SelectedDate == null ||
                dpFin.SelectedDate == null)
            {
                MostrarError("Debe seleccionar las fechas de la cita.");
                return;
            }

            bool todoElDia = chkTodoElDia.IsChecked == true;

            DateTime inicio = dpInicio.SelectedDate.Value.Date;
            DateTime fin = dpFin.SelectedDate.Value.Date;

            // AGREGAR HORAS SI NO ES TODO EL DÍA
            if (!todoElDia)
            {
                if (cmbHoraInicio.SelectedItem == null ||
                    cmbHoraFin.SelectedItem == null)
                {
                    MostrarError("Debe seleccionar la hora de inicio y finalización.");
                    return;
                }

                inicio = inicio.Add(
                    TimeSpan.Parse(cmbHoraInicio.SelectedItem.ToString()!));

                fin = fin.Add(
                    TimeSpan.Parse(cmbHoraFin.SelectedItem.ToString()!));
            }

            // VALIDAR ORDEN DE FECHAS
            if (fin <= inicio)
            {
                MostrarError("La fecha y hora final deben ser posteriores al inicio.");
                return;
            }

            guardando = true;

            try
            {
                // VERIFICAR DISPONIBILIDAD
                DateTime finConsulta = todoElDia
                    ? fin.AddDays(1)
                    : fin;

                DisponibilidadAgenda disponibilidad =
                    await servicio.VerificarDisponibilidadAsync(
                        inicio,
                        finConsulta,
                        idEventoExcluir: idEvento);

                if (!disponibilidad.Disponible)
                {
                    MostrarError("El horario seleccionado no está disponible.");
                    return;
                }

                // MODIFICAR CITA
                await servicio.ModificarAsync(
                    idEvento,
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
            catch (Exception ex)
            {
                MostrarError(
                    "No fue posible modificar la cita: " + ex.Message);
            }
            finally
            {
                guardando = false;
            }
        }

        // MOSTRAR ERROR
        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            txtError.Visibility = Visibility.Visible;
        }

        // LIMPIAR ERROR
        private void LimpiarError()
        {
            txtError.Text = string.Empty;
            txtError.Visibility = Visibility.Collapsed;
        }
    }
}
