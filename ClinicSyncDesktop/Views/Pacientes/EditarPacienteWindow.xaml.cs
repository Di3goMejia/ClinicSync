
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
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class EditarPacienteWindow : Window
    {
        /// <summary>
        /// The paciente
        /// </summary>
        private readonly PacienteVista paciente;
        /// <summary>
        /// The estado original
        /// </summary>
        private readonly bool estadoOriginal;

        /// <summary>
        /// The guardando
        /// </summary>
        private bool guardando = false;
        /// <summary>
        /// The actualizando fecha
        /// </summary>
        private bool actualizandoFecha = false;

        /// <summary>
        /// The popup calendario
        /// </summary>
        private Popup? popupCalendario;

        /// <summary>
        /// Gets a value indicating whether [error al guardar].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [error al guardar]; otherwise, <c>false</c>.
        /// </value>
        public bool ErrorAlGuardar { get; private set; } = false;

        /// <summary>
        /// Gets or sets the identidad ya registrada.
        /// </summary>
        /// <value>
        /// The identidad ya registrada.
        /// </value>
        public Func<string, bool>? IdentidadYaRegistrada
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EditarPacienteWindow"/> class.
        /// </summary>
        /// <param name="paciente">The paciente.</param>
        public EditarPacienteWindow(PacienteVista paciente)
        {
            InitializeComponent();

            this.paciente = paciente;
            estadoOriginal = paciente.Activo;

            // CARGAR INFORMACIÓN DEL PACIENTE
            txtIdentidad.Text = paciente.Identidad;
            txtNombres.Text = paciente.Nombres;
            txtApellidos.Text = paciente.Apellidos;
            txtTelefono.Text = paciente.Telefono;

            EstablecerFecha(paciente.FechaNacimiento);

            cmbSexo.SelectedIndex =
                paciente.Sexo == "F" ? 1 : 2;

            // CARGAR ESTADO
            chkActivo.IsChecked = paciente.Activo;

            txtEstado.Text = paciente.Activo
                ? "Activo"
                : "Inactivo";

            // BLOQUEAR CAMPOS SI ESTÁ INACTIVO
            ConfigurarCampos();

            // MOSTRAR AVISO SI ESTÁ INACTIVO
            ActualizarAvisoInactivo();
        }

        // =====================================
        // CONFIGURAR CAMPOS SEGÚN ESTADO
        // =====================================
        /// <summary>
        /// Configurars the campos.
        /// </summary>
        private void ConfigurarCampos()
        {
            bool permitirEdicion = estadoOriginal;

            txtIdentidad.IsReadOnly = !permitirEdicion;
            txtNombres.IsReadOnly = !permitirEdicion;
            txtApellidos.IsReadOnly = !permitirEdicion;
            txtTelefono.IsReadOnly = !permitirEdicion;
            txtFechaNacimiento.IsReadOnly = !permitirEdicion;

            cmbSexo.IsEnabled = permitirEdicion;

            // EL INTERRUPTOR SIEMPRE ESTÁ DISPONIBLE
            chkActivo.IsEnabled = true;
        }

        // =====================================
        // AVISO DE PACIENTE INACTIVO
        // =====================================
        /// <summary>
        /// Actualizars the aviso inactivo.
        /// </summary>
        private void ActualizarAvisoInactivo()
        {
            if (chkActivo == null ||
                txtError == null ||
                iconoError == null)
            {
                return;
            }

            if (!estadoOriginal)
            {
                if (chkActivo.IsChecked == true)
                {
                    txtError.Text = "";
                    iconoError.Visibility =
                        Visibility.Collapsed;
                }
                else
                {
                    txtError.Text =
                        "El paciente está desactivado. " +
                        "Actívalo para poder modificar sus datos.";

                    iconoError.Visibility =
                        Visibility.Visible;
                }
            }
        }

        // =====================================
        // CAMBIAR ESTADO
        // =====================================
        /// <summary>
        /// Handles the Changed event of the Estado control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Estado_Changed(
            object sender,
            RoutedEventArgs e)
        {
            if (txtEstado == null || chkActivo == null)
                return;

            txtEstado.Text = chkActivo.IsChecked == true
                ? "Activo"
                : "Inactivo";

            ActualizarAvisoInactivo();
        }

        // =====================================
        // MOSTRAR ERROR
        // =====================================
        /// <summary>
        /// Mostrars the error.
        /// </summary>
        /// <param name="mensaje">The mensaje.</param>
        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;

            iconoError.Visibility =
                Visibility.Visible;
        }

        // =====================================
        // LIMPIAR ERROR
        // =====================================
        /// <summary>
        /// Limpiars the error.
        /// </summary>
        private void LimpiarError()
        {
            txtError.Text = "";

            iconoError.Visibility =
                Visibility.Collapsed;

            ActualizarAvisoInactivo();
        }

        // =====================================
        // OBTENER FECHA
        // =====================================
        /// <summary>
        /// Tries the obtener fecha.
        /// </summary>
        /// <param name="fecha">The fecha.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Establecers the fecha.
        /// </summary>
        /// <param name="fecha">The fecha.</param>
        private void EstablecerFecha(DateTime fecha)
        {
            actualizandoFecha = true;

            txtFechaNacimiento.Text =
                fecha.ToString(
                    "yyyy/MM/dd",
                    CultureInfo.InvariantCulture);

            txtFechaNacimiento.CaretIndex =
                txtFechaNacimiento.Text.Length;

            actualizandoFecha = false;

            LimpiarError();
        }

        // =====================================
        // FORMATO DE FECHA
        // =====================================
        /// <summary>
        /// Handles the TextChanged event of the Fecha control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void Fecha_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (actualizandoFecha || !estadoOriginal)
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

            string texto = numeros;

            if (numeros.Length > 4)
            {
                texto =
                    numeros.Substring(0, 4) + "/" +
                    numeros.Substring(
                        4,
                        Math.Min(2, numeros.Length - 4));

                if (numeros.Length > 6)
                {
                    texto += "/" +
                        numeros.Substring(6);
                }
            }

            txtFechaNacimiento.Text = texto;

            txtFechaNacimiento.CaretIndex =
                texto.Length;

            actualizandoFecha = false;
        }

        // =====================================
        // VALIDAR FECHA
        // =====================================
        /// <summary>
        /// Handles the LostFocus event of the Fecha control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Fecha_LostFocus(
            object sender,
            RoutedEventArgs e)
        {
            if (!estadoOriginal)
                return;

            if (string.IsNullOrWhiteSpace(
                txtFechaNacimiento.Text))
            {
                return;
            }

            if (!TryObtenerFecha(out DateTime fecha))
            {
                MostrarError(
                    "Ingresa una fecha válida (YYYY/MM/DD).");

                return;
            }

            if (fecha.Year < 1900 ||
                fecha.Date > DateTime.Today)
            {
                MostrarError(
                    "Ingresa una fecha de nacimiento válida.");

                return;
            }

            EstablecerFecha(fecha);
        }

        // =====================================
        // ABRIR CALENDARIO
        // =====================================
        /// <summary>
        /// Handles the Click event of the Calendario control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Calendario_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!estadoOriginal)
                return;

            if (sender is not Button boton)
                return;

            if (popupCalendario?.IsOpen == true)
            {
                popupCalendario.IsOpen = false;
                return;
            }

            var calendario =
                new System.Windows.Controls.Calendar
                {
                    DisplayDateStart =
                        new DateTime(1900, 1, 1),

                    DisplayDateEnd =
                        DateTime.Today,

                    SelectionMode =
                        CalendarSelectionMode.SingleDate,

                    Background =
                        Brushes.White
                };

            if (TryObtenerFecha(out DateTime fecha) &&
                fecha.Year >= 1900 &&
                fecha.Date <= DateTime.Today)
            {
                calendario.DisplayDate = fecha;
                calendario.SelectedDate = fecha;
            }

            popupCalendario = new Popup
            {
                PlacementTarget = boton,
                Placement = PlacementMode.Bottom,
                AllowsTransparency = true,
                StaysOpen = false,

                Child = new Border
                {
                    Background = Brushes.White,
                    BorderBrush = Brushes.LightGray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(10),
                    Padding = new Thickness(5),
                    Child = calendario
                }
            };

            calendario.SelectedDatesChanged += (s, args) =>
            {
                if (calendario.SelectedDate.HasValue)
                {
                    EstablecerFecha(
                        calendario.SelectedDate.Value);

                    popupCalendario.IsOpen = false;
                }
            };

            popupCalendario.IsOpen = true;
        }

        // =====================================
        // CERRAR VENTANA
        // =====================================
        /// <summary>
        /// Handles the Click event of the Cerrar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Cerrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (guardando)
                return;

            ConfirmarDescarteWindow confirmar =
                new ConfirmarDescarteWindow
                {
                    Owner = this
                };

            if (confirmar.ShowDialog() == true)
            {
                DialogResult = false;
            }
        }

        // =====================================
        // GUARDAR CAMBIOS
        // =====================================
        /// <summary>
        /// Handles the Click event of the Guardar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Guardar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (guardando)
                return;

            ErrorAlGuardar = false;
            LimpiarError();

            bool nuevoEstado =
                chkActivo.IsChecked == true;

            // =================================
            // PACIENTE ORIGINALMENTE INACTIVO
            // SOLO PERMITIR REACTIVAR
            // =================================
            if (!estadoOriginal)
            {
                if (!nuevoEstado)
                {
                    MostrarError(
                        "El paciente está desactivado. " +
                        "Actívalo para poder modificar sus datos.");

                    return;
                }

                guardando = true;
                btnGuardar.IsEnabled = false;

                try
                {
                    await ClienteApi.CambiarEstadoPacienteAsync(
                        paciente.IdPaciente,
                        true);

                    DialogResult = true;
                }
                catch (Exception ex)
                {
                    ErrorAlGuardar = true;

                    MostrarError(
                        "No se pudo activar el paciente: " +
                        ex.Message);
                }
                finally
                {
                    guardando = false;
                    btnGuardar.IsEnabled = true;
                }

                return;
            }

            // =================================
            // PACIENTE ACTIVO
            // OBTENER INFORMACIÓN
            // =================================
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
            // FORMATEAR INFORMACIÓN
            // =================================
            string identidadFormateada =
                identidad.Substring(0, 4) + "-" +
                identidad.Substring(4, 4) + "-" +
                identidad.Substring(8, 5);

            string telefonoFormateado =
                telefono.Substring(0, 4) + "-" +
                telefono.Substring(4, 4);

            // =================================
            // GUARDAR EN SQL SERVER
            // =================================
            guardando = true;
            btnGuardar.IsEnabled = false;

            try
            {
                using var respuesta =
                    await ClienteApi.PutAutenticadoAsync(
                        $"api/pacientes/{paciente.IdPaciente}",
                        new
                        {
                            Identidad = identidadFormateada,
                            Nombres = nombres,
                            Apellidos = apellidos,
                            FechaNacimiento =
                                fechaNacimiento.Date,
                            Genero = sexo,
                            Telefono = telefonoFormateado
                        });

                if (respuesta.StatusCode ==
                    HttpStatusCode.Conflict)
                {
                    ErrorAlGuardar = true;

                    MostrarError(
                        "Ya existe un paciente con esta identidad " +
                        "o no se puede modificar este registro.");

                    return;
                }

                if (!respuesta.IsSuccessStatusCode)
                {
                    ErrorAlGuardar = true;

                    MostrarError(
                        "No se pudo editar el paciente. Código: " +
                        (int)respuesta.StatusCode);

                    return;
                }

                // CAMBIAR ESTADO DESPUÉS DE
                // GUARDAR CORRECTAMENTE
                if (!nuevoEstado)
                {
                    await ClienteApi.CambiarEstadoPacienteAsync(
                        paciente.IdPaciente,
                        false);
                }

                DialogResult = true;
            }
            catch (Exception ex)
            {
                ErrorAlGuardar = true;

                MostrarError(
                    "No se pudo guardar el paciente: " +
                    ex.Message);
            }
            finally
            {
                guardando = false;
                btnGuardar.IsEnabled = true;
            }
        }
    }
}
