
using ClinicSyncDesktop.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClinicSyncDesktop.Views
{
    public partial class FinanzasView : UserControl
    {
        // BOTÓN ACTUALMENTE SELECCIONADO
        private Button? botonSeleccionado;

        // VISTAS DE FINANZAS
        private ResumenFinanzasView? vistaResumen;
        private IngresosView? vistaIngresos;
        private EgresosView? vistaEgresos;
        private GraficosView? vistaGraficos;

        // COLOR ACTIVO
        private readonly Brush colorActivo =
            new SolidColorBrush(
                Color.FromRgb(134, 197, 208));

        // COLOR INACTIVO
        private readonly Brush colorInactivo =
            new SolidColorBrush(
                Color.FromRgb(247, 249, 247));

        // CONSTRUCTOR
        public FinanzasView()
        {
            InitializeComponent();

            Loaded += FinanzasView_Loaded;
            Unloaded += FinanzasView_Unloaded;

            // RESUMEN SELECCIONADO POR DEFECTO
            SeleccionarPestana(BtnResumen);
        }

        // AL ENTRAR AL MÓDULO
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
