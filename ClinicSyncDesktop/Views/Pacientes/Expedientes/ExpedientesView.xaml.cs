
using ClinicSyncDesktop.Data;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClinicSyncDesktop.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    /// <seealso cref="System.Windows.Markup.IStyleConnector" />
    public partial class ExpedientesView : UserControl
    {
        /// <summary>
        /// The identifier paciente
        /// </summary>
        private readonly int idPaciente;
        /// <summary>
        /// The cargando expediente
        /// </summary>
        private bool cargandoExpediente = false;
        /// <summary>
        /// The identifier expediente
        /// </summary>
        private int idExpediente;
        /// <summary>
        /// The editando notas
        /// </summary>
        private bool editandoNotas;
        /// <summary>
        /// The paciente activo
        /// </summary>
        private bool pacienteActivo = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpedientesView"/> class.
        /// </summary>
        /// <param name="idPaciente">The identifier paciente.</param>
        public ExpedientesView(int idPaciente)
        {
            InitializeComponent();

            this.idPaciente = idPaciente;

            Loaded += ExpedientesView_Loaded;
        }

        // MODELO DEL EXPEDIENTE
        /// <summary>
        /// 
        /// </summary>
        public class ExpedienteApi
        {
            /// <summary>
            /// Gets or sets the identifier expediente.
            /// </summary>
            /// <value>
            /// The identifier expediente.
            /// </value>
            public int IdExpediente { get; set; }
            /// <summary>
            /// Gets or sets the identifier paciente.
            /// </summary>
            /// <value>
            /// The identifier paciente.
            /// </value>
            public int IdPaciente { get; set; }
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

        // MODELO DE LAS VISITAS
        /// <summary>
        /// 
        /// </summary>
        public class VisitaApi
        {
            /// <summary>
            /// Gets or sets the identifier visita.
            /// </summary>
            /// <value>
            /// The identifier visita.
            /// </value>
            public int IdVisita { get; set; }
            /// <summary>
            /// Gets or sets the identifier paciente.
            /// </summary>
            /// <value>
            /// The identifier paciente.
            /// </value>
            public int IdPaciente { get; set; }
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

        // MOSTRAR AVISOS DE ÉXITO Y ERROR
        /// <summary>
        /// Mostrars the aviso.
        /// </summary>
        /// <param name="mensaje">The mensaje.</param>
        /// <param name="exitoso">if set to <c>true</c> [exitoso].</param>
        private void MostrarAviso(
            string mensaje,
            bool exitoso)
        {
            PanelAvisos.Children.Clear();

            AvisoNotificacion aviso = new(mensaje, exitoso);

            PanelAvisos.Visibility = Visibility.Visible;

            PanelAvisos.Children.Add(aviso);
        }

        // CARGAR EXPEDIENTE AL ABRIR LA VISTA
        /// <summary>
        /// Handles the Loaded event of the ExpedientesView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ExpedientesView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            if (cargandoExpediente)
                return;

            try
            {
                await CargarExpedienteAsync();
            }
            catch (Exception exc)
            {
                txtEstado.Text =
                    "Error al cargar el expediente.";

                Debug.WriteLine(exc);

                MostrarAviso(
                    "Error al cargar el expediente.",
                    false);
            }
        }

        // CONSULTAR DATOS DEL PACIENTE Y EXPEDIENTE
        /// <summary>
        /// Cargars the expediente asynchronous.
        /// </summary>
        private async Task CargarExpedienteAsync()
        {
            if (cargandoExpediente)
                return;

            cargandoExpediente = true;

            txtEstado.Text = "Cargando expediente...";

            try
            {
                // CONSULTAR PACIENTES
                PacienteApi[] pacientes =
                    await ClienteApi
                        .ObtenerAutenticadoAsync<PacienteApi[]>(
                            "api/pacientes");

                // BUSCAR PACIENTE
                PacienteApi? paciente =
                    pacientes.FirstOrDefault(p =>
                        p.IdPaciente == idPaciente);

                if (paciente == null)
                {
                    txtNombre.Text =
                        "Paciente no encontrado";

                    txtEstado.Text =
                        "No se encontró el paciente seleccionado.";

                    return;
                }

                // VERIFICAR ESTADO DEL PACIENTE
                pacienteActivo = paciente.Activo;

                // OCULTAR AGREGAR VISITA SI ESTÁ INACTIVO
                btnAgregarVisita.Visibility = pacienteActivo
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                // MOSTRAR AVISO ROJO SI ESTÁ INACTIVO
                AvisoPacienteInactivo.Visibility = pacienteActivo
                    ? Visibility.Collapsed
                    : Visibility.Visible;

                // MOSTRAR DATOS DEL PACIENTE
                txtNombre.Text =
                    paciente.Nombres + " " +
                    paciente.Apellidos;

                txtSexo.Text = paciente.Genero;

                txtTelefono.Text = paciente.Telefono;

                // CALCULAR EDAD
                DateTime hoy = DateTime.Today;

                int edad =
                    hoy.Year -
                    paciente.FechaNacimiento.Year;

                if (paciente.FechaNacimiento.Date >
                    hoy.AddYears(-edad))
                {
                    edad--;
                }

                txtEdad.Text = edad.ToString();

                // CONSULTAR EXPEDIENTE
                ExpedienteApi expediente =
                    await ClienteApi
                        .ObtenerAutenticadoAsync<ExpedienteApi>(
                            $"api/pacientes/{idPaciente}/expediente");

                txtNotasExpediente.Text =
                    expediente?.Notas ?? "";

                txtNotasExpediente.IsReadOnly = true;
                btnEditarNotas.Visibility = Visibility.Visible;
                btnGuardarNotas.Visibility = Visibility.Collapsed;
                editandoNotas = false;

                idExpediente = expediente.IdExpediente;

                await CargarArchivosExpedienteAsync();

                // CONSULTAR VISITAS
                VisitaApi[] visitas =
                    await ClienteApi
                        .ObtenerAutenticadoAsync<VisitaApi[]>(
                            $"api/pacientes/{idPaciente}/expediente/visitas");

                // ORDENAR VISITAS
                dgVisitas.ItemsSource =
                    visitas
                        .OrderByDescending(v => v.Fecha)
                        .ToList();

                // MOSTRAR TOTAL
                if (visitas.Length == 0)
                {
                    txtEstado.Text =
                        "Este paciente todavía no tiene visitas registradas.";
                }

                else
                {
                    txtEstado.Text =
                        $"Total de visitas: {visitas.Length}";
                }

            }
            finally
            {
                cargandoExpediente = false;
            }
        }

        /// <summary>
        /// Handles the Click event of the EditarNotas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void EditarNotas_Click(
            object sender,
            RoutedEventArgs e)
        {
            editandoNotas = true;
            txtNotasExpediente.IsReadOnly = false;
            btnEditarNotas.Visibility = Visibility.Collapsed;
            btnGuardarNotas.Visibility = Visibility.Visible;
            txtNotasExpediente.Focus();
            txtNotasExpediente.CaretIndex =
                txtNotasExpediente.Text.Length;
        }

        /// <summary>
        /// Handles the Click event of the GuardarNotas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void GuardarNotas_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!editandoNotas)
                return;

            btnGuardarNotas.IsEnabled = false;

            try
            {
                await ClienteApi.ModificarNotasExpedienteAsync(
                    idPaciente,
                    txtNotasExpediente.Text.Trim());

                editandoNotas = false;
                txtNotasExpediente.IsReadOnly = true;
                btnEditarNotas.Visibility = Visibility.Visible;
                btnGuardarNotas.Visibility = Visibility.Collapsed;

                MostrarAviso("Notas guardadas correctamente.", true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MostrarAviso("No se pudieron guardar las notas.", false);
            }
            finally
            {
                btnGuardarNotas.IsEnabled = true;
            }
        }

        // VOLVER AL MÓDULO PACIENTES
        /// <summary>
        /// Handles the Click event of the Volver control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Volver_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (Window.GetWindow(this)
                is ClinicSync.Views.MenuLateral menu)
            {
                menu.VolverAPacientes();
            }
        }

        // AGREGAR NUEVA VISITA
        /// <summary>
        /// Handles the Click event of the AgregarVisita control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void AgregarVisita_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                AgregarVisitaWindow ventana =
                    new AgregarVisitaWindow(idPaciente);

                ventana.Owner = Window.GetWindow(this);

                bool? resultado = ventana.ShowDialog();

                if (resultado == true)
                {
                    await CargarExpedienteAsync();

                    MostrarAviso(
                        "Visita registrada exitosamente.",
                        true);
                }
            }
            catch (Exception)
            {
                MostrarAviso(
                    "Error al agregar registro.",
                    false);
            }
        }

        // EDITAR VISITA EXISTENTE
        /// <summary>
        /// Handles the Click event of the VisualizarVisita control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void VisualizarVisita_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button boton || boton.DataContext is not VisitaApi visita)
                return;

            try
            {
                var ventana = new VisualizarVisitaWindow(
                    visita.IdVisita,
                    idPaciente,
                    pacienteActivo)
                {
                    Owner = Window.GetWindow(this)
                };

                ventana.ShowDialog();

                if (ventana.VisitaActualizada)
                {
                    await CargarExpedienteAsync();
                    MostrarAviso("Visita actualizada exitosamente.", true);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MostrarAviso("Error al visualizar la visita.", false);
            }
        }

        // SUBIR ARCHIVO
        /// <summary>
        /// Handles the Click event of the SubirArchivo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void SubirArchivo_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenFileDialog explorador = new()
            {
                Title = "Seleccionar archivo del expediente",
                Filter = "Archivos permitidos|*.jpg;*.jpeg;*.png;*.pdf",
                Multiselect = false
            };

            if (explorador.ShowDialog() != true)
                return;

            FileInfo archivo = new(explorador.FileName);

            if (!archivo.Exists || archivo.Length == 0)
            {
                MostrarAviso("El archivo está vacío o no existe.", false);
                return;
            }

            if (archivo.Length > 10 * 1024 * 1024)
            {
                MostrarAviso("El archivo no puede superar los 10 MB.", false);
                return;
            }

            try
            {
                await ClienteApi.SubirArchivoExpedienteAsync(
                    idExpediente,
                    archivo.FullName);

                await CargarArchivosExpedienteAsync();

                MostrarAviso("El archivo se subió exitosamente.", true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MostrarAviso("No se pudo subir el archivo.", false);
            }
        }

        /// <summary>
        /// Cargars the archivos expediente asynchronous.
        /// </summary>
        private async Task CargarArchivosExpedienteAsync()
        {
            ArchivoExpedienteApi[] archivos =
                await ClienteApi.ObtenerArchivosExpedienteAsync<
                    ArchivoExpedienteApi[]>(idExpediente);

            PanelArchivos.Children.Clear();

            if (archivos.Length == 0)
            {
                MostrarEstadoArchivos("No hay archivos adjuntos.");
                return;
            }

            foreach (ArchivoExpedienteApi archivo in archivos)
                AgregarArchivoVisual(archivo);
        }

        /// <summary>
        /// Mostrars the estado archivos.
        /// </summary>
        /// <param name="mensaje">The mensaje.</param>
        private void MostrarEstadoArchivos(string mensaje)
        {
            PanelArchivos.Children.Clear();
            PanelArchivos.Children.Add(new TextBlock
            {
                Text = mensaje,
                Foreground = new SolidColorBrush(Color.FromRgb(23, 71, 90)),
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap
            });
        }

        /// <summary>
        /// Agregars the archivo visual.
        /// </summary>
        /// <param name="archivo">The archivo.</param>
        private void AgregarArchivoVisual(ArchivoExpedienteApi archivo)
        {
            StackPanel fila = new()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(2, 4, 2, 4)
            };

            Image icono = new()
            {
                Width = 18,
                Height = 18,
                Margin = new Thickness(0, 0, 6, 0),
                Source = new BitmapImage(new Uri(
                    "pack://application:,,,/Resources/" +
                    "ImagenesPacientes/ImagenesExpediente/" +
                    "subir-archivo.png",
                    UriKind.Absolute))
            };

            Button abrir = new()
            {
                Content = archivo.NombreArchivo,
                Tag = archivo,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = new SolidColorBrush(Color.FromRgb(23, 71, 90)),
                Cursor = System.Windows.Input.Cursors.Hand,
                ToolTip = "Descargar archivo"
            };
            abrir.Click += DescargarArchivo_Click;

            Button borrar = new()
            {
                Content = "×",
                Tag = archivo.IdArchivo,
                Width = 20,
                Height = 20,
                Margin = new Thickness(5, 0, 0, 0),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = new SolidColorBrush(Color.FromRgb(227, 68, 98)),
                Cursor = System.Windows.Input.Cursors.Hand,
                ToolTip = "Descartar archivo"
            };
            borrar.Click += BorrarArchivo_Click;

            fila.Children.Add(icono);
            fila.Children.Add(abrir);
            fila.Children.Add(borrar);
            PanelArchivos.Children.Add(fila);
        }

        /// <summary>
        /// Handles the Click event of the DescargarArchivo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void DescargarArchivo_Click(
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
                byte[] datos = await ClienteApi.DescargarArchivoAsync(
                    archivo.IdArchivo);

                await File.WriteAllBytesAsync(dialogo.FileName, datos);
                MostrarAviso("Archivo descargado correctamente.", true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MostrarAviso("No se pudo descargar el archivo.", false);
            }
        }

        /// <summary>
        /// Handles the Click event of the BorrarArchivo control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void BorrarArchivo_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button boton ||
                boton.Tag is not Guid idArchivo)
            {
                return;
            }

            ConfirmarDescarteWindow advertencia = new ConfirmarDescarteWindow("¿Seguro que desea descartar este archivo?")
            {
                Owner = Window.GetWindow(this)
            };

            if (advertencia.ShowDialog() != true)
                return;

            boton.IsEnabled = false;

            try
            {
                await ClienteApi.DesactivarArchivoAsync(idArchivo);
                await CargarArchivosExpedienteAsync();
                MostrarAviso("El archivo se eliminó exitosamente.", true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                boton.IsEnabled = true;
                MostrarAviso("No se pudo eliminar el archivo.", false);
            }
        }
    }
}
