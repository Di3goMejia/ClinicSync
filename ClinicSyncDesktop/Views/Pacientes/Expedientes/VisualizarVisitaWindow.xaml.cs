using ClinicSyncDesktop.Data;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static ClinicSyncDesktop.Views.EditarVisitaWindow;

namespace ClinicSyncDesktop.Views
{
    public partial class VisualizarVisitaWindow : Window
    {
        private readonly int idVisita;
        private readonly int idPaciente;
        private readonly bool pacienteActivo;
        private TratamientoFila[] tratamientos = Array.Empty<TratamientoFila>();

        public bool VisitaActualizada { get; private set; }

        public sealed record TratamientoFila(int Cantidad, string Nombre);

        private sealed class ProfesionalListado
        {
            public int IdEmpleado { get; set; }
            public string Nombres { get; set; } = "";
            public string Apellidos { get; set; } = "";
        }

        public VisualizarVisitaWindow(int idVisita, int idPaciente, bool pacienteActivo)
        {
            InitializeComponent();

            this.idVisita = idVisita;
            this.idPaciente = idPaciente;
            this.pacienteActivo = pacienteActivo;

            MostrarPestana(PanelAntecedentes, btnAntecedentes);

            if (!pacienteActivo)
            {
                btnEditar.ToolTip = "Activa al paciente para editar esta visita.";
                ToolTipService.SetShowOnDisabled(btnEditar, true);
            }
        }

        private async void Ventana_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarAsync();
        }

        private async Task CargarAsync()
        {
            btnEditar.IsEnabled = false;
            txtEstado.Text = "Cargando visita…";

            VisitaDetalle visita;

            try
            {
                visita = await ClienteApi.ObtenerAutenticadoAsync<VisitaDetalle>(
                    $"api/visitas/{idVisita}");

                txtDoctor.Text = visita.IdEmpleado > 0
                    ? $"Empleado #{visita.IdEmpleado}"
                    : "Sin profesional registrado";

                txtFecha.Text = visita.Fecha.ToString("yyyy/MM/dd");
                txtMotivo.Text = visita.Motivo;
                txtNotas.Text = visita.Notas ?? "";
                txtEstado.Text = "";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                txtEstado.Text = "No se pudo cargar la visita.";
                return;
            }

            await CargarProfesionalAsync(visita.IdEmpleado);
            await CargarTratamientosAsync();
            await CargarArchivosAsync();

            btnEditar.IsEnabled = pacienteActivo;
        }

        private async Task CargarProfesionalAsync(int idEmpleado)
        {
            if (idEmpleado <= 0)
                return;

            try
            {
                var perfil = await ClienteApi.ObtenerPerfilAsync();

                if (perfil.IdEmpleado == idEmpleado &&
                    !string.IsNullOrWhiteSpace(perfil.NombreEmpleado))
                {
                    txtDoctor.Text = perfil.NombreEmpleado;
                    return;
                }

                var usuarios =
                    await ClienteApi.ObtenerAutenticadoAsync<ProfesionalListado[]>(
                        "api/usuarios");

                var profesional = usuarios.FirstOrDefault(
                    usuario => usuario.IdEmpleado == idEmpleado);

                if (profesional != null)
                {
                    string nombre = $"{profesional.Nombres} {profesional.Apellidos}".Trim();

                    if (nombre.Length > 0)
                        txtDoctor.Text = nombre;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private async Task CargarTratamientosAsync()
        {
            tratamientos = Array.Empty<TratamientoFila>();
            FiltrarTratamientos();

            try
            {
                var registrados =
                    await ClienteApi.ObtenerAutenticadoAsync<TratamientoRegistrado[]>(
                        $"api/visitas/{idVisita}/tratamientos");

                tratamientos = registrados.Select(tratamiento =>
                {
                    string nombre = string.IsNullOrWhiteSpace(tratamiento.NombreTratamiento)
                        ? $"Tratamiento #{tratamiento.IdTratamiento}"
                        : tratamiento.NombreTratamiento;

                    return new TratamientoFila(tratamiento.Cantidad, nombre);
                }).ToArray();

                FiltrarTratamientos();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                txtEstado.Text = "No se pudieron cargar los tratamientos registrados.";
            }
        }

        private async Task CargarArchivosAsync()
        {
            PanelArchivos.Children.Clear();

            try
            {
                var archivos =
                    await ClienteApi.ObtenerArchivosVisitaAsync<ArchivoExpedienteApi[]>(idVisita);

                if (archivos.Length == 0)
                {
                    PanelArchivos.Children.Add(new TextBlock
                    {
                        Text = "No hay archivos adjuntos."
                    });
                }

                foreach (var archivo in archivos)
                {
                    var boton = new Button
                    {
                        Content = archivo.NombreArchivo,
                        Tag = archivo,
                        Style = (Style)FindResource("BotonSinBorde"),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(0, 0, 0, 5),
                        ToolTip = "Descargar archivo"
                    };

                    boton.Click += DescargarArchivo_Click;
                    PanelArchivos.Children.Add(boton);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                PanelArchivos.Children.Add(new TextBlock
                {
                    Text = "No se pudieron cargar los archivos."
                });
            }
        }

        private async void DescargarArchivo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button { Tag: ArchivoExpedienteApi archivo })
                return;

            var dialogo = new SaveFileDialog
            {
                FileName = Path.GetFileName(archivo.NombreArchivo)
            };

            if (dialogo.ShowDialog(this) != true)
                return;

            try
            {
                byte[] datos = await ClienteApi.DescargarArchivoAsync(archivo.IdArchivo);
                await File.WriteAllBytesAsync(dialogo.FileName, datos);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                MessageBox.Show(this, "No se pudo descargar el archivo.", Title,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void Editar_Click(object sender, RoutedEventArgs e)
        {
            if (!pacienteActivo || !btnEditar.IsEnabled)
                return;

            var ventana = new EditarVisitaWindow(idVisita, idPaciente)
            {
                Owner = this
            };

            if (ventana.ShowDialog() == true)
                VisitaActualizada = true;

            await CargarAsync();
        }

        private void Buscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (dgTratamientos != null)
                FiltrarTratamientos();
        }

        private void FiltrarTratamientos()
        {
            string filtro = txtBuscar.Text.Trim();
            dgTratamientos.ItemsSource = tratamientos
                .Where(t => t.Nombre.Contains(filtro, StringComparison.CurrentCultureIgnoreCase))
                .ToArray();
        }

        private void MostrarPestana(Grid panelActivo, Button botonActivo)
        {
            foreach (var panel in new[] { PanelAntecedentes, PanelTratamientos, PanelNotas })
            {
                panel.Visibility = panel == panelActivo
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }

            foreach (var boton in new[] { btnAntecedentes, btnTratamientos, btnNotas })
            {
                bool activo = boton == botonActivo;
                boton.Background = new SolidColorBrush(activo
                    ? Color.FromRgb(33, 152, 168)
                    : Color.FromRgb(217, 221, 216));
                boton.Foreground = activo
                    ? Brushes.White
                    : new SolidColorBrush(Color.FromRgb(23, 71, 90));
            }
        }

        private void Antecedentes_Click(object sender, RoutedEventArgs e)
        {
            MostrarPestana(PanelAntecedentes, btnAntecedentes);
        }

        private void Tratamientos_Click(object sender, RoutedEventArgs e)
        {
            MostrarPestana(PanelTratamientos, btnTratamientos);
        }

        private void Notas_Click(object sender, RoutedEventArgs e)
        {
            MostrarPestana(PanelNotas, btnNotas);
        }

        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Encabezado_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}