
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
    public partial class EditarVisitaWindow : Window
    {
        private readonly int idVisita;
        private readonly int idPaciente;
        private int idEmpleado;

        private bool pacienteActivo = false;
        private bool estadoVerificado = false;
        private bool guardando = false;

        private readonly ObservableCollection<TratamientoOpcion>
            tratamientos = new();

        private ICollectionView? vistaTratamientos;
        private TextBlock? mensajeArchivo;

        // CONSTRUCTOR
        public EditarVisitaWindow(int idVisita, int idPaciente)
        {
            InitializeComponent();

            this.idVisita = idVisita;
            this.idPaciente = idPaciente;

            calFecha.DisplayDateEnd = DateTime.Today;

            dgTratamientos.ItemsSource = tratamientos;

            vistaTratamientos =
                CollectionViewSource.GetDefaultView(tratamientos);

            MostrarPestana("Antecedentes");

            AplicarRestriccionesPaciente();
        }

        // MODELOS
        public class VisitaDetalle
        {
            public int IdVisita { get; set; }
            public int IdExpediente { get; set; }
            public int IdEmpleado { get; set; }
            public DateTime Fecha { get; set; }
            public string Motivo { get; set; } = "";
            public string? Notas { get; set; }
        }

        public class PacienteEstadoApi
        {
            public int IdPaciente { get; set; }
            public bool Activo { get; set; }
        }

        public class TratamientoApi
        {
            public int IdTratamiento { get; set; }
            public string Nombre { get; set; } = "";
            public bool Activo { get; set; }
        }

        public class TratamientoRegistrado
        {
            public int IdTratamiento { get; set; }
            public int Cantidad { get; set; }
        }

        public class ArchivoExpedienteApi
        {
            public Guid IdArchivo { get; set; }
            public string NombreArchivo { get; set; } = "";
            public DateTime FechaSubida { get; set; }
        }

        public class TratamientoOpcion : INotifyPropertyChanged
        {
            public int IdTratamiento { get; set; }
            public string Nombre { get; set; } = "";

            private bool seleccionado;
            private int cantidad = 1;

            public bool Seleccionado
            {
                get => seleccionado;
                set
                {
                    seleccionado = value;
                    Notificar();
                }
            }

            public int Cantidad
            {
                get => cantidad;
                set
                {
                    cantidad = Math.Max(1, value);
                    Notificar();
                }
            }

            public event PropertyChangedEventHandler?
                PropertyChanged;

            private void Notificar(
                [CallerMemberName] string? propiedad = null)
            {
                PropertyChanged?.Invoke(
                    this,
                    new PropertyChangedEventArgs(propiedad));
            }
        }

        // CARGAR VENTANA
        private async void Ventana_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            btnGuardar.IsEnabled = false;

            try
            {
                txtError.Visibility = Visibility.Collapsed;

                // VERIFICAR SI EL PACIENTE ESTÁ ACTIVO
                await VerificarEstadoPacienteAsync();

                // CARGAR INFORMACIÓN
                await CargarVisitaAsync();
                await CargarTratamientosAsync();

                try
                {
                    await CargarArchivosAsync();
                }
                catch (Exception ex)
                {
                    MostrarMensajeArchivo(
                        "Hubo un error al cargar los archivos.",
                        false);

                    MessageBox.Show(
                        ex.ToString(),
                        "Error al cargar archivos",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                estadoVerificado = false;
                pacienteActivo = false;

                MostrarError(
                    "No se pudo cargar la visita: " + ex.Message);
            }
            finally
            {
                AplicarRestriccionesPaciente();
            }
        }

        // CONSULTAR ESTADO DEL PACIENTE
        private async Task VerificarEstadoPacienteAsync()
        {
            estadoVerificado = false;
            pacienteActivo = false;

            if (idPaciente <= 0)
            {
                MostrarError(
                    "No se pudo identificar al paciente de esta visita.");

                return;
            }

            PacienteEstadoApi[] pacientes =
                await ClienteApi.ObtenerAutenticadoAsync<
                    PacienteEstadoApi[]>("api/pacientes");

            PacienteEstadoApi? paciente =
                pacientes.FirstOrDefault(p =>
                    p.IdPaciente == idPaciente);

            if (paciente == null)
            {
                MostrarError(
                    "No se pudo verificar el estado del paciente.");

                return;
            }
            pacienteActivo = paciente.Activo;
            estadoVerificado = true;
        }

        // BLOQUEAR CAMPOS SI EL PACIENTE ESTÁ INACTIVO
        private void AplicarRestriccionesPaciente()
        {
            bool permitir =
                estadoVerificado && pacienteActivo;

            // DOCTOR SIEMPRE ES SOLO LECTURA
            txtDoctor.IsReadOnly = true;

            // ANTECEDENTES
            txtFecha.IsReadOnly = !permitir;
            txtMotivo.IsReadOnly = !permitir;

            // NOTAS
            txtNotas.IsReadOnly = !permitir;

            // CALENDARIO
            btnCalendario.IsEnabled = permitir;
            PopupFecha.IsOpen = false;

            // GUARDAR
            btnGuardar.IsEnabled = permitir && !guardando;

            // ARCHIVOS
            btnSubirArchivo.Visibility = permitir
                ? Visibility.Visible
                : Visibility.Collapsed;

            // LAS PESTAÑAS SIGUEN DISPONIBLES
            btnAntecedentes.IsEnabled = true;
            btnTratamientos.IsEnabled = true;
            btnNotas.IsEnabled = true;

            // EL BUSCADOR SIGUE DISPONIBLE
            txtBuscar.IsEnabled = true;

            if (estadoVerificado && !pacienteActivo)
            {
                btnGuardar.ToolTip =
                    "Activa al paciente para modificar esta visita.";
            }
            else
            {
                btnGuardar.ToolTip = null;
            }
        }

        // CARGAR DATOS DE LA VISITA
        private async Task CargarVisitaAsync()
        {
            VisitaDetalle visita =
                await ClienteApi.ObtenerAutenticadoAsync<VisitaDetalle>(
                    $"api/visitas/{idVisita}");

            idEmpleado = visita.IdEmpleado;

            PerfilActual perfil =
                await ClienteApi.ObtenerPerfilAsync();

            txtDoctor.Text = perfil.NombreUsuario;
            txtDoctor.IsReadOnly = true;
            txtDoctor.Focusable = false;

            txtFecha.Text =
                visita.Fecha.ToString("yyyy/MM/dd");

            if (visita.Fecha != DateTime.MinValue)
            {
                calFecha.SelectedDate = visita.Fecha;
            }

            txtMotivo.Text = visita.Motivo;
            txtNotas.Text = visita.Notas ?? "";
        }

        // CARGAR TRATAMIENTOS
        private async Task CargarTratamientosAsync()
        {
            TratamientoApi[] catalogo =
                await ClienteApi.ObtenerAutenticadoAsync<
                    TratamientoApi[]>(
                    "api/tratamientos?Activo=true");

            TratamientoRegistrado[] registrados =
                await ClienteApi.ObtenerAutenticadoAsync<
                    TratamientoRegistrado[]>(
                    $"api/visitas/{idVisita}/tratamientos");

            tratamientos.Clear();

            foreach (TratamientoApi item in catalogo)
            {
                TratamientoRegistrado? existente =
                    registrados.FirstOrDefault(t =>
                        t.IdTratamiento == item.IdTratamiento);

                tratamientos.Add(new TratamientoOpcion
                {
                    IdTratamiento = item.IdTratamiento,
                    Nombre = item.Nombre,
                    Seleccionado = existente != null,
                    Cantidad = existente != null
                        ? Math.Max(1, existente.Cantidad)
                        : 1
                });
            }
        }

        // MOSTRAR PESTAÑAS
        private void MostrarPestana(string pestana)
        {
            PanelAntecedentes.Visibility =
                pestana == "Antecedentes"
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            PanelTratamientos.Visibility =
                pestana == "Tratamientos"
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            PanelNotas.Visibility =
                pestana == "Notas"
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            Color activo =
                Color.FromRgb(141, 213, 232);

            Color inactivo =
                Color.FromRgb(209, 215, 216);

            btnAntecedentes.Background =
                new SolidColorBrush(
                    pestana == "Antecedentes"
                        ? activo : inactivo);

            btnTratamientos.Background =
                new SolidColorBrush(
                    pestana == "Tratamientos"
                        ? activo : inactivo);

            btnNotas.Background =
                new SolidColorBrush(
                    pestana == "Notas"
                        ? activo : inactivo);
        }

        private void Antecedentes_Click(
            object sender,
            RoutedEventArgs e)
        {
            MostrarPestana("Antecedentes");
        }

        private void Tratamientos_Click(
            object sender,
            RoutedEventArgs e)
        {
            MostrarPestana("Tratamientos");
        }

        private void Notas_Click(
            object sender,
            RoutedEventArgs e)
        {
            MostrarPestana("Notas");
        }

        // BUSCAR TRATAMIENTOS
        private void Buscar_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (vistaTratamientos == null)
                return;

            string buscar = txtBuscar.Text.Trim();

            vistaTratamientos.Filter = objeto =>
            {
                if (objeto is not TratamientoOpcion tratamiento)
                    return false;

                return string.IsNullOrWhiteSpace(buscar)
                    || tratamiento.Nombre.Contains(
                        buscar,
                        StringComparison.OrdinalIgnoreCase);
            };

            vistaTratamientos.Refresh();
        }

        // SELECCIONAR TRATAMIENTO
        private void SeleccionarTratamiento_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!estadoVerificado || !pacienteActivo)
                return;

            if (sender is Button boton &&
                boton.DataContext is TratamientoOpcion tratamiento)
            {
                tratamiento.Seleccionado =
                    !tratamiento.Seleccionado;
            }
        }

        // AUMENTAR CANTIDAD
        private void Sumar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!estadoVerificado || !pacienteActivo)
                return;

            if (sender is Button boton &&
                boton.DataContext is TratamientoOpcion tratamiento)
            {
                tratamiento.Cantidad++;
            }
        }

        // DISMINUIR CANTIDAD
        private void Restar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!estadoVerificado || !pacienteActivo)
                return;

            if (sender is Button boton &&
                boton.DataContext is TratamientoOpcion tratamiento)
            {
                if (tratamiento.Cantidad > 1)
                    tratamiento.Cantidad--;
            }
        }

        // CALENDARIO
        private void Calendario_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!estadoVerificado || !pacienteActivo)
                return;

            PopupFecha.IsOpen = !PopupFecha.IsOpen;
        }

        private void Calendario_SelectedDatesChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (!estadoVerificado || !pacienteActivo)
                return;

            if (calFecha.SelectedDate.HasValue)
            {
                txtFecha.Text =
                    calFecha.SelectedDate.Value.ToString(
                        "yyyy/MM/dd");

                PopupFecha.IsOpen = false;
            }
        }

        // GUARDAR VISITA
        private async void Guardar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (guardando)
                return;

            txtError.Visibility = Visibility.Collapsed;

            // NO PERMITIR EDITAR PACIENTES INACTIVOS
            if (!estadoVerificado || !pacienteActivo)
            {
                MostrarError(
                    "El paciente está desactivado. " +
                    "Actívalo para poder agregar nuevas visitas " +
                    "o modificar los datos de las visitas anteriores.");

                return;
            }

            // VOLVER A CONSULTAR ESTADO ANTES DE GUARDAR
            try
            {
                await VerificarEstadoPacienteAsync();
                AplicarRestriccionesPaciente();
            }
            catch (Exception ex)
            {
                estadoVerificado = false;
                pacienteActivo = false;

                AplicarRestriccionesPaciente();

                MostrarError(
                    "No se pudo verificar el estado del paciente: " +
                    ex.Message);

                return;
            }

            if (!estadoVerificado || !pacienteActivo)
            {
                MostrarError(
                    "El paciente está desactivado. " +
                    "Actívalo para modificar los datos de esta visita.");

                return;
            }

            var resultadoValidacion = new ValidadorVisitaEdicion().Validar(
                new EntradaVisitaEdicion(txtFecha.Text, txtMotivo.Text));
            if (!resultadoValidacion.EsValido)
            {
                MostrarError(resultadoValidacion.Errores[0].Mensaje);
                MostrarPestana("Antecedentes");
                return;
            }

            DateTime fecha = DateTime.ParseExact(txtFecha.Text.Trim(),
                "yyyy/MM/dd", CultureInfo.InvariantCulture);

            guardando = true;
            btnGuardar.IsEnabled = false;

            try
            {
                var datos = new
                {
                    IdEmpleado = idEmpleado,
                    Fecha = fecha,
                    Motivo = txtMotivo.Text.Trim(),
                    Notas = txtNotas.Text.Trim(),

                    Tratamientos = tratamientos
                        .Where(t => t.Seleccionado)
                        .Select(t => new
                        {
                            t.IdTratamiento,
                            t.Cantidad
                        })
                        .ToArray()
                };

                await ClienteApi.ModificarVisitaCompletaAsync(
                    idVisita,
                    datos);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MostrarError(
                    "No se pudo guardar la visita: " +
                    ex.Message);
            }
            finally
            {
                guardando = false;
                AplicarRestriccionesPaciente();
            }
        }

        // SUBIR ARCHIVO
        private async void SubirArchivo_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!estadoVerificado || !pacienteActivo)
                return;

            OpenFileDialog explorador = new OpenFileDialog
            {
                Title = "Seleccionar archivo del expediente",
                Filter =
                    "Archivos permitidos|*.jpg;*.jpeg;*.png;*.pdf",
                Multiselect = false
            };

            if (explorador.ShowDialog() != true)
                return;

            try
            {
                FileInfo archivo =
                    new FileInfo(explorador.FileName);

                if (!archivo.Exists || archivo.Length == 0)
                {
                    MostrarMensajeArchivo(
                        "El archivo está vacío o no existe.",
                        false);
                    return;
                }

                if (archivo.Length > 10 * 1024 * 1024)
                {
                    MostrarMensajeArchivo(
                        "El archivo no puede superar los 10 MB.",
                        false);
                    return;
                }

                // VERIFICAR ESTADO ANTES DE SUBIR
                await VerificarEstadoPacienteAsync();
                AplicarRestriccionesPaciente();

                if (!estadoVerificado || !pacienteActivo)
                {
                    MostrarMensajeArchivo(
                        "El paciente está desactivado.",
                        false);
                    return;
                }

                await ClienteApi.SubirArchivoVisitaAsync(
                    idVisita,
                    explorador.FileName);

                await CargarArchivosAsync();

                MostrarMensajeArchivo(
                    "El archivo se subió exitosamente.",
                    true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.ToString(),
                    "Error al subir archivo",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // LISTAR ARCHIVOS
        private async Task CargarArchivosAsync()
        {
            ArchivoExpedienteApi[] archivos =
                await ClienteApi.ObtenerArchivosVisitaAsync<
                    ArchivoExpedienteApi[]>(idVisita);

            PanelArchivos.Children.Clear();
            mensajeArchivo = null;

            if (archivos.Length == 0)
            {
                PanelArchivos.Children.Add(
                    new TextBlock
                    {
                        Text = "No hay archivos adjuntos.",
                        Foreground = new SolidColorBrush(
                            Color.FromRgb(23, 71, 90)),
                        FontSize = 13
                    });

                return;
            }

            foreach (ArchivoExpedienteApi archivo in archivos)
            {
                StackPanel fila = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(5, 5, 0, 5)
                };

                Image icono = new Image
                {
                    Width = 22,
                    Height = 22,
                    Margin = new Thickness(0, 0, 10, 0),
                    Source = new BitmapImage(
                        new Uri(
                            "pack://application:,,,/Resources/" +
                            "ImagenesPacientes/ImagenesExpediente/" +
                            "subir-archivo.png",
                            UriKind.Absolute))
                };

                Button botonAbrir = new Button
                {
                    Content = archivo.NombreArchivo,
                    Tag = archivo,
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Foreground = new SolidColorBrush(
                        Color.FromRgb(23, 71, 90)),
                    Cursor = Cursors.Hand,
                    ToolTip = "Abrir archivo"
                };

                Button botonBorrar = new Button
                {
                    Width = 17,
                    Height = 17,
                    Margin = new Thickness(8, 0, 0, 0),
                    Background = Brushes.Transparent,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    Tag = archivo.IdArchivo,
                    ToolTip = "Descartar archivo",
                    Visibility =
                        estadoVerificado && pacienteActivo
                            ? Visibility.Visible
                            : Visibility.Collapsed
                };

                botonBorrar.Content = new Image
                {
                    Source = new BitmapImage(
                        new Uri(
                            "pack://application:,,,/Resources/" +
                            "ImagenesAvisos/borrar-archivo.png",
                            UriKind.Absolute)),
                    Stretch = Stretch.Uniform
                };

                botonBorrar.Click += BorrarArchivo_Click;
                botonAbrir.Click += AbrirArchivo_Click;

                fila.Children.Add(icono);
                fila.Children.Add(botonAbrir);
                fila.Children.Add(botonBorrar);

                PanelArchivos.Children.Add(fila);
            }
        }

        // MENSAJES DE ARCHIVOS
        private void MostrarMensajeArchivo(
            string mensaje,
            bool exitoso)
        {
            if (mensajeArchivo != null)
            {
                PanelArchivos.Children.Remove(mensajeArchivo);
            }

            mensajeArchivo = new TextBlock
            {
                Text = mensaje,
                Foreground = exitoso
                    ? new SolidColorBrush(
                        Color.FromRgb(29, 175, 160))
                    : new SolidColorBrush(
                        Color.FromRgb(227, 68, 98)),
                FontSize = 12,
                Margin = new Thickness(0, 12, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };

            PanelArchivos.Children.Add(mensajeArchivo);
        }

        // DESCARTAR ARCHIVO
        private async void BorrarArchivo_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!estadoVerificado || !pacienteActivo)
                return;

            if (sender is not Button boton ||
                boton.Tag is not Guid idArchivo)
                return;

            ConfirmarDescarteWindow advertencia =
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
                    MostrarMensajeArchivo(
                        "El paciente está desactivado.",
                        false);
                    return;
                }

                await ClienteApi.DesactivarArchivoAsync(
                    idArchivo);

                await CargarArchivosAsync();

                MostrarMensajeArchivo(
                    "El archivo se eliminó exitosamente.",
                    true);
            }
            catch (Exception)
            {
                MostrarMensajeArchivo(
                    "Hubo un error al eliminar el archivo.",
                    false);
            }
            finally
            {
                boton.IsEnabled = true;
            }
        }

        // ABRIR O DESCARGAR ARCHIVO
        private async void AbrirArchivo_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button boton ||
                boton.Tag is not ArchivoExpedienteApi archivo)
            {
                return;
            }

            SaveFileDialog dialogo = new()
            {
                FileName = archivo.NombreArchivo,
                Filter = "Todos los archivos|*.*"
            };

            if (dialogo.ShowDialog() != true)
                return;

            try
            {
                byte[] datos =
                    await ClienteApi.DescargarArchivoAsync(
                        archivo.IdArchivo);

                await File.WriteAllBytesAsync(
                    dialogo.FileName,
                    datos);

                MostrarMensajeArchivo(
                    "Archivo descargado correctamente.",
                    true);
            }
            catch (Exception ex)
            {
                MostrarMensajeArchivo(
                    "No se pudo descargar el archivo: " +
                    ex.Message,
                    false);
            }
        }

        // MOSTRAR ERROR
        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;
            txtError.Visibility = Visibility.Visible;
        }

        // MOVER VENTANA
        private void Encabezado_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }

        // CERRAR VENTANA
        private void Cerrar_Click(
            object sender,
            RoutedEventArgs e)
        {
            var confirmacion =
                new ConfirmarDescarteWindow
                {
                    Owner = this
                };

            bool? resultado =
                confirmacion.ShowDialog();

            if (resultado == true)
            {
                DialogResult = false;
                Close();
            }
        }
    }
}
