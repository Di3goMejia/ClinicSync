using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Data;
using System;
using System.Collections.Generic;
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
    public partial class InventarioView : UserControl
    {
        /// <summary>
        /// The inventario service
        /// </summary>
        private readonly InventarioApiService inventarioService = new();

        /// <summary>
        /// The materiales
        /// </summary>
        private List<Material> materiales = new();

        // Evita descargar repetidamente las mismas imágenes.
        /// <summary>
        /// The imagenes cache
        /// </summary>
        private readonly Dictionary<string, BitmapImage> imagenesCache =
            new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="InventarioView"/> class.
        /// </summary>
        public InventarioView()
        {
            InitializeComponent();

            PanelAvisos.Visibility = Visibility.Collapsed;
            Loaded += InventarioView_Loaded;
        }

        /// <summary>
        /// Handles the Loaded event of the InventarioView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void InventarioView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            Loaded -= InventarioView_Loaded;
            await CargarMaterialesAsync();
        }

        // MOSTRAR AVISOS
        /// <summary>
        /// Mostrars the aviso.
        /// </summary>
        /// <param name="mensaje">The mensaje.</param>
        /// <param name="exitoso">if set to <c>true</c> [exitoso].</param>
        private void MostrarAviso(string mensaje, bool exitoso)
        {
            PanelAvisos.Children.Add(
                new AvisoNotificacion(mensaje, exitoso));

            PanelAvisos.Visibility = Visibility.Visible;
        }

        // CARGAR MATERIALES
        /// <summary>
        /// Cargars the materiales asynchronous.
        /// </summary>
        private async Task CargarMaterialesAsync()
        {
            try
            {
                materiales = await inventarioService.ListarAsync();
                AplicarBusqueda();
            }
            catch (Exception ex)
            {
                MostrarAviso(
                    "No fue posible cargar el inventario. " + ex.Message,
                    false);
            }
        }

        // BUSCAR MATERIAL
        /// <summary>
        /// Handles the TextChanged event of the txtBuscar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void txtBuscar_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            AplicarBusqueda();
        }

        /// <summary>
        /// Aplicars the busqueda.
        /// </summary>
        private void AplicarBusqueda()
        {
            if (dgMateriales == null)
                return;

            string texto = txtBuscar.Text?.Trim() ?? "";

            var resultado = materiales
                .Where(m =>
                    string.IsNullOrWhiteSpace(texto) ||
                    (m.Nombre?.Contains(
                        texto,
                        StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (m.Codigo?.Contains(
                        texto,
                        StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

            dgMateriales.ItemsSource = resultado;
        }

        // ABRIR VENTANA PARA AGREGAR
        /// <summary>
        /// Handles the Click event of the btnAgregar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void btnAgregar_Click(
            object sender,
            RoutedEventArgs e)
        {
            var ventana = new AgregarMaterialWindow(inventarioService)
            {
                Owner = Window.GetWindow(this)
            };

            if (ventana.ShowDialog() == true)
            {
                await CargarMaterialesAsync();

                MostrarAviso(
                    "Material registrado exitosamente.",
                    true);
            }
        }

        // CARGAR MINIATURA DESDE LA API
        /// <summary>
        /// Handles the Loaded event of the ImagenMaterial control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ImagenMaterial_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Image imagenControl ||
                imagenControl.DataContext is not Material material)
            {
                return;
            }

            string? referencia = material.Imagen;

            if (string.IsNullOrWhiteSpace(referencia))
            {
                imagenControl.Visibility = Visibility.Collapsed;
                return;
            }

            if (imagenesCache.TryGetValue(
                referencia,
                out BitmapImage? imagenGuardada))
            {
                imagenControl.Source = imagenGuardada;
                imagenControl.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                byte[] bytes =
                    await ClienteApi.DescargarImagenMaterialAsync(
                        referencia);

                // La fila puede haber cambiado mientras se descargaba.
                if (!string.Equals(
                    material.Imagen,
                    referencia,
                    StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                BitmapImage bitmap = CrearBitmap(bytes);

                imagenesCache[referencia] = bitmap;

                imagenControl.Source = bitmap;
                imagenControl.Visibility = Visibility.Visible;
            }
            catch
            {
                // Si la descarga falla, se conserva el icono de respaldo.
                imagenControl.Source = null;
                imagenControl.Visibility = Visibility.Collapsed;
            }
        }

        // CREAR IMAGEN DESDE BYTES
        /// <summary>
        /// Crears the bitmap.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns></returns>
        private static BitmapImage CrearBitmap(byte[] bytes)
        {
            using var flujo = new MemoryStream(bytes);

            var bitmap = new BitmapImage();

            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = flujo;
            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }

        // AMPLIAR IMAGEN
        /// <summary>
        /// Handles the Click event of the VerImagen control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void VerImagen_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button boton ||
                boton.DataContext is not Material material)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(material.Imagen))
            {
                MessageBox.Show(
                    "Este material no tiene una imagen registrada.",
                    "Imagen del material",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            try
            {
                BitmapImage bitmap;

                if (!imagenesCache.TryGetValue(
                    material.Imagen,
                    out bitmap!))
                {
                    byte[] bytes =
                        await ClienteApi.DescargarImagenMaterialAsync(
                            material.Imagen);

                    bitmap = CrearBitmap(bytes);
                    imagenesCache[material.Imagen] = bitmap;
                }

                var ventanaImagen = new ImagenMaterialWindow(
                    material.Nombre,
                    bitmap)
                {
                    Owner = Window.GetWindow(this)
                };

                ventanaImagen.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No fue posible cargar la imagen del material.\n\n" +
                    ex.Message,
                    "Error al mostrar imagen",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // ABRIR VENTANA PARA EDITAR
        /// <summary>
        /// Handles the Click event of the Editar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Editar_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button boton ||
                boton.DataContext is not Material material)
            {
                return;
            }

            var ventana = new EditarMaterialWindow(
                inventarioService,
                material)
            {
                Owner = Window.GetWindow(this)
            };

            if (ventana.ShowDialog() == true)
            {
                await CargarMaterialesAsync();

                MostrarAviso(
                    "Material actualizado exitosamente.",
                    true);
            }
        }
    }
}
