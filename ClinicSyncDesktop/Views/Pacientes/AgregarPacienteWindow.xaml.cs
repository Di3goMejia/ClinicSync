
using System;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Validations;

namespace ClinicSyncDesktop.Views
{
    public partial class AgregarPacienteWindow : Window
    {
        public PacienteVista? PacienteCreado
        {
            get;
            private set;
        }

        // INDICA SI FALLÓ EL REGISTRO EN EL SERVIDOR
        public bool ErrorAlRegistrar { get; private set; } = false;

        public Func<string, bool>? IdentidadYaRegistrada
        {
            get;
            set;
        }

        private Popup? popupCalendario;

        private bool actualizandoFecha = false;

        private bool guardando = false;

        public AgregarPacienteWindow()
        {
            InitializeComponent();

            dpNacimiento.DisplayDateStart =
                new DateTime(1900, 1, 1);

            dpNacimiento.DisplayDateEnd =
                DateTime.Today;

            txtPlaceholderFecha.Visibility =
                Visibility.Visible;
        }

        // =====================================
        // ABRIR CALENDARIO
        // =====================================
        private void Calendario_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button boton)
                return;

            if (popupCalendario != null &&
                popupCalendario.IsOpen)
            {
                popupCalendario.IsOpen = false;
                return;
            }

            System.Windows.Controls.Calendar calendario =
                new System.Windows.Controls.Calendar
                {
                    DisplayDateStart =
                        new DateTime(1900, 1, 1),

                    DisplayDateEnd =
                        DateTime.Today,

                    DisplayMode =
                        CalendarMode.Month,

                    SelectionMode =
                        CalendarSelectionMode.SingleDate,

                    Background =
                        Brushes.White
                };

            if (TryObtenerFecha(out DateTime fechaEscrita)
                && FechaEnRango(fechaEscrita))
            {
                calendario.DisplayDate = fechaEscrita;
                calendario.SelectedDate = fechaEscrita;
            }
            else if (dpNacimiento.SelectedDate.HasValue)
            {
                calendario.DisplayDate =
                    dpNacimiento.SelectedDate.Value;

                calendario.SelectedDate =
                    dpNacimiento.SelectedDate.Value;
            }

            Border contenedor = new Border
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

            calendario.SelectedDatesChanged +=
                (s, args) =>
                {
                    if (calendario.SelectedDate.HasValue)
                    {
                        DateTime fecha =
                            calendario.SelectedDate.Value.Date;

                        if (FechaEnRango(fecha))
                        {
                            EstablecerFecha(fecha);

                            popupCalendario.IsOpen = false;
                        }
                    }
                };

