using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;

namespace ClinicSyncDesktop.Views.Notificaciones
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class NotificacionesView : UserControl
    {
        // SERVICIO DE NOTIFICACIONES
        /// <summary>
        /// The servicio
        /// </summary>
        private readonly NotificacionesApiService servicio = new();

        // LISTA COMPLETA
        /// <summary>
        /// The todas
        /// </summary>
        private Notificacion[] todas =
            Array.Empty<Notificacion>();

        /// <summary>
        /// The listo
        /// </summary>
        private bool listo;
        /// <summary>
        /// The cargando
        /// </summary>
        private bool cargando;

        // CONSTRUCTOR
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificacionesView"/> class.
        /// </summary>
        public NotificacionesView()
        {
            InitializeComponent();

            Loaded += NotificacionesView_Loaded;
        }

        // CARGAR AL ABRIR EL MÓDULO
        /// <summary>
        /// Handles the Loaded event of the NotificacionesView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void NotificacionesView_Loaded(
            object sender, RoutedEventArgs e)
        {
            if (listo)
                return;

            listo = true;

            await RecargarAsync();
        }

        // CONSULTAR API
        /// <summary>
        /// Recargars the asynchronous.
        /// </summary>
        private async Task RecargarAsync()
        {
            if (cargando)
                return;

            cargando = true;

            ErrorListado.Text = "";
            ErrorListado.Visibility = Visibility.Collapsed;

            try
            {
                todas = await servicio.ListarAsync();

                Pintar();
            }
            catch (Exception ex)
            {
                ErrorListado.Text =
                    "No se pudieron cargar las notificaciones: " +
                    ex.Message;

                ErrorListado.Visibility = Visibility.Visible;
            }
            finally
            {
                cargando = false;
            }
        }

        // MOSTRAR NOTIFICACIONES SEGÚN LOS FILTROS
        /// <summary>
        /// Pintars this instance.
        /// </summary>
        private void Pintar()
        {
            if (!listo ||
                Lista == null ||
                CmbTipo == null ||
                CmbEstado == null ||
                TxtBuscar == null ||
                DpDesde == null)
            {
                return;
            }

            string? tipo =
                (CmbTipo.SelectedItem as ComboBoxItem)?
                    .Tag?.ToString();

            string? estado =
                (CmbEstado.SelectedItem as ComboBoxItem)?
                    .Tag?.ToString();

            string buscar = TxtBuscar.Text.Trim();

            var consulta = todas
                .Where(n =>
                    !n.Archivada &&
                    (string.IsNullOrEmpty(tipo) ||
                     n.Tipo == tipo) &&

                    (string.IsNullOrEmpty(estado) ||
                     (estado == "LEIDA"
                         ? n.Leida
                         : !n.Leida)) &&

                    (!DpDesde.SelectedDate.HasValue ||
                     n.FechaCreacion.Date >=
                         DpDesde.SelectedDate.Value.Date) &&

                    (buscar.Length == 0 ||
                     n.Titulo.Contains(
                         buscar,
                         StringComparison.OrdinalIgnoreCase) ||
                     n.Descripcion.Contains(
                         buscar,
                         StringComparison.OrdinalIgnoreCase)))
                .OrderBy(n => n.Leida)
                .ThenByDescending(n => n.FechaCreacion)
                .ToList();

            // LIMPIAR LISTADO
            Lista.Children.Clear();

            // MOSTRAR MENSAJE SI NO EXISTEN NOTIFICACIONES
            if (consulta.Count == 0)
            {
                Lista.Children.Add(new TextBlock
                {
                    Text =
                        "No hay notificaciones para los filtros seleccionados.",

                    Foreground = new SolidColorBrush(
                        Color.FromRgb(23, 71, 90)),

                    Margin = new Thickness(14, 20, 14, 0),

                    TextWrapping = TextWrapping.Wrap,

                    HorizontalAlignment =
                        HorizontalAlignment.Center
                });

                return;
            }

            // CREAR TARJETAS
            foreach (var n in consulta)
            {
                Lista.Children.Add(
                    NotificacionesVisuales.CrearTarjeta(
                        n,

                        // MARCAR COMO LEÍDA
                        async () =>
                        {
                            await servicio.MarcarLeidaAsync(
                                n.IdNotificacion);

                            await RecargarAsync();
                        },

                        // ARCHIVAR
                        async () =>
                        {
                            await servicio.ArchivarAsync(
                                n.IdNotificacion);

                            await RecargarAsync();
                        },

                        true));
            }
        }

        // BUSCAR
        /// <summary>
        /// Filtroes the cambiado.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void FiltroCambiado(
            object sender,
            TextChangedEventArgs e)
        {
            Pintar();
        }

        // FILTRO POR FECHA
        /// <summary>
        /// Filtroes the fecha cambiado.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void FiltroFechaCambiado(
            object sender,
            SelectionChangedEventArgs e)
        {
            Pintar();
        }

        // FILTROS DE TIPO Y ESTADO
        /// <summary>
        /// Comboes the cambiado.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void ComboCambiado(
            object sender,
            SelectionChangedEventArgs e)
        {
            Pintar();
        }

        // MARCAR TODAS COMO LEÍDAS
        /// <summary>
        /// Handles the Click event of the MarcarTodas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void MarcarTodas_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                await servicio.MarcarTodasAsync();

                await RecargarAsync();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        // ARCHIVAR NOTIFICACIONES LEÍDAS
        /// <summary>
        /// Handles the Click event of the ArchivarLeidas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void ArchivarLeidas_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                foreach (var n in todas.Where(
                    n => n.Leida && !n.Archivada))
                {
                    await servicio.ArchivarAsync(
                        n.IdNotificacion);
                }

                await RecargarAsync();
            }
            catch (Exception ex)
            {
                MostrarError(ex);
            }
        }

        // MOSTRAR ERRORES EN PANTALLA
        /// <summary>
        /// Mostrars the error.
        /// </summary>
        /// <param name="ex">The ex.</param>
        private void MostrarError(Exception ex)
        {
            ErrorListado.Text = ex.Message;
            ErrorListado.Visibility = Visibility.Visible;
        }

        // LIMPIAR LOS FILTROS
        /// <summary>
        /// Handles the Click event of the Limpiar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Limpiar_Click(
            object sender,
            RoutedEventArgs e)
        {
            CmbTipo.SelectedIndex = 0;
            CmbEstado.SelectedIndex = 0;
            DpDesde.SelectedDate = null;
            TxtBuscar.Text = "";

            Pintar();
        }
    }
}

