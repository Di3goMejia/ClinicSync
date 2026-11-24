
using ClinicSyncDesktop.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClinicSyncDesktop.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class FinanzasView : UserControl
    {
        // BOTÓN ACTUALMENTE SELECCIONADO
        /// <summary>
        /// The boton seleccionado
        /// </summary>
        private Button? botonSeleccionado;

        // VISTAS DE FINANZAS
        /// <summary>
        /// The vista resumen
        /// </summary>
        private ResumenFinanzasView? vistaResumen;
        /// <summary>
        /// The vista ingresos
        /// </summary>
        private IngresosView? vistaIngresos;
        /// <summary>
        /// The vista egresos
        /// </summary>
        private EgresosView? vistaEgresos;
        /// <summary>
        /// The vista graficos
        /// </summary>
        private GraficosView? vistaGraficos;

        // COLOR ACTIVO
        /// <summary>
        /// The color activo
        /// </summary>
        private readonly Brush colorActivo =
            new SolidColorBrush(
                Color.FromRgb(134, 197, 208));

        // COLOR INACTIVO
        /// <summary>
        /// The color inactivo
        /// </summary>
        private readonly Brush colorInactivo =
            new SolidColorBrush(
                Color.FromRgb(247, 249, 247));

        // CONSTRUCTOR
        /// <summary>
        /// Initializes a new instance of the <see cref="FinanzasView"/> class.
        /// </summary>
        public FinanzasView()
        {
            InitializeComponent();

            Loaded += FinanzasView_Loaded;
            Unloaded += FinanzasView_Unloaded;

            // RESUMEN SELECCIONADO POR DEFECTO
            SeleccionarPestana(BtnResumen);
        }

        // AL ENTRAR AL MÓDULO
        /// <summary>
        /// Handles the Loaded event of the FinanzasView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void FinanzasView_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            // SI SE HABÍAN DESCARTADO LAS VISTAS,
            // VOLVER A CREAR RESUMEN
            if (ContenidoFinanzas.Content == null)
            {
                SeleccionarPestana(BtnResumen);
            }
        }

        // AL SALIR DEL MÓDULO
        /// <summary>
        /// Handles the Unloaded event of the FinanzasView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void FinanzasView_Unloaded(
            object sender,
            RoutedEventArgs e)
        {
            // DESCARTAR VISTAS PARA QUE LOS FILTROS
            // SE REINICIEN EN LA SIGUIENTE ENTRADA
            ContenidoFinanzas.Content = null;

            vistaResumen = null;
            vistaIngresos = null;
            vistaEgresos = null;
            vistaGraficos = null;

            // RESTAURAR BOTONES SUPERIORES
            BtnResumen.Background = colorInactivo;
            BtnIngresos.Background = colorInactivo;
            BtnEgresos.Background = colorInactivo;
            BtnGraficos.Background = colorInactivo;

            botonSeleccionado = null;
        }

        // EVENTO DE BOTONES
        /// <summary>
        /// Handles the Click event of the Pestana control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Pestana_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button boton)
            {
                SeleccionarPestana(boton);
            }
        }

        // CAMBIAR PESTAÑA
        /// <summary>
        /// Seleccionars the pestana.
        /// </summary>
        /// <param name="boton">The boton.</param>
        private void SeleccionarPestana(
            Button boton)
        {
            // RESTAURAR COLOR ANTERIOR
            if (botonSeleccionado != null)
            {
                botonSeleccionado.Background =
                    colorInactivo;
            }

            // ACTIVAR NUEVO BOTÓN
            boton.Background = colorActivo;

            botonSeleccionado = boton;

            // OBTENER MÓDULO
            string modulo =
                boton.Tag?.ToString() ?? "";

            AbrirModulo(modulo);
        }

        // NAVEGACIÓN INTERNA DE FINANZAS
        /// <summary>
        /// Abrirs the modulo.
        /// </summary>
        /// <param name="modulo">The modulo.</param>
        private void AbrirModulo(string modulo)
        {
            switch (modulo)
            {
                case "Resumen":

                    vistaResumen ??=
                        new ResumenFinanzasView(
                            new ResumenFinanzasApiService());

                    ContenidoFinanzas.Content = vistaResumen;

                    break;

                case "Ingresos":

                    vistaIngresos ??= new IngresosView();

                    ContenidoFinanzas.Content = vistaIngresos;

                    break;

                case "Egresos":

                    vistaEgresos ??=
                        new EgresosView(
                            new EgresosApiService());

                    ContenidoFinanzas.Content = vistaEgresos;

                    break;

                case "Graficos":

                    vistaGraficos ??=
                        new GraficosView(
                            new GraficosApiService());

                    ContenidoFinanzas.Content = vistaGraficos;

                    break;
            }
        }
    }
}
