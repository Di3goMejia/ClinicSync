using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Validations;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace ClinicSyncDesktop.Views
{
    public partial class AgregarMaterialWindow : Window
    {
        private readonly InventarioApiService inventarioService;
        private string? rutaImagen;
        private readonly ValidadorMaterial validador = new();
        private readonly Dictionary<string, Control> controles;

        public AgregarMaterialWindow(InventarioApiService servicio)
        {
            InitializeComponent();
            inventarioService = servicio;
            controles = new Dictionary<string, Control>
            {
                [nameof(EntradaMaterial.Nombre)] = txtNombre,
                [nameof(EntradaMaterial.Codigo)] = txtCodigo,
                [nameof(EntradaMaterial.UnidadMedida)] = txtUnidadMedida,
                [nameof(EntradaMaterial.TextoPrecioCompra)] = txtPrecioCompra,
                [nameof(EntradaMaterial.TextoCantidadInicial)] = txtCantidadInicial,
                [nameof(EntradaMaterial.TextoStockMinimo)] = txtStockMinimo
            };
        }

        // SELECCIONAR IMAGEN
        private void SeleccionarImagen_Click(
            object sender,
            RoutedEventArgs e)
        {
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

                txtImagen.Text = System.IO.Path.GetFileName(archivo);
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

        // QUITAR IMAGEN
        private void QuitarImagen_Click(
            object sender,
            RoutedEventArgs e)
        {
            rutaImagen = null;

            txtImagen.Clear();
            txtImagen.ToolTip = null;

            imgVistaPrevia.Source = null;
            btnQuitarImagen.Visibility = Visibility.Collapsed;
        }

        // MOSTRAR ERROR DE VALIDACIÓN
        private void MostrarErrorValidacion(string mensaje)
        {
            txtAvisoValidacion.Text = mensaje;
            txtAvisoValidacion.Visibility = Visibility.Visible;
        }

        // OCULTAR ERROR DE VALIDACIÓN
        private void LimpiarErrorValidacion()
        {
            txtAvisoValidacion.Text = "";
            txtAvisoValidacion.Visibility = Visibility.Collapsed;
        }

        // COMPROBAR SI EL FORMULARIO TIENE CAMBIOS
        private bool TieneCambios()
        {
            return
                !string.IsNullOrWhiteSpace(txtNombre.Text) ||
                !string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                !string.IsNullOrWhiteSpace(txtUnidadMedida.Text) ||
                !string.IsNullOrWhiteSpace(txtDescripcion.Text) ||
                !string.IsNullOrWhiteSpace(rutaImagen) ||
                !EsCero(txtCantidadInicial.Text) ||
                !EsCero(txtStockMinimo.Text) ||
                !EsCero(txtPrecioCompra.Text);
        }

        private static bool EsCero(string texto)
        {
            if (decimal.TryParse(
                texto,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out decimal valor))
            {
                return valor == 0;
            }

            if (decimal.TryParse(
                texto,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out valor))
            {
                return valor == 0;
            }

            return string.IsNullOrWhiteSpace(texto);
        }

        // CERRAR CON LA X
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

        // REGISTRAR MATERIAL
        private async void Guardar_Click(
            object sender,
            RoutedEventArgs e)
        {
            LimpiarErrorValidacion();

            var entrada = new EntradaMaterial(txtNombre.Text.Trim(), txtCodigo.Text.Trim(),
                txtUnidadMedida.Text.Trim(), txtPrecioCompra.Text.Trim(),
                txtCantidadInicial.Text.Trim(), txtStockMinimo.Text.Trim());
            var resultado = validador.Validar(entrada);
            if (!HelperValidacion.AplicarResultado(resultado, controles,
                MostrarErrorValidacion, LimpiarErrorValidacion))
                return;

            string nombre = entrada.Nombre;
            string codigo = entrada.Codigo;
            string unidadMedida = entrada.UnidadMedida;
            string descripcion = txtDescripcion.Text.Trim();
            decimal precioCompra = resultado.PrecioCompra;
            decimal cantidadInicial = resultado.CantidadInicial;
            decimal stockMinimo = resultado.StockMinimo;

            btnGuardar.IsEnabled = false;

            try
            {
                await inventarioService.RegistrarAsync(
                    codigo,
                    nombre,
                    unidadMedida,
                    stockMinimo,
                    cantidadInicial,
                    precioCompra,
                    rutaImagen,
                    string.IsNullOrWhiteSpace(descripcion)
                        ? null
                        : descripcion);

                LimpiarErrorValidacion();
                DialogResult = true;
            }
            catch (Exception ex)
            {
                string mensajeError = ex.Message;

                // MATERIAL DUPLICADO: MOSTRAR DENTRO DEL FORMULARIO
                if (mensajeError.Contains(
                    "Ya existe un material con ese nombre",
                    StringComparison.OrdinalIgnoreCase))
                {
                    MostrarErrorValidacion(
                        "Ya existe un material con ese nombre.");
                }
                else
                {
                    MessageBox.Show(
                        "No fue posible registrar el material.\n\n" +
                        mensajeError,
                        "Error al registrar",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            finally
            {
                btnGuardar.IsEnabled = true;
            }
        }

        // CONVERTIR TEXTO A DECIMAL
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
    }
}
