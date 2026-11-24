using ClinicSyncDesktop.Data;
using ClinicSyncDesktop.Validations;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClinicSyncDesktop.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="System.Windows.Markup.IStyleConnector" />
    public partial class EditarVisitaWindow : Window
    {
        /// <summary>
        /// The identifier visita
        /// </summary>
        private readonly int idVisita;
        /// <summary>
        /// The identifier paciente
        /// </summary>
        private readonly int idPaciente;
        /// <summary>
        /// The identifier empleado
        /// </summary>
        private int idEmpleado;

        /// <summary>
        /// The paciente activo
        /// </summary>
        private bool pacienteActivo;
        /// <summary>
        /// The estado verificado
        /// </summary>
        private bool estadoVerificado;
        /// <summary>
        /// The guardando
        /// </summary>
        private bool guardando;

        /// <summary>
        /// The tratamientos
        /// </summary>
        private readonly ObservableCollection<TratamientoOpcion> tratamientos = new();
        /// <summary>
        /// The vista tratamientos
        /// </summary>
        private ICollectionView? vistaTratamientos;
        /// <summary>
        /// The mensaje archivo
        /// </summary>
        private TextBlock? mensajeArchivo;

        /// <summary>
        /// Initializes a new instance of the <see cref="EditarVisitaWindow"/> class.
        /// </summary>
        /// <param name="idVisita">The identifier visita.</param>
        /// <param name="idPaciente">The identifier paciente.</param>
        public EditarVisitaWindow(int idVisita, int idPaciente)
        {
            InitializeComponent();

            this.idVisita = idVisita;
            this.idPaciente = idPaciente;

            calFecha.DisplayDateEnd = DateTime.Today;
            dgTratamientos.ItemsSource = tratamientos;
            vistaTratamientos = CollectionViewSource.GetDefaultView(tratamientos);

            MostrarPestana("Antecedentes");
            AplicarRestriccionesPaciente();
        }

        /// <summary>
        /// 
        /// </summary>
        public class VisitaDetalle
        {
            /// <summary>
            /// Gets or sets the identifier visita.
            /// </summary>
            /// <value>
            /// The identifier visita.
            /// </value>
            public int IdVisita { get; set; }
            /// <summary>
            /// Gets or sets the identifier expediente.
            /// </summary>
            /// <value>
            /// The identifier expediente.
            /// </value>
            public int IdExpediente { get; set; }
            /// <summary>
            /// Gets or sets the identifier empleado.
            /// </summary>
            /// <value>
            /// The identifier empleado.
            /// </value>
            public int IdEmpleado { get; set; }
            /// <summary>
            /// Gets or sets the fecha.
            /// </summary>
            /// <value>
            /// The fecha.
            /// </value>
            public DateTime Fecha { get; set; }
            /// <summary>
            /// Gets or sets the motivo.
            /// </summary>
            /// <value>
            /// The motivo.
            /// </value>
            public string Motivo { get; set; } = "";
            /// <summary>
            /// Gets or sets the notas.
            /// </summary>
            /// <value>
            /// The notas.
            /// </value>
            public string? Notas { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class PacienteEstadoApi
        {
            /// <summary>
            /// Gets or sets the identifier paciente.
            /// </summary>
            /// <value>
            /// The identifier paciente.
            /// </value>
            public int IdPaciente { get; set; }
            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="PacienteEstadoApi"/> is activo.
            /// </summary>
            /// <value>
            ///   <c>true</c> if activo; otherwise, <c>false</c>.
            /// </value>
            public bool Activo { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class TratamientoApi
        {
            /// <summary>
            /// Gets or sets the identifier tratamiento.
            /// </summary>
            /// <value>
            /// The identifier tratamiento.
            /// </value>
            public int IdTratamiento { get; set; }
            /// <summary>
            /// Gets or sets the nombre.
            /// </summary>
            /// <value>
            /// The nombre.
            /// </value>
            public string Nombre { get; set; } = "";
            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="TratamientoApi"/> is activo.
            /// </summary>
            /// <value>
            ///   <c>true</c> if activo; otherwise, <c>false</c>.
            /// </value>
            public bool Activo { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class TratamientoRegistrado
        {
            /// <summary>
            /// Gets or sets the identifier tratamiento.
            /// </summary>
            /// <value>
            /// The identifier tratamiento.
            /// </value>
            public int IdTratamiento { get; set; }
            /// <summary>
            /// Gets or sets the cantidad.
            /// </summary>
            /// <value>
            /// The cantidad.
            /// </value>
            public int Cantidad { get; set; }
            /// <summary>
            /// Gets or sets the nombre tratamiento.
            /// </summary>
            /// <value>
            /// The nombre tratamiento.
            /// </value>
            public string NombreTratamiento { get; set; } = "";
            /// <summary>
            /// Gets or sets a value indicating whether [tratamiento activo].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [tratamiento activo]; otherwise, <c>false</c>.
            /// </value>
            public bool TratamientoActivo { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class ArchivoExpedienteApi
        {
            /// <summary>
            /// Gets or sets the identifier archivo.
            /// </summary>
            /// <value>
            /// The identifier archivo.
            /// </value>
            public Guid IdArchivo { get; set; }
            /// <summary>
            /// Gets or sets the nombre archivo.
            /// </summary>
            /// <value>
            /// The nombre archivo.
            /// </value>
            public string NombreArchivo { get; set; } = "";
            /// <summary>
            /// Gets or sets the fecha subida.
            /// </summary>
            /// <value>
            /// The fecha subida.
            /// </value>
            public DateTime FechaSubida { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
        public class TratamientoOpcion : INotifyPropertyChanged
        {
            /// <summary>
            /// Gets or sets the identifier tratamiento.
            /// </summary>
            /// <value>
            /// The identifier tratamiento.
            /// </value>
            public int IdTratamiento { get; set; }
            /// <summary>
            /// Gets or sets the nombre.
            /// </summary>
            /// <value>
            /// The nombre.
            /// </value>
            public string Nombre { get; set; } = "";

            /// <summary>
            /// Gets a value indicating whether [puede editar].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [puede editar]; otherwise, <c>false</c>.
            /// </value>
            public bool PuedeEditar { get; init; } = true;

            /// <summary>
            /// The seleccionado
            /// </summary>
            private bool seleccionado;
            /// <summary>
            /// The cantidad
            /// </summary>
            private int cantidad = 1;

            /// <summary>
            /// Gets or sets a value indicating whether this <see cref="TratamientoOpcion"/> is seleccionado.
            /// </summary>
            /// <value>
            ///   <c>true</c> if seleccionado; otherwise, <c>false</c>.
            /// </value>
            public bool Seleccionado
            {
                get => seleccionado;
                set
                {
                    seleccionado = value;
                    Notificar();
                }
            }

            /// <summary>
            /// Gets or sets the cantidad.
            /// </summary>
            /// <value>
            /// The cantidad.
            /// </value>
            public int Cantidad
            {
                get => cantidad;
                set
                {
                    cantidad = Math.Max(1, value);
                    Notificar();
                }
            }

            /// <summary>
            /// Occurs when a property value changes.
            /// </summary>
            public event PropertyChangedEventHandler? PropertyChanged;

            /// <summary>
            /// Notificars the specified propiedad.
            /// </summary>
            /// <param name="propiedad">The propiedad.</param>
            private void Notificar([CallerMemberName] string? propiedad = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propiedad));
            }
        }

        /// <summary>
        /// Handles the Loaded event of the Ventana control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Ventana_Loaded(object sender, RoutedEventArgs e)
        {
            btnGuardar.IsEnabled = false;

            try
            {
                txtError.Visibility = Visibility.Collapsed;

                await VerificarEstadoPacienteAsync();
                await CargarVisitaAsync();
                await CargarTratamientosAsync();

                try
                {
                    await CargarArchivosAsync();
                }
                catch (Exception ex)
                {
                    MostrarMensajeArchivo("Hubo un error al cargar los archivos.", false);
                    MessageBox.Show(ex.ToString(), "Error al cargar archivos",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                estadoVerificado = false;
                pacienteActivo = false;
                MostrarError("No se pudo cargar la visita: " + ex.Message);
            }
            finally
            {
                AplicarRestriccionesPaciente();
            }
        }

        /// <summary>
        /// Verificars the estado paciente asynchronous.
        /// </summary>
        private async Task VerificarEstadoPacienteAsync()
        {
            estadoVerificado = false;
            pacienteActivo = false;

            if (idPaciente <= 0)
            {
                MostrarError("No se pudo identificar al paciente de esta visita.");
                return;
            }

            var pacientes = await ClienteApi.ObtenerAutenticadoAsync<PacienteEstadoApi[]>(
                "api/pacientes");

            var paciente = pacientes.FirstOrDefault(p => p.IdPaciente == idPaciente);

            if (paciente == null)
            {
                MostrarError("No se pudo verificar el estado del paciente.");
                return;
            }

            pacienteActivo = paciente.Activo;
            estadoVerificado = true;
        }

        /// <summary>
        /// Aplicars the restricciones paciente.
        /// </summary>
        private void AplicarRestriccionesPaciente()
        {
            bool permitir = estadoVerificado && pacienteActivo;

            txtDoctor.IsReadOnly = true;
            txtFecha.IsReadOnly = !permitir;
            txtMotivo.IsReadOnly = !permitir;
            txtNotas.IsReadOnly = !permitir;

            btnCalendario.IsEnabled = permitir;
            PopupFecha.IsOpen = false;
            btnGuardar.IsEnabled = permitir && !guardando;
            btnSubirArchivo.Visibility = permitir ? Visibility.Visible : Visibility.Collapsed;

            btnAntecedentes.IsEnabled = true;
            btnTratamientos.IsEnabled = true;
            btnNotas.IsEnabled = true;
            txtBuscar.IsEnabled = true;

            btnGuardar.ToolTip = estadoVerificado && !pacienteActivo
                ? "Activa al paciente para modificar esta visita."
                : null;
        }

        /// <summary>
        /// Cargars the visita asynchronous.
        /// </summary>
        private async Task CargarVisitaAsync()
        {
            var visita = await ClienteApi.ObtenerAutenticadoAsync<VisitaDetalle>(
                $"api/visitas/{idVisita}");

            idEmpleado = visita.IdEmpleado;

            var perfil = await ClienteApi.ObtenerPerfilAsync();

            bool tieneNombreRegistrado =
                !string.IsNullOrWhiteSpace(perfil.Nombres) ||
                !string.IsNullOrWhiteSpace(perfil.Apellidos);

            txtDoctor.Text = tieneNombreRegistrado
                ? perfil.NombreCorto
                : "Nombre no registrado";

            txtDoctor.IsReadOnly = true;
            txtDoctor.Focusable = false;

            txtFecha.Text = visita.Fecha.ToString(
                "yyyy/MM/dd", CultureInfo.InvariantCulture);

            if (visita.Fecha != DateTime.MinValue)
                calFecha.SelectedDate = visita.Fecha;

            txtMotivo.Text = visita.Motivo;
            txtNotas.Text = visita.Notas ?? "";
        }

        /// <summary>
        /// Cargars the tratamientos asynchronous.
        /// </summary>
        private async Task CargarTratamientosAsync()
        {
            var catalogo = await ClienteApi.ObtenerAutenticadoAsync<TratamientoApi[]>(
                "api/tratamientos?Activo=true");

            var registrados =
                await ClienteApi.ObtenerAutenticadoAsync<TratamientoRegistrado[]>(
                    $"api/visitas/{idVisita}/tratamientos");

            var opciones = catalogo
                .Where(item => item.Activo)
                .ToDictionary(item => item.IdTratamiento, item => new TratamientoOpcion
                {
                    IdTratamiento = item.IdTratamiento,
                    Nombre = item.Nombre,
                    Cantidad = 1
                });

            foreach (var registrado in registrados)
            {
                opciones.TryGetValue(registrado.IdTratamiento, out var opcion);

                string nombre = !string.IsNullOrWhiteSpace(registrado.NombreTratamiento)
                    ? registrado.NombreTratamiento
                    : opcion?.Nombre ?? $"Tratamiento #{registrado.IdTratamiento}";

                opciones[registrado.IdTratamiento] = new TratamientoOpcion
                {
                    IdTratamiento = registrado.IdTratamiento,
                    Nombre = nombre,
                    Cantidad = registrado.Cantidad,
                    Seleccionado = true,
                    PuedeEditar = registrado.TratamientoActivo
                };
            }

            tratamientos.Clear();

            foreach (var opcion in opciones.Values.OrderBy(item => item.Nombre))
                tratamientos.Add(opcion);
        }

        /// <summary>
        /// Mostrars the pestana.
        /// </summary>
        /// <param name="pestana">The pestana.</param>
        private void MostrarPestana(string pestana)
        {
            PanelAntecedentes.Visibility = pestana == "Antecedentes"
                ? Visibility.Visible : Visibility.Collapsed;
            PanelTratamientos.Visibility = pestana == "Tratamientos"
                ? Visibility.Visible : Visibility.Collapsed;
            PanelNotas.Visibility = pestana == "Notas"
                ? Visibility.Visible : Visibility.Collapsed;

            Color activo = Color.FromRgb(141, 213, 232);
            Color inactivo = Color.FromRgb(209, 215, 216);

            btnAntecedentes.Background = new SolidColorBrush(
                pestana == "Antecedentes" ? activo : inactivo);
            btnTratamientos.Background = new SolidColorBrush(
                pestana == "Tratamientos" ? activo : inactivo);
            btnNotas.Background = new SolidColorBrush(
                pestana == "Notas" ? activo : inactivo);
        }

        /// <summary>
        /// Handles the Click event of the Antecedentes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Antecedentes_Click(object sender, RoutedEventArgs e)
        {
            MostrarPestana("Antecedentes");
        }

        /// <summary>
        /// Handles the Click event of the Tratamientos control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Tratamientos_Click(object sender, RoutedEventArgs e)
        {
            MostrarPestana("Tratamientos");
        }

        /// <summary>
        /// Handles the Click event of the Notas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Notas_Click(object sender, RoutedEventArgs e)
        {
            MostrarPestana("Notas");
        }

        /// <summary>
        /// Handles the TextChanged event of the Buscar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void Buscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (vistaTratamientos == null)
                return;

            string buscar = txtBuscar.Text.Trim();

            vistaTratamientos.Filter = objeto =>
                objeto is TratamientoOpcion tratamiento &&
                tratamiento.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase);

            vistaTratamientos.Refresh();
        }

        private void SeleccionarTratamiento_Click(object sender, RoutedEventArgs e)
        {
            if (!estadoVerificado || !pacienteActivo)
                return;

            if (sender is Button { DataContext: TratamientoOpcion tratamiento } &&
                tratamiento.PuedeEditar)
            {
                tratamiento.Seleccionado = !tratamiento.Seleccionado;
            }
        }

        private void Sumar_Click(object sender, RoutedEventArgs e)
        {
            if (!estadoVerificado || !pacienteActivo)
                return;

            if (sender is Button { DataContext: TratamientoOpcion tratamiento } &&
                tratamiento.PuedeEditar)
            {
                tratamiento.Cantidad++;
            }
        }

        private void Restar_Click(object sender, RoutedEventArgs e)
        {
            if (!estadoVerificado || !pacienteActivo)
                return;

            if (sender is Button { DataContext: TratamientoOpcion tratamiento } &&
                tratamiento.PuedeEditar &&
                tratamiento.Cantidad > 1)
            {
                tratamiento.Cantidad--;
            }
        }

        private void Calendario_Click(object sender, RoutedEventArgs e)
        {
            if (!estadoVerificado || !pacienteActivo)
                return;

            PopupFecha.IsOpen = !PopupFecha.IsOpen;
        }

        private void Calendario_SelectedDatesChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!estadoVerificado || !pacienteActivo)
                return;

            if (calFecha.SelectedDate.HasValue)
            {
                txtFecha.Text = calFecha.SelectedDate.Value.ToString("yyyy/MM/dd");
                PopupFecha.IsOpen = false;
            }
        }

        private async void Guardar_Click(object sender, RoutedEventArgs e)
        {
            if (guardando)
                return;

            txtError.Visibility = Visibility.Collapsed;

            if (!estadoVerificado || !pacienteActivo)
            {
                MostrarError("Activa al paciente para modificar los datos de esta visita.");
                return;
            }

            guardando = true;
            btnGuardar.IsEnabled = false;

            try
            {
                try
                {
                    await VerificarEstadoPacienteAsync();
                }
                catch (Exception ex)
                {
                    MostrarError("No se pudo verificar el estado del paciente: " + ex.Message);
                    return;
                }

                if (!estadoVerificado || !pacienteActivo)
                {
                    MostrarError("Activa al paciente para modificar los datos de esta visita.");
                    return;
                }

                var resultado = new ValidadorVisitaEdicion().Validar(
                    new EntradaVisitaEdicion(txtFecha.Text, txtMotivo.Text));

                if (!resultado.EsValido)
                {
                    MostrarError(resultado.Errores[0].Mensaje);
                    MostrarPestana("Antecedentes");
                    return;
                }

                DateTime fecha = DateTime.ParseExact(
                    txtFecha.Text.Trim(), "yyyy/MM/dd", CultureInfo.InvariantCulture);

                var datos = new
                {
                    IdEmpleado = idEmpleado,
                    Fecha = fecha,
                    Motivo = txtMotivo.Text.Trim(),
                    Notas = txtNotas.Text.Trim(),
                    Tratamientos = tratamientos
                        .Where(t => t.Seleccionado)
                        .Select(t => new { t.IdTratamiento, t.Cantidad })
                        .ToArray()
                };

                await ClienteApi.ModificarVisitaCompletaAsync(idVisita, datos);
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MostrarError("No se pudo guardar la visita: " + ex.Message);
            }
            finally
            {
                guardando = false;
                AplicarRestriccionesPaciente();
            }
        }

        private async void SubirArchivo_Click(object sender, RoutedEventArgs e)
        {
            if (!estadoVerificado || !pacienteActivo)
                return;

            var explorador = new OpenFileDialog
            {
                Title = "Seleccionar archivo del expediente",
                Filter = "Archivos permitidos|*.jpg;*.jpeg;*.png;*.pdf",
                Multiselect = false
            };

            if (explorador.ShowDialog(this) != true)
                return;

            try
            {
                var archivo = new FileInfo(explorador.FileName);

                if (!archivo.Exists || archivo.Length == 0)
                {
                    MostrarMensajeArchivo("El archivo está vacío o no existe.", false);
                    return;
                }

                if (archivo.Length > 10 * 1024 * 1024)
                {
                    MostrarMensajeArchivo("El archivo no puede superar los 10 MB.", false);
                    return;
                }

                await VerificarEstadoPacienteAsync();
                AplicarRestriccionesPaciente();

                if (!estadoVerificado || !pacienteActivo)
                {
                    MostrarMensajeArchivo("El paciente está desactivado.", false);
                    return;
                }

                await ClienteApi.SubirArchivoVisitaAsync(idVisita, explorador.FileName);
                await CargarArchivosAsync();
                MostrarMensajeArchivo("El archivo se subió exitosamente.", true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error al subir archivo",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task CargarArchivosAsync()
        {
            var archivos =
                await ClienteApi.ObtenerArchivosVisitaAsync<ArchivoExpedienteApi[]>(idVisita);

            PanelArchivos.Children.Clear();
            mensajeArchivo = null;

            if (archivos.Length == 0)
            {
                PanelArchivos.Children.Add(new TextBlock
                {
                    Text = "No hay archivos adjuntos.",
                    Foreground = new SolidColorBrush(Color.FromRgb(23, 71, 90)),
                    FontSize = 13
                });
                return;
            }

            foreach (var archivo in archivos)
            {
                var fila = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(5, 5, 0, 5)
                };

                var icono = new Image
                {
                    Width = 22,
                    Height = 22,
                    Margin = new Thickness(0, 0, 10, 0),
                    Source = new BitmapImage(new Uri(
                        "pack://application:,,,/Resources/ImagenesPacientes/" +
                        "ImagenesExpediente/subir-archivo.png", UriKind.Absolute))
                };

                var botonAbrir = new Button
                {
                    Content = archivo.NombreArchivo,
                    Tag = archivo,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Foreground = new SolidColorBrush(Color.FromRgb(23, 71, 90)),
                    Cursor = Cursors.Hand,
                    ToolTip = "Abrir archivo"
                };

                var botonBorrar = new Button
                {
                    Width = 17,
                    Height = 17,
                    Margin = new Thickness(8, 0, 0, 0),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Tag = archivo.IdArchivo,
                    ToolTip = "Descartar archivo",
                    Visibility = estadoVerificado && pacienteActivo
                        ? Visibility.Visible : Visibility.Collapsed,
                    Content = new Image
                    {
                        Source = new BitmapImage(new Uri(
                            "pack://application:,,,/Resources/ImagenesAvisos/borrar-archivo.png",
                            UriKind.Absolute)),
                        Stretch = Stretch.Uniform
                    }
                };

                botonAbrir.Click += AbrirArchivo_Click;
                botonBorrar.Click += BorrarArchivo_Click;

                fila.Children.Add(icono);
                fila.Children.Add(botonAbrir);
                fila.Children.Add(botonBorrar);
                PanelArchivos.Children.Add(fila);
            }
        }

        private void MostrarMensajeArchivo(string mensaje, bool exitoso)
        {
            if (mensajeArchivo != null)
                PanelArchivos.Children.Remove(mensajeArchivo);

            mensajeArchivo = new TextBlock
            {
                Text = mensaje,
                Foreground = new SolidColorBrush(exitoso
                    ? Color.FromRgb(29, 175, 160)
                    : Color.FromRgb(227, 68, 98)),
                FontSize = 12,
                Margin = new Thickness(0, 12, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };

            PanelArchivos.Children.Add(mensajeArchivo);
        }

        private async void BorrarArchivo_Click(object sender, RoutedEventArgs e)
        {
            if (!estadoVerificado || !pacienteActivo)
                return;

            if (sender is not Button boton || boton.Tag is not Guid idArchivo)
                return;

            var advertencia =
                new ConfirmarDescarteWindow("¿Seguro que desea descartar este archivo?")
                {
                    Owner = this
                };

            if (advertencia.ShowDialog() != true)
                return;

            boton.IsEnabled = false;

            try
            {
                await VerificarEstadoPacienteAsync();
                AplicarRestriccionesPaciente();

                if (!estadoVerificado || !pacienteActivo)
                {
                    MostrarMensajeArchivo("El paciente está desactivado.", false);
                    return;
                }

                await ClienteApi.DesactivarArchivoAsync(idArchivo);
                await CargarArchivosAsync();
                MostrarMensajeArchivo("El archivo se eliminó exitosamente.", true);
            }
            catch (Exception)
            {
                MostrarMensajeArchivo("Hubo un error al eliminar el archivo.", false);
            }
            finally
            {
                boton.IsEnabled = true;
            }
        }

        private async void AbrirArchivo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: ArchivoExpedienteApi archivo })
                return;

            var dialogo = new SaveFileDialog
            {
                FileName = archivo.NombreArchivo,
                Filter = "Todos los archivos|*.*"
            };

            if (dialogo.ShowDialog(this) != true)
                return;

            try
            {
                byte[] datos = await ClienteApi.DescargarArchivoAsync(archivo.IdArchivo);
                await File.WriteAllBytesAsync(dialogo.FileName, datos);
                MostrarMensajeArchivo("Archivo descargado correctamente.", true);
            }
            catch (Exception ex)
            {
                MostrarMensajeArchivo("No se pudo descargar el archivo: " + ex.Message, false);
            }
        }

        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            txtError.Visibility = Visibility.Visible;
        }

        private void Encabezado_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            var confirmacion = new ConfirmarDescarteWindow { Owner = this };

            if (confirmacion.ShowDialog() == true)
                DialogResult = false;
        }
    }
}