            popupCalendario.IsOpen = true;
        }

        // =====================================
        // VALIDAR RANGO DE FECHA
        // =====================================
        private bool FechaEnRango(DateTime fecha)
        {
            return fecha.Date <= DateTime.Today &&
                   fecha.Year >= 1900;
        }

        // =====================================
        // OBTENER FECHA ESCRITA
        // =====================================
        private bool TryObtenerFecha(out DateTime fecha)
        {
            return DateTime.TryParseExact(
                txtFechaNacimiento.Text.Trim(),
                "yyyy/MM/dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out fecha);
        }

        // =====================================
        // ESTABLECER FECHA
        // =====================================
        private void EstablecerFecha(DateTime fecha)
        {
            actualizandoFecha = true;

            dpNacimiento.SelectedDate = fecha.Date;

            txtFechaNacimiento.Text =
                fecha.ToString(
                    "yyyy/MM/dd",
                    CultureInfo.InvariantCulture);

            txtFechaNacimiento.CaretIndex =
                txtFechaNacimiento.Text.Length;

            txtPlaceholderFecha.Visibility =
                Visibility.Collapsed;

            actualizandoFecha = false;

            LimpiarError();
        }

        // =====================================
        // FORMATO AUTOMÁTICO DE FECHA
        // =====================================
        private void txtFechaNacimiento_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (actualizandoFecha)
                return;

            if (txtPlaceholderFecha == null ||
                dpNacimiento == null)
                return;

            actualizandoFecha = true;

            string numeros = Regex.Replace(
                txtFechaNacimiento.Text,
                @"\D",
                "");

            if (numeros.Length > 8)
            {
                numeros = numeros.Substring(0, 8);
            }

            string fechaFormateada = numeros;

            if (numeros.Length >= 4)
            {
                fechaFormateada =
                    numeros.Substring(0, 4) + "/";

                if (numeros.Length > 4)
                {
                    int cantidadMes = Math.Min(
                        numeros.Length - 4, 2);

                    fechaFormateada +=
                        numeros.Substring(4, cantidadMes);

                    if (numeros.Length >= 6)
                    {
                        fechaFormateada += "/";

                        if (numeros.Length > 6)
                        {
                            fechaFormateada +=
                                numeros.Substring(6);
                        }
                    }
                }
            }

            txtFechaNacimiento.Text =
                fechaFormateada;

            txtFechaNacimiento.CaretIndex =
                fechaFormateada.Length;

            txtPlaceholderFecha.Visibility =
                string.IsNullOrEmpty(fechaFormateada)
                ? Visibility.Visible
                : Visibility.Collapsed;

            dpNacimiento.SelectedDate = null;

            actualizandoFecha = false;
        }

        // =====================================
        // VALIDAR FECHA AL SALIR
        // =====================================
        private void txtFechaNacimiento_LostFocus(
            object sender,
            RoutedEventArgs e)
        {
            string texto =
                txtFechaNacimiento.Text.Trim();

            if (string.IsNullOrWhiteSpace(texto))
                return;

            if (!TryObtenerFecha(out DateTime fecha))
            {
                MostrarError(
                    "Ingresa una fecha válida (YYYY/MM/DD).");

                return;
            }

            if (fecha.Date > DateTime.Today)
            {
                MostrarError(
                    "La fecha de nacimiento no puede ser posterior a hoy.");

                return;
            }

            if (fecha.Year < 1900)
            {
                MostrarError(
                    "Ingresa una fecha de nacimiento válida.");

                return;
            }

            EstablecerFecha(fecha);
        }

        // =====================================
        // FECHA SELECCIONADA
        // =====================================
        private void dpNacimiento_SelectedDateChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (actualizandoFecha ||
                dpNacimiento.SelectedDate == null)
                return;

            DateTime fecha =
                dpNacimiento.SelectedDate.Value.Date;

            if (!FechaEnRango(fecha))
            {
                dpNacimiento.SelectedDate = null;

                MostrarError(
                    "La fecha de nacimiento no puede ser posterior a hoy.");

                return;
            }

            EstablecerFecha(fecha);
        }

        // =====================================
        // LIMPIAR ERROR
        // =====================================
        private void LimpiarError()
        {
            txtError.Text = "";

            iconoError.Visibility =
                Visibility.Collapsed;
        }

        // =====================================
        // MOSTRAR ERROR EN EL FORMULARIO
        // =====================================
        private void MostrarError(
            string mensaje,
            Control? campo = null)
        {
            txtError.Text = mensaje;

            iconoError.Visibility =
                Visibility.Visible;
        }

        // =====================================
        // CERRAR Y CONFIRMAR DESCARTE
        // =====================================
        private void Cerrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (guardando)
                return;

            ConfirmarDescarteWindow ventana =
                new ConfirmarDescarteWindow();

            ventana.Owner = this;

            bool? resultado = ventana.ShowDialog();

            if (resultado == true)
            {
                DialogResult = false;
            }
        }

        // =====================================
        // AGREGAR PACIENTE
        // =====================================
        private async void Agregar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (guardando)
                return;

            // REINICIAR EL ESTADO DEL ERROR
            ErrorAlRegistrar = false;

            LimpiarError();

            string identidadTexto =
                txtIdentidad.Text.Trim();

            string identidad = Regex.Replace(
                identidadTexto,
                @"[ -]",
                "");

            string nombres = Regex.Replace(
                txtNombres.Text.Trim(),
                @"\s+",
                " ");

            string apellidos = Regex.Replace(
                txtApellidos.Text.Trim(),
                @"\s+",
                " ");

            string telefonoTexto =
                txtTelefono.Text.Trim();

            string telefono =
                telefonoTexto.Replace("-", "");

            string? sexoSeleccionado =
                (cmbSexo.SelectedItem as ComboBoxItem)?.Tag as string;
            var resultadoValidacion = new ValidadorPaciente().Validar(
                new EntradaPaciente(identidadTexto, nombres, apellidos,
                    txtFechaNacimiento.Text, sexoSeleccionado, telefonoTexto));

            if (!HelperValidacion.AplicarResultado(
                resultadoValidacion,
                new Dictionary<string, Control>
                {
                    [nameof(EntradaPaciente.Identidad)] = txtIdentidad,
                    [nameof(EntradaPaciente.Nombres)] = txtNombres,
                    [nameof(EntradaPaciente.Apellidos)] = txtApellidos,
                    [nameof(EntradaPaciente.TextoFechaNacimiento)] = txtFechaNacimiento,
                    [nameof(EntradaPaciente.Sexo)] = cmbSexo,
                    [nameof(EntradaPaciente.Telefono)] = txtTelefono
                },
                mensaje => MostrarError(mensaje),
                LimpiarError))
                return;

            identidad = resultadoValidacion.IdentidadNormalizada;
            nombres = resultadoValidacion.NombresNormalizados;
            apellidos = resultadoValidacion.ApellidosNormalizados;
            telefono = resultadoValidacion.TelefonoNormalizado;

            if (IdentidadYaRegistrada?.Invoke(identidad) == true)
            {
                MostrarError(
                    "Ya existe un paciente con esta identidad.");

                return;
            }

            DateTime fechaNacimiento = resultadoValidacion.FechaNacimiento;
            string sexo = sexoSeleccionado!;

            // =================================
            // PREPARAR PACIENTE
            // =================================
            PacienteVista nuevoPaciente =
                new PacienteVista
                {
                    Identidad =
                        identidad.Substring(0, 4) + "-" +
                        identidad.Substring(4, 4) + "-" +
                        identidad.Substring(8, 5),

                    Nombres = nombres,

                    Apellidos = apellidos,

                    FechaNacimiento =
                        fechaNacimiento.Date,

                    Sexo = sexo,

                    Telefono =
                        telefono.Substring(0, 4) + "-" +
                        telefono.Substring(4),

                    Activo = true
                };

            // =================================
            // GUARDAR EN SQL SERVER
            // =================================
            guardando = true;

            if (sender is Button botonGuardar)
                botonGuardar.IsEnabled = false;

            try
            {
                using var respuesta =
                    await ClienteApi.PostAutenticadoAsync(
                        "api/pacientes",
                        new
                        {
                            nuevoPaciente.Identidad,
                            nuevoPaciente.Nombres,
                            nuevoPaciente.Apellidos,
                            nuevoPaciente.FechaNacimiento,
                            Genero = nuevoPaciente.Sexo,
                            nuevoPaciente.Telefono
                        });

                // IDENTIDAD DUPLICADA EN SQL SERVER
                if (respuesta.StatusCode ==
                    HttpStatusCode.Conflict)
                {
                    ErrorAlRegistrar = true;

                    MostrarError(
                        "Ya existe un paciente con esta identidad.");

                    return;
                }

                // ERROR DEL SERVIDOR
                if (!respuesta.IsSuccessStatusCode)
                {
                    ErrorAlRegistrar = true;

                    MostrarError(
                        "No se pudo guardar el paciente. " +
                        "Código del servidor: " +
                        (int)respuesta.StatusCode);

                    return;
                }

                // REGISTRO EXITOSO
                ErrorAlRegistrar = false;

                PacienteCreado = nuevoPaciente;

                DialogResult = true;
            }
            catch (Exception ex)
            {
                ErrorAlRegistrar = true;

                MostrarError(
                    "No se pudo guardar el paciente: " +
                    ex.Message);
            }
            finally
            {
                guardando = false;

                if (sender is Button boton)
                    boton.IsEnabled = true;
            }
        }
    }
}
