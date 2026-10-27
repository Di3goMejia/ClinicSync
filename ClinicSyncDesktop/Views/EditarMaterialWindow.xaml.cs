
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClinicSyncDesktop.Views
{
    public partial class EditarMaterialWindow : Window
    {
        private readonly InventarioApiService inventarioService;
        private readonly Material materialOriginal;

        private string? rutaImagen;
        private bool imagenModificada;

        public EditarMaterialWindow(
            InventarioApiService servicio,
            Material material)
        {
            InitializeComponent();

            inventarioService = servicio;
            materialOriginal = material;

            CargarDatosMaterial();
        }

        // CARGAR LOS DATOS ACTUALES
        private void CargarDatosMaterial()
        {
            txtNombre.Text = materialOriginal.Nombre;
            txtCodigo.Text = materialOriginal.Codigo;

            txtPrecioCompra.Text = materialOriginal.PrecioCompra
                .ToString("0.00", CultureInfo.InvariantCulture);

            txtStockActual.Text = materialOriginal.StockActual
                .ToString("0.##", CultureInfo.CurrentCulture);

            txtStockMinimo.Text = materialOriginal.StockMinimo
                .ToString(CultureInfo.CurrentCulture);

            txtUnidadMedida.Text = materialOriginal.UnidadMedida;
            txtDescripcion.Text = materialOriginal.Descripcion ?? "";

            // CARGAR EL ESTADO ACTUAL
            chkActivo.IsChecked = materialOriginal.Activo;

            rutaImagen = materialOriginal.Imagen;

            if (!string.IsNullOrWhiteSpace(rutaImagen))
            {
                txtImagen.Text = ObtenerNombreImagen(rutaImagen);
                txtImagen.ToolTip = rutaImagen;
                btnQuitarImagen.Visibility = Visibility.Visible;

                CargarVistaPrevia(rutaImagen);
            }

            // BLOQUEAR LOS CAMPOS SI EL MATERIAL ESTÁ DESHABILITADO
            EstablecerEdicionHabilitada(materialOriginal.Activo);

            if (!materialOriginal.Activo)
            {
                MostrarErrorValidacion(
                    "⚠ El material está deshabilitado. " +
                    "Habilítalo para poder modificar sus datos.");
            }
        }

        // HABILITAR O BLOQUEAR LOS CAMPOS EDITABLES
        private void EstablecerEdicionHabilitada(bool habilitada)
        {
            txtNombre.IsEnabled = habilitada;
            txtCodigo.IsEnabled = habilitada;
            txtPrecioCompra.IsEnabled = habilitada;
            txtStockMinimo.IsEnabled = habilitada;
            txtUnidadMedida.IsEnabled = habilitada;
            txtDescripcion.IsEnabled = habilitada;

            // EL STOCK ACTUAL SIEMPRE ES DE SOLO LECTURA
            txtStockActual.IsEnabled = true;

            // PERMITIR CAMBIAR LA IMAGEN SOLO SI ESTÁ HABILITADO
            btnQuitarImagen.IsEnabled = habilitada;

            // EL BOTÓN PARA SELECCIONAR IMAGEN NO TIENE NOMBRE
            // EN EL XAML, POR ESO SE LOCALIZA POR SU EVENTO.
            foreach (var boton in BuscarBotonesImagen())
            {
                boton.IsEnabled = habilitada;
            }
        }

        // LOCALIZAR EL BOTÓN ELEGIR IMAGEN
        private System.Collections.Generic.IEnumerable<System.Windows.Controls.Button>
            BuscarBotonesImagen()
        {
            return BuscarBotonesEnContenedor(this);
        }

        private System.Collections.Generic.IEnumerable<System.Windows.Controls.Button>
            BuscarBotonesEnContenedor(
                System.Windows.DependencyObject contenedor)
        {
            int cantidad = System.Windows.Media.VisualTreeHelper
                .GetChildrenCount(contenedor);

            for (int i = 0; i < cantidad; i++)
            {
                var hijo = System.Windows.Media.VisualTreeHelper
                    .GetChild(contenedor, i);

                if (hijo is System.Windows.Controls.Button boton &&
                    boton.Name != "btnQuitarImagen" &&
                    boton.Name != "btnGuardar" &&
                    boton.Name != "")
                {
                    yield return boton;
                }

                foreach (var descendiente in BuscarBotonesEnContenedor(hijo))
                {
                    yield return descendiente;
                }
            }
        }

        private static string ObtenerNombreImagen(string ruta)
        {
            if (Uri.TryCreate(ruta, UriKind.Absolute, out Uri? uri) &&
                uri.IsFile)
            {
                return Path.GetFileName(uri.LocalPath);
            }

            return Path.GetFileName(ruta);
        }

        // MOSTRAR LA VISTA PREVIA DE LA IMAGEN
        private void CargarVistaPrevia(string ruta)
        {
            try
            {
                BitmapImage imagen = new BitmapImage();

                imagen.BeginInit();
                imagen.CacheOption = BitmapCacheOption.OnLoad;

                if (Uri.TryCreate(ruta, UriKind.Absolute, out Uri? uri))
                {
                    imagen.UriSource = uri;
                }
                else if (File.Exists(ruta))
                {
                    imagen.UriSource = new Uri(
                        Path.GetFullPath(ruta));
                }
                else
                {
                    return;
                }

                imagen.EndInit();
                imgVistaPrevia.Source = imagen;
            }
            catch
            {
                // La imagen puede no estar disponible localmente.
            }
        }

        // SELECCIONAR UNA IMAGEN NUEVA
        private void SeleccionarImagen_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!materialOriginal.Activo)
                return;

            OpenFileDialog dialogo = new OpenFileDialog
            {
                Title = "Seleccionar imagen del material",
                Filter = "Imágenes (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
                Multiselect = false
            };

            if (dialogo.ShowDialog() != true)
                return;

            try
            {
                string archivo = dialogo.FileName;

                BitmapImage imagen = new BitmapImage();
                imagen.BeginInit();
                imagen.CacheOption = BitmapCacheOption.OnLoad;
                imagen.UriSource = new Uri(archivo);
                imagen.EndInit();

                rutaImagen = archivo;
                imagenModificada = true;

                txtImagen.Text = Path.GetFileName(archivo);
                txtImagen.ToolTip = archivo;

                imgVistaPrevia.Source = imagen;
                btnQuitarImagen.Visibility = Visibility.Visible;

                LimpiarErrorValidacion();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No fue posible cargar la imagen.\n\n" + ex.Message,
                    "Seleccionar imagen",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        // QUITAR LA IMAGEN
        private void QuitarImagen_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!materialOriginal.Activo)
                return;

            rutaImagen = null;
            imagenModificada = true;

            txtImagen.Clear();
            txtImagen.ToolTip = null;

            imgVistaPrevia.Source = null;
            btnQuitarImagen.Visibility = Visibility.Collapsed;

            LimpiarErrorValidacion();
        }

        // MOSTRAR MENSAJES EN ROJO
        private void MostrarErrorValidacion(string mensaje)
        {
            txtAvisoValidacion.Text = mensaje;
            txtAvisoValidacion.Visibility = Visibility.Visible;
        }

        private void LimpiarErrorValidacion()
        {
            txtAvisoValidacion.Text = "";
            txtAvisoValidacion.Visibility = Visibility.Collapsed;
        }

        private static bool TryLeerDecimal(
            string texto,
            out decimal valor)
        {
            texto = texto.Trim();

            if (decimal.TryParse(
                texto,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out valor))
            {
                return true;
            }

            return decimal.TryParse(
                texto,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out valor);
        }

        // COMPROBAR SI SE CAMBIÓ ALGÚN DATO
        private bool TieneCambios()
        {
            decimal.TryParse(
                txtPrecioCompra.Text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out decimal precioActual);

            if (!decimal.TryParse(
                txtPrecioCompra.Text,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out precioActual))
            {
                decimal.TryParse(
                    txtPrecioCompra.Text,
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out precioActual);
            }

            decimal.TryParse(
                txtStockMinimo.Text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out decimal minimoActual);

            if (!decimal.TryParse(
                txtStockMinimo.Text,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out minimoActual))
            {
                decimal.TryParse(
                    txtStockMinimo.Text,
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out minimoActual);
            }

            return
                txtNombre.Text.Trim() != materialOriginal.Nombre ||
                txtCodigo.Text.Trim() != materialOriginal.Codigo ||
                txtUnidadMedida.Text.Trim() != materialOriginal.UnidadMedida ||
                txtDescripcion.Text.Trim() != (materialOriginal.Descripcion ?? "") ||
                precioActual != materialOriginal.PrecioCompra ||
                minimoActual != materialOriginal.StockMinimo ||
                imagenModificada ||
                chkActivo.IsChecked != materialOriginal.Activo;
        }

        // CANCELAR Y CONFIRMAR DESCARTE
        private void Cancelar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (!TieneCambios())
            {
                DialogResult = false;
                return;
            }

            var confirmacion = new DescartarPacienteWindow
            {
                Owner = this
            };

            if (confirmacion.ShowDialog() == true)
            {
                DialogResult = false;
            }
        }

        // GUARDAR CAMBIOS
        private async void Guardar_Click(
            object sender,
            RoutedEventArgs e)
        {
            LimpiarErrorValidacion();

            bool activoNuevo = chkActivo.IsChecked == true;

            // MATERIAL ORIGINALMENTE DESHABILITADO
            // SOLO SE PERMITE REACTIVARLO
            if (!materialOriginal.Activo)
            {
                if (!activoNuevo)
                {
                    MostrarErrorValidacion(
                        "⚠ El material está deshabilitado. " +
                        "Habilítalo para poder modificar sus datos.");

                    return;
                }

                btnGuardar.IsEnabled = false;

                try
                {
                    await inventarioService.CambiarEstadoAsync(
                        materialOriginal.IdMaterial,
                        true);

                    DialogResult = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        "No fue posible habilitar el material.\n\n" +
                        ex.Message,
                        "Error al habilitar",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                finally
                {
                    btnGuardar.IsEnabled = true;
                }

                return;
            }

            string nombre = txtNombre.Text.Trim();
            string codigo = txtCodigo.Text.Trim();
            string unidadMedida = txtUnidadMedida.Text.Trim();
            string descripcion = txtDescripcion.Text.Trim();

            decimal precioCompra;
            decimal stockMinimo;

            // 1. NOMBRE
            if (string.IsNullOrWhiteSpace(nombre))
            {
                MostrarErrorValidacion(
                    "Ingresa el nombre del material.");

                txtNombre.Focus();
                return;
            }

            // 2. CÓDIGO
            if (string.IsNullOrWhiteSpace(codigo))
            {
                MostrarErrorValidacion("Ingresa un código.");

                txtCodigo.Focus();
                return;
            }

            // 3. PRECIO
            if (string.IsNullOrWhiteSpace(txtPrecioCompra.Text))
            {
                MostrarErrorValidacion(
                    "Ingresa el precio por unidad.");

                txtPrecioCompra.Focus();
                return;
            }

            if (!TryLeerDecimal(
                txtPrecioCompra.Text,
                out precioCompra))
            {
                MostrarErrorValidacion(
                    "Ingresa un precio válido.");

                txtPrecioCompra.Focus();
                return;
            }

            // 4. CANTIDAD MÍNIMA
            if (string.IsNullOrWhiteSpace(txtStockMinimo.Text))
            {
                MostrarErrorValidacion(
                    "Ingresa la cantidad mínima.");

                txtStockMinimo.Focus();
                return;
            }

            if (!TryLeerDecimal(
                txtStockMinimo.Text,
                out stockMinimo))
            {
                MostrarErrorValidacion(
                    "Ingresa una cantidad mínima válida.");

                txtStockMinimo.Focus();
                return;
            }

            // 5. UNIDAD DE MEDIDA
            if (string.IsNullOrWhiteSpace(unidadMedida))
            {
                MostrarErrorValidacion(
                    "Ingresa una unidad de medida.");

                txtUnidadMedida.Focus();
                return;
            }

            // 6. VALORES NEGATIVOS
            if (precioCompra < 0 || stockMinimo < 0)
            {
                MostrarErrorValidacion(
                    "Las cantidades y el precio no pueden ser negativos.");

                return;
            }

            // 7. LÍMITES ADMITIDOS
            if (stockMinimo > 9999999999.99m ||
                precioCompra > 99999999.99m)
            {
                MostrarErrorValidacion(
                    "Uno de los valores supera el máximo permitido.");

                return;
            }

            btnGuardar.IsEnabled = false;

            try
            {
                // ACTUALIZAR LOS DATOS DEL MATERIAL
                await inventarioService.ModificarAsync(
                    materialOriginal.IdMaterial,
                    codigo,
                    nombre,
                    unidadMedida,
                    stockMinimo,
                    precioCompra,
                    rutaImagen,
                    string.IsNullOrWhiteSpace(descripcion)
                        ? null
                        : descripcion);

                // CAMBIAR EL ESTADO SI CAMBIÓ
                if (activoNuevo != materialOriginal.Activo)
                {
                    await inventarioService.CambiarEstadoAsync(
                        materialOriginal.IdMaterial,
                        activoNuevo);
                }

                LimpiarErrorValidacion();
                DialogResult = true;
            }
            catch (Exception ex)
            {
                string mensajeError = ex.Message;

                // NOMBRE DUPLICADO
                if (mensajeError.Contains(
                        "Ya existe otro material con ese nombre",
                        StringComparison.OrdinalIgnoreCase) ||
                    mensajeError.Contains(
                        "Ya existe un material con ese nombre",
                        StringComparison.OrdinalIgnoreCase))
                {
                    MostrarErrorValidacion(
                        "Este material ya está ingresado.");
                }
                else
                {
                    MessageBox.Show(
                        "No fue posible actualizar el material.\n\n" +
                        mensajeError,
                        "Error al editar",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            finally
            {
                btnGuardar.IsEnabled = true;
            }
        }
    }
}
