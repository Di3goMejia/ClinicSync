
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
    public partial class ExpedientesView : UserControl
    {
        private readonly int idPaciente;
        private bool cargandoExpediente = false;
        private int idExpediente;
        private bool editandoNotas;

        public ExpedientesView(int idPaciente)
        {
            InitializeComponent();

            this.idPaciente = idPaciente;

            Loaded += ExpedientesView_Loaded;
        }

        // MODELO DEL EXPEDIENTE
        public class ExpedienteApi
        {
            public int IdExpediente { get; set; }
            public int IdPaciente { get; set; }
            public string? Notas { get; set; }
        }

        public class ArchivoExpedienteApi
        {
            public Guid IdArchivo { get; set; }
            public string NombreArchivo { get; set; } = "";
            public DateTime FechaSubida { get; set; }
        }

        // MODELO DE LAS VISITAS
        public class VisitaApi
        {
            public int IdVisita { get; set; }
            public int IdPaciente { get; set; }
            public int IdEmpleado { get; set; }
            public DateTime Fecha { get; set; }
            public string Motivo { get; set; } = "";
            public string? Notas { get; set; }
        }

        // MOSTRAR AVISOS DE ÉXITO Y ERROR
        private void MostrarAviso(
            string mensaje,
            bool exitoso)
        {
            PanelAvisos.Children.Clear();

            AvisoPaciente aviso = new(mensaje, exitoso);
            
            PanelAvisos.Visibility = Visibility.Visible;

            PanelAvisos.Children.Add(aviso);
        }

        // CARGAR EXPEDIENTE AL ABRIR LA VISTA
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
        private async void EditarVisita_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button boton &&
                boton.DataContext is VisitaApi visita)
            {
                try
                {
                    // ABRIR LA VISITA SELECCIONADA
                    EditarVisitaWindow ventana =
                        new EditarVisitaWindow(visita.IdVisita);

                    ventana.Owner = Window.GetWindow(this);

                    bool? resultado = ventana.ShowDialog();

                    // SI SE GUARDARON LOS CAMBIOS
                    if (resultado == true)
                    {
                        // ACTUALIZAR LISTA DE VISITAS
                        await CargarExpedienteAsync();

                        // MOSTRAR AVISO
                        MostrarAviso(
                            "Visita actualizada exitosamente.",
                            true);
                    }
                }
                catch (Exception)
                {
                    MostrarAviso(
                        "Error al editar la visita.",
                        false);
                }
            }
        }

        // SUBIR ARCHIVO
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

        private async void BorrarArchivo_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button boton ||
                boton.Tag is not Guid idArchivo)
            {
                return;
            }

            DescartarArchivoWindow advertencia = new()
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
