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
    public partial class NotificacionesView : UserControl
    {
        // SERVICIO DE NOTIFICACIONES
        private readonly NotificacionesApiService servicio = new();

        // LISTA COMPLETA
        private Notificacion[] todas =
            Array.Empty<Notificacion>();

        private bool listo;
        private bool cargando;

        // CONSTRUCTOR
        public NotificacionesView()
        {
            InitializeComponent();

            Loaded += NotificacionesView_Loaded;
        }

        // CARGAR AL ABRIR EL MÓDULO
        private async void NotificacionesView_Loaded(
            object sender, RoutedEventArgs e)
        {
            if (listo)
                return;

            listo = true;

            await RecargarAsync();
        }

        // CONSULTAR API
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
        private void FiltroCambiado(
            object sender,
            TextChangedEventArgs e)
        {
            Pintar();
        }

        // FILTRO POR FECHA
        private void FiltroFechaCambiado(
            object sender,
            SelectionChangedEventArgs e)
        {
            Pintar();
        }

        // FILTROS DE TIPO Y ESTADO
        private void ComboCambiado(
            object sender,
            SelectionChangedEventArgs e)
        {
            Pintar();
        }

        // MARCAR TODAS COMO LEÍDAS
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
        private void MostrarError(Exception ex)
        {
            ErrorListado.Text = ex.Message;
            ErrorListado.Visibility = Visibility.Visible;
        }

        // LIMPIAR LOS FILTROS
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

