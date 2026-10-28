
using ClinicSyncDesktop.Data;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ClinicSyncDesktop.Views
{
    public partial class AgregarVisitaWindow : Window
    {
        // PACIENTE Y EMPLEADO
        private readonly int idPaciente;
        private int idEmpleadoActual = 0;

        // PASO ACTUAL
        private int pasoActual = 0;

        // TRATAMIENTOS
        private readonly ObservableCollection<TratamientoOpcion>
            tratamientos = new();

        private ICollectionView vistaTratamientos = null!;

        private readonly ObservableCollection<string>
            archivosPendientes = new();

        // CONSTRUCTOR
        public AgregarVisitaWindow(int idPaciente)
        {
            InitializeComponent();

            this.idPaciente = idPaciente;

            // FECHA ACTUAL
            txtFechaVisita.Text =
                DateTime.Today.ToString("yyyy/MM/dd");

            // CONFIGURAR TABLA
            vistaTratamientos =
                CollectionViewSource.GetDefaultView(tratamientos);

            vistaTratamientos.Filter = FiltrarTratamiento;

            dgTratamientos.ItemsSource = vistaTratamientos;

            // BLOQUEAR DOCTOR
            txtDoctor.IsReadOnly = true;
            txtDoctor.Focusable = false;

            // BLOQUEAR FECHAS FUTURAS
            calFechaVisita.DisplayDateEnd = DateTime.Today;

            MostrarPaso(0);

            Loaded += Ventana_Loaded;
        }

        // MODELO DE TRATAMIENTOS
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

                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(
                            nameof(Seleccionado)));
                }
            }

            public int Cantidad
            {
                get => cantidad;

                set
                {
                    cantidad = value;

                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(
                            nameof(Cantidad)));
                }
            }

            public event PropertyChangedEventHandler?
                PropertyChanged;
        }

        // MODELO DE RESPUESTA DE TRATAMIENTOS
        public class TratamientoApi
        {
            public int IdTratamiento { get; set; }

            public string Nombre { get; set; } = "";

            public bool Activo { get; set; }
        }

        // MOVER VENTANA
        private void Encabezado_MouseLeftButtonDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
            }
        }

        // CARGAR INFORMACIÓN INICIAL
        private async void Ventana_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            // OBTENER EMPLEADO DE LA SESIÓN
            try
            { 
                PerfilActual perfil = await ClienteApi.ObtenerPerfilAsync();

                // GUARDAR ID DEL EMPLEADO
                idEmpleadoActual = perfil.IdEmpleado;

                // MOSTRAR USUARIO QUE INICIÓ SESIÓN
                txtDoctor.Text = perfil.NombreUsuario;  

                // BLOQUEAR CAMPO
                txtDoctor.IsReadOnly = true;
                txtDoctor.Focusable = false;

                // VALIDAR ASOCIACIÓN
                if (idEmpleadoActual <= 0)
                {
                    MostrarError(
                        "Tu cuenta no tiene un empleado asociado.");
                }
                else if (string.IsNullOrWhiteSpace(txtDoctor.Text))
                {
                    MostrarError(
                        "No se pudo obtener el nombre del empleado.");
                }
            }
            catch (Exception ex)
            {
                idEmpleadoActual = 0;

                txtDoctor.Text = "";

                MostrarError(
                    "No se pudo obtener el empleado: " +
                    ex.Message);
            }

            // CARGAR TRATAMIENTOS
            txtEstadoTratamientos.Text =
                "Cargando tratamientos...";

            txtEstadoTratamientos.Visibility =
                Visibility.Visible;

            try
            {
                await CargarTratamientosAsync();

                if (tratamientos.Count == 0)
                {
                    txtEstadoTratamientos.Text =
                        "No hay tratamientos activos disponibles.";
                }
                else
                {
                    txtEstadoTratamientos.Visibility =
                        Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                txtEstadoTratamientos.Text =
                    "No se pudieron cargar los tratamientos.\n" +
                    ex.Message;

                txtEstadoTratamientos.Visibility =
                    Visibility.Visible;
            }
        }

        // OBTENER TRATAMIENTOS DESDE LA API
        private async Task CargarTratamientosAsync()
        {
            TratamientoApi[] lista =
                await ClienteApi
                    .ObtenerAutenticadoAsync<TratamientoApi[]>(
                        "api/tratamientos");

            tratamientos.Clear();

            foreach (TratamientoApi t in lista.Where(t => t.Activo))
            {
                tratamientos.Add(new TratamientoOpcion
                {
                    IdTratamiento = t.IdTratamiento,
                    Nombre = t.Nombre,
                    Cantidad = 1,
                    Seleccionado = false
                });
            }

            vistaTratamientos.Refresh();
        }

        // CAMBIAR ENTRE LOS TRES PASOS
        private void MostrarPaso(int paso)
        {
            pasoActual = paso;

            PanelAntecedentes.Visibility =
                paso == 0
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            PanelTratamientos.Visibility =
                paso == 1
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            PanelNotas.Visibility =
                paso == 2
                    ? Visibility.Visible
                    : Visibility.Collapsed;

            Brush activo =
                new SolidColorBrush(
                    Color.FromRgb(19, 141, 170));

            Brush inactivo =
                new SolidColorBrush(
                    Color.FromRgb(217, 221, 216));

            BarraPaso1.Background =
                paso == 0 ? activo : inactivo;

            BarraPaso2.Background =
                paso == 1 ? activo : inactivo;

            BarraPaso3.Background =
                paso == 2 ? activo : inactivo;

            btnRegresar.Visibility =
                paso == 0
                    ? Visibility.Collapsed
                    : Visibility.Visible;

            btnSiguiente.Visibility =
                paso == 2
                    ? Visibility.Collapsed
                    : Visibility.Visible;

            btnAgregar.Visibility =
                paso == 2
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        // MOSTRAR MENSAJES DE ERROR
        private void MostrarError(string mensaje)
        {
            txtError.Text = mensaje;

            txtError.Visibility =
                Visibility.Visible;
        }

        // OCULTAR ERROR AL MODIFICAR CAMPOS
        private void Campo_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (txtError != null)
            {
                txtError.Text = "";

                txtError.Visibility =
                    Visibility.Collapsed;
            }
        }

        // VALIDAR ANTECEDENTES
        private bool ValidarAntecedentes()
        {
            // VALIDAR EMPLEADO
            if (idEmpleadoActual <= 0)
            {
                MostrarError(
                    "No se pudo identificar al empleado de la sesión.");

                return false;
            }

            // VALIDAR NOMBRE
            if (string.IsNullOrWhiteSpace(txtDoctor.Text))
            {
                MostrarError(
                    "No se pudo obtener el nombre del empleado.");

                return false;
            }

            // VALIDAR FECHA
            if (!DateTime.TryParseExact(
                txtFechaVisita.Text.Trim(),
                "yyyy/MM/dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime fecha))
            {
                MostrarError(
                    "Ingresa una fecha válida (YYYY/MM/DD).");

                return false;
            }

            // NO PERMITIR FECHAS FUTURAS
            if (fecha.Date > DateTime.Today)
            {
                MostrarError(
                    "La fecha de la visita no puede ser posterior a hoy.");

                return false;
            }

            // VALIDAR MOTIVO
            if (string.IsNullOrWhiteSpace(txtMotivo.Text))
            {
                MostrarError(
                    "Ingresa el motivo de la visita.");

                return false;
            }

            // LIMPIAR ERROR
            txtError.Text = "";

            txtError.Visibility =
                Visibility.Collapsed;

            return true;
        }

        // BOTÓN SIGUIENTE
        private void Siguiente_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (pasoActual == 0)
            {
                if (!ValidarAntecedentes())
                {
                    return;
                }
            }

            if (pasoActual < 2)
            {
                MostrarPaso(pasoActual + 1);
            }
        }

        // BOTÓN REGRESAR
        private void Regresar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (pasoActual > 0)
            {
                MostrarPaso(pasoActual - 1);
            }
        }

        // ABRIR CALENDARIO
        private void CalendarioVisita_Click(
            object sender,
            RoutedEventArgs e)
        {
            // BLOQUEAR FECHAS FUTURAS
            calFechaVisita.DisplayDateEnd =
                DateTime.Today;

            if (DateTime.TryParseExact(
                txtFechaVisita.Text.Trim(),
                "yyyy/MM/dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime fecha)
                && fecha.Date <= DateTime.Today)
            {
                calFechaVisita.SelectedDate = fecha;

                calFechaVisita.DisplayDate = fecha;
            }
            else
            {
                calFechaVisita.SelectedDate =
                    DateTime.Today;

                calFechaVisita.DisplayDate =
                    DateTime.Today;
            }

            PopupCalendarioVisita.IsOpen = true;
        }

        // SELECCIONAR FECHA DEL CALENDARIO
        private void CalendarioVisita_SelectedDatesChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (calFechaVisita.SelectedDate.HasValue)
            {
                DateTime fecha =
                    calFechaVisita.SelectedDate.Value;

                if (fecha.Date > DateTime.Today)
                {
                    MostrarError(
                        "La fecha de la visita no puede ser posterior a hoy.");

                    return;
                }

                txtFechaVisita.Text =
                    fecha.ToString("yyyy/MM/dd");

                PopupCalendarioVisita.IsOpen = false;
            }
        }

        // VALIDAR FECHA ESCRITA MANUALMENTE
        private void FechaVisita_LostFocus(
            object sender,
            RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                txtFechaVisita.Text))
            {
                return;
            }

            if (!DateTime.TryParseExact(
                txtFechaVisita.Text.Trim(),
                "yyyy/MM/dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime fecha))
            {
                MostrarError(
                    "Ingresa una fecha válida (YYYY/MM/DD).");

                return;
            }

            if (fecha.Date > DateTime.Today)
            {
                MostrarError(
                    "La fecha de la visita no puede ser posterior a hoy.");

                return;
            }

            txtFechaVisita.Text =
                fecha.ToString("yyyy/MM/dd");
        }

        // FILTRAR TRATAMIENTOS
        private bool FiltrarTratamiento(object objeto)
        {
            if (objeto is not TratamientoOpcion t)
            {
                return false;
            }

            string buscar =
                txtBuscarTratamiento.Text.Trim();

            return string.IsNullOrWhiteSpace(buscar)
                || t.Nombre.Contains(
                    buscar,
                    StringComparison.OrdinalIgnoreCase);
        }

        // BUSCAR TRATAMIENTO
        private void BuscarTratamiento_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            vistaTratamientos?.Refresh();
        }

        // SELECCIONAR TRATAMIENTO
        private void SeleccionarTratamiento_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button boton &&
                boton.DataContext is TratamientoOpcion t)
            {
                t.Seleccionado = !t.Seleccionado;
            }
        }

        // SUMAR CANTIDAD
        private void SumarCantidad_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button boton &&
                boton.DataContext is TratamientoOpcion t)
            {
                if (t.Cantidad < 999)
                {
                    t.Cantidad++;
                }
            }
        }

        // RESTAR CANTIDAD
        private void RestarCantidad_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button boton &&
                boton.DataContext is TratamientoOpcion t)
            {
                if (t.Cantidad > 1)
                {
                    t.Cantidad--;
                }
            }
        }

        // GUARDAR VISITA
        // PENDIENTE DE CONECTAR EL ENVÍO A LA API

        private async void Guardar_Click(
            object sender,
            RoutedEventArgs e)
        {
            // VALIDAR ANTECEDENTES
            if (!ValidarAntecedentes())
            {
                MostrarPaso(0);
                return;
            }

            // EVITAR GUARDAR DOS VECES
            btnAgregar.IsEnabled = false;
            btnAgregar.Content = "Guardando...";

            try
            {
                // CONVERTIR LA FECHA
                DateTime fecha = DateTime.ParseExact(
                    txtFechaVisita.Text.Trim(),
                    "yyyy/MM/dd",
                    CultureInfo.InvariantCulture);

                // OBTENER LOS TRATAMIENTOS SELECCIONADOS
                var tratamientosSeleccionados = tratamientos
                    .Where(t => t.Seleccionado)
                    .Select(t => new
                    {
                        t.IdTratamiento,
                        t.Cantidad
                    })
                    .ToList();

                // PREPARAR LA VISITA
                var nuevaVisita = new
                {
                    IdPaciente = idPaciente,
                    IdEmpleado = idEmpleadoActual,
                    Fecha = fecha,
                    Motivo = txtMotivo.Text.Trim(),
                    Notas = txtNotas.Text.Trim(),
                    Tratamientos = tratamientosSeleccionados
                };

                int idVisita =
                    await ClienteApi.CrearVisitaCompletaAsync(
                        nuevaVisita);

                foreach (string rutaArchivo in archivosPendientes)
                {
                    await ClienteApi.SubirArchivoVisitaAsync(
                        idVisita,
                        rutaArchivo);
                }

                DialogResult = true;
            }
            catch (Exception ex)
            {
                MostrarPaso(0);

                MostrarError(
                    "Ocurrió un error al guardar la visita.\n" +
                    ex.Message);
            }
            finally
            {
                btnAgregar.IsEnabled = true;
                btnAgregar.Content = "Agregar";
            }
        }


        // SUBIR ARCHIVOS PENDIENTES
        private void SubirArchivo_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenFileDialog explorador = new()
            {
                Title = "Seleccionar archivos del expediente",
                Filter = "Archivos permitidos|*.jpg;*.jpeg;*.png;*.pdf",
                Multiselect = true
            };

            if (explorador.ShowDialog() != true)
                return;

            foreach (string ruta in explorador.FileNames)
            {
                FileInfo archivo = new(ruta);

                if (!archivo.Exists || archivo.Length == 0)
                {
                    MostrarErrorArchivo(
                        $"El archivo {archivo.Name} está vacío o no existe.");
                    continue;
                }

                if (archivo.Length > 10 * 1024 * 1024)
                {
                    MostrarErrorArchivo(
                        $"El archivo {archivo.Name} supera los 10 MB.");
                    continue;
                }

                if (archivosPendientes.Contains(
                    ruta,
                    StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                archivosPendientes.Add(ruta);
                AgregarArchivoPendiente(archivo);
            }
        }

        private void AgregarArchivoPendiente(FileInfo archivo)
        {
            StackPanel fila = new()
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5, 4, 0, 4),
                Tag = archivo.FullName
            };

            TextBlock nombre = new()
            {
                Text = archivo.Name,
                Foreground = new SolidColorBrush(Color.FromRgb(23, 71, 90)),
                FontSize = 13,
                Width = 330,
                TextTrimming = TextTrimming.CharacterEllipsis,
                ToolTip = archivo.FullName
            };

            Button quitar = new()
            {
                Content = "×",
                Width = 22,
                Height = 22,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Foreground = new SolidColorBrush(Color.FromRgb(227, 68, 98)),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = fila
            };

            quitar.Click += QuitarArchivoPendiente_Click;
            fila.Children.Add(nombre);
            fila.Children.Add(quitar);
            PanelArchivos.Children.Add(fila);
        }

        private void QuitarArchivoPendiente_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button boton ||
                boton.Tag is not StackPanel fila ||
                fila.Tag is not string ruta)
            {
                return;
            }

            archivosPendientes.Remove(ruta);
            PanelArchivos.Children.Remove(fila);
        }

        private void MostrarErrorArchivo(string mensaje)
        {
            MostrarError(mensaje);
            MostrarPaso(1);
        }

        // CERRAR CON CONFIRMACIÓN DE DESCARTE
        private void Cancelar_Click(
            object sender,
            RoutedEventArgs e)
        {
            // REUTILIZAR VENTANA EXISTENTE
            DescartarPacienteWindow confirmar =
                new DescartarPacienteWindow();

            confirmar.Owner = this;

            bool? respuesta = confirmar.ShowDialog();

            // SÍ: DESCARTAR Y CERRAR
            if (respuesta == true)
            {
                DialogResult = false;
            }

            // NO: MANTENER EL FORMULARIO ABIERTO
        }
    }
}
