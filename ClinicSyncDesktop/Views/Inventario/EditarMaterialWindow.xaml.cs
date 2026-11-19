using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Validations;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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

            chkActivo.IsChecked = materialOriginal.Activo;

            rutaImagen = materialOriginal.Imagen;

            if (!string.IsNullOrWhiteSpace(rutaImagen))
            {
                txtImagen.Text = ObtenerNombreImagen(rutaImagen);
                txtImagen.ToolTip = rutaImagen;
                btnQuitarImagen.Visibility = Visibility.Visible;

                CargarVistaPrevia(rutaImagen);
            }

            EstablecerEdicionHabilitada(materialOriginal.Activo);

            if (!materialOriginal.Activo)
            {
                MostrarErrorValidacion(
                    "El material está deshabilitado. " +
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
            txtDescripcion.IsEnabled = habilitada;

            // El stock puede editarse si el material está habilitado.
            txtStockActual.IsEnabled = habilitada;

            // La unidad de medida siempre permanece bloqueada.
            txtUnidadMedida.IsReadOnly = true;
            txtUnidadMedida.Focusable = false;

            // El motivo solo puede editarse si el material está habilitado.
            txtMotivoAjuste.IsEnabled = habilitada;

            btnQuitarImagen.IsEnabled = habilitada;

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
                    imagen.UriSource = new Uri(Path.GetFullPath(ruta));
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

        // MOSTRAR MENSAJES DE VALIDACIÓN
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

        // LEER DECIMALES SEGÚN LA CONFIGURACIÓN REGIONAL
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
            TryLeerDecimal(txtPrecioCompra.Text, out decimal precioActual);
            TryLeerDecimal(txtStockMinimo.Text, out decimal minimoActual);

            bool stockValido = TryLeerDecimal(
                txtStockActual.Text,
                out decimal stockActual);

            return
                txtNombre.Text.Trim() != materialOriginal.Nombre ||
                txtCodigo.Text.Trim() != materialOriginal.Codigo ||
                txtUnidadMedida.Text.Trim() != materialOriginal.UnidadMedida ||
                txtDescripcion.Text.Trim() != (materialOriginal.Descripcion ?? "") ||
                precioActual != materialOriginal.PrecioCompra ||
                minimoActual != materialOriginal.StockMinimo ||
                !stockValido ||
                stockActual != materialOriginal.StockActual ||
                !string.IsNullOrWhiteSpace(txtMotivoAjuste.Text) ||
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

            var confirmacion = new ConfirmarDescarteWindow
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

            // SI ESTABA DESHABILITADO, SOLO SE PERMITE REACTIVARLO
            if (!materialOriginal.Activo)
            {
                if (!activoNuevo)
                {
                    MostrarErrorValidacion(
                        "El material está deshabilitado. " +
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
            string motivoAjuste = txtMotivoAjuste.Text.Trim();

            decimal precioCompra;
            decimal stockMinimo;
            decimal stockActual;

            var resultadoValidacion = new ValidadorMaterialEdicion().Validar(
                new EntradaMaterialEdicion(nombre, codigo, unidadMedida,
                    txtPrecioCompra.Text.Trim(), txtStockMinimo.Text.Trim(),
                    txtStockActual.Text.Trim(), motivoAjuste,
                    materialOriginal.StockActual));
            if (!HelperValidacion.AplicarResultado(resultadoValidacion,
                new Dictionary<string, Control>
                {
                    [nameof(EntradaMaterialEdicion.Nombre)] = txtNombre,
                    [nameof(EntradaMaterialEdicion.Codigo)] = txtCodigo,
                    [nameof(EntradaMaterialEdicion.UnidadMedida)] = txtUnidadMedida,
                    [nameof(EntradaMaterialEdicion.TextoPrecioCompra)] = txtPrecioCompra,
                    [nameof(EntradaMaterialEdicion.TextoStockMinimo)] = txtStockMinimo,
                    [nameof(EntradaMaterialEdicion.TextoStockActual)] = txtStockActual,
                    [nameof(EntradaMaterialEdicion.MotivoAjuste)] = txtMotivoAjuste
                }, MostrarErrorValidacion, LimpiarErrorValidacion))
                return;
            precioCompra = resultadoValidacion.PrecioCompra;
            stockMinimo = resultadoValidacion.StockMinimo;
            stockActual = resultadoValidacion.StockActual;

            bool cambioStock = stockActual != materialOriginal.StockActual;

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
                        : descripcion,
                    stockActual,
                    materialOriginal.StockActual,
                    cambioStock ? motivoAjuste : null);

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