
using System;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ClinicSyncDesktop.Data;

namespace ClinicSyncDesktop.Views
{
    public partial class EditarPacienteWindow : Window
    {
        private readonly PacienteVista paciente;
        private readonly bool estadoOriginal;

        private bool guardando = false;
        private bool actualizandoFecha = false;

        private Popup? popupCalendario;

        public bool ErrorAlGuardar { get; private set; } = false;

        public Func<string, bool>? IdentidadYaRegistrada
        {
            get;
            set;
        }

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

            // BLOQUEAR LOS CAMPOS SI ESTÁ INACTIVO
            ConfigurarCampos();
        }

        // =====================================
        // BLOQUEAR CAMPOS SEGÚN ESTADO ORIGINAL
        // =====================================
        private void ConfigurarCampos()
        {
            bool permitirEdicion = estadoOriginal;

            txtIdentidad.IsReadOnly = !permitirEdicion;
            txtNombres.IsReadOnly = !permitirEdicion;
            txtApellidos.IsReadOnly = !permitirEdicion;
            txtTelefono.IsReadOnly = !permitirEdicion;

            txtFechaNacimiento.IsReadOnly = !permitirEdicion;

            cmbSexo.IsEnabled = permitirEdicion;

            // El estado siempre puede modificarse.
            chkActivo.IsEnabled = true;

            // Los campos inactivos siguen visibles.
            // No modificamos su diseño ni sus colores.
        }

        // =====================================
        // CAMBIAR TEXTO DEL ESTADO
        // =====================================
        private void Estado_Changed(
            object sender,
            RoutedEventArgs e)
        {
            if (txtEstado == null || chkActivo == null)
                return;

            txtEstado.Text = chkActivo.IsChecked == true
                ? "Activo"
                : "Inactivo";
        }

        // =====================================
        // MOSTRAR ERROR
        // =====================================
        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;

            iconoError.Visibility =
                Visibility.Visible;
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
        // OBTENER FECHA
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
                    texto += "/" + numeros.Substring(6);
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
        private void Calendario_Click(
            object sender,
            RoutedEventArgs e)
        {
            // PACIENTE INACTIVO:
            // NO PERMITIR CAMBIAR LA FECHA
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
        private void Cerrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (guardando)
                return;

            DescartarPacienteWindow confirmar =
                new DescartarPacienteWindow
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
            // PACIENTE INACTIVO
            // SOLO PERMITIR REACTIVAR
            // =================================
            if (!estadoOriginal)
            {
                if (!nuevoEstado)
                {
                    MostrarError(
                        "El paciente está inactivo. " +
                        "Actívalo para poder modificar sus datos.");

                    return;
                }

                guardando = true;
                btnGuardar.IsEnabled = false;

                try
                {
                    // REACTIVAR EN SQL SERVER
                    // SIN MODIFICAR NINGÚN CAMPO
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
            // VALIDAR TODOS LOS CAMPOS
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

            // VALIDAR IDENTIDAD
            if (string.IsNullOrWhiteSpace(identidadTexto))
            {
                MostrarError("Ingresa una identidad.");
                return;
            }

            if (!Regex.IsMatch(
                identidadTexto,
                @"^(\d{13}|\d{4}-\d{4}-\d{5}|\d{4} \d{4} \d{5})$"))
            {
                MostrarError(
                    "Ingresa una identidad válida de 13 dígitos.");

                return;
            }

            if (IdentidadYaRegistrada?.Invoke(identidad) == true)
            {
                MostrarError(
                    "Ya existe un paciente con esta identidad.");

                return;
            }

            // VALIDAR NOMBRES
            if (string.IsNullOrWhiteSpace(nombres))
            {
                MostrarError(
                    "Ingresa los nombres del paciente.");

                return;
            }

            if (!Regex.IsMatch(
                nombres,
                @"^[\p{L}]+(?:[ '\-][\p{L}]+)*$") ||
                nombres.Length > 50)
            {
                MostrarError("Ingresa nombres válidos.");
                return;
            }

            // VALIDAR APELLIDOS
            if (string.IsNullOrWhiteSpace(apellidos))
            {
                MostrarError(
                    "Ingresa los apellidos del paciente.");

                return;
            }

            if (!Regex.IsMatch(
                apellidos,
                @"^[\p{L}]+(?:[ '\-][\p{L}]+)*$") ||
                apellidos.Length > 50)
            {
                MostrarError("Ingresa apellidos válidos.");
                return;
            }

            // VALIDAR FECHA
            if (string.IsNullOrWhiteSpace(
                txtFechaNacimiento.Text))
            {
                MostrarError(
                    "Ingresa una fecha de nacimiento.");

                return;
            }

            if (!TryObtenerFecha(
                out DateTime fechaNacimiento))
            {
                MostrarError(
                    "Ingresa una fecha válida (YYYY/MM/DD).");

                return;
            }

            if (fechaNacimiento.Year < 1900 ||
                fechaNacimiento.Date > DateTime.Today)
            {
                MostrarError(
                    "Ingresa una fecha de nacimiento válida.");

                return;
            }

            // VALIDAR SEXO
            if (cmbSexo.SelectedItem is not ComboBoxItem opcion ||
                opcion.Tag is not string sexo ||
                (sexo != "F" && sexo != "M"))
            {
                MostrarError(
                    "Selecciona una opción de sexo.");

                return;
            }

            // VALIDAR TELÉFONO
            if (string.IsNullOrWhiteSpace(telefonoTexto))
            {
                MostrarError("Ingresa un teléfono.");
                return;
            }

            if (!Regex.IsMatch(
                telefonoTexto,
                @"^(\d{8}|\d{4}-\d{4})$"))
            {
                MostrarError(
                    "Ingresa un teléfono válido de 8 dígitos.");

                return;
            }

            // =================================
            // FORMATEAR DATOS
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
                // MODIFICAR INFORMACIÓN
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

                // DESACTIVAR SOLO DESPUÉS
                // DE GUARDAR CORRECTAMENTE
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
