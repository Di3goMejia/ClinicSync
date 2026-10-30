
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClinicSyncDesktop.Views
{
    public partial class FinanzasView : UserControl
    {
        // BOTÓN ACTUALMENTE SELECCIONADO
        private Button? botonSeleccionado;

        // COLOR ACTIVO
        private readonly Brush colorActivo =
            new SolidColorBrush(
                Color.FromRgb(134, 197, 208));

        // COLOR INACTIVO
        private readonly Brush colorInactivo =
            new SolidColorBrush(
                Color.FromRgb(247, 249, 247));

        // ====================================
        // CONSTRUCTOR
        // ====================================

        public FinanzasView()
        {
            InitializeComponent();

            // INGRESOS SELECCIONADO POR DEFECTO
            SeleccionarPestana(BtnIngresos);
        }

        // ====================================
        // EVENTO DE BOTONES
        // ====================================

        private void Pestana_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is Button boton)
            {
                SeleccionarPestana(boton);
            }
        }

        // ====================================
        // CAMBIAR PESTAÑA
        // ====================================

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
            boton.Background =
                colorActivo;

            botonSeleccionado =
                boton;

            // OBTENER MÓDULO
            string modulo =
                boton.Tag?.ToString() ?? "";

            AbrirModulo(modulo);
        }

        // ====================================
        // NAVEGACIÓN INTERNA DE FINANZAS
        // ====================================
        private void AbrirModulo(string modulo)
        {
            switch (modulo)
            {
                case "Resumen":
                    ContenidoFinanzas.Content = null;
                    break;

                case "Ingresos":
                    ContenidoFinanzas.Content = new IngresosView();
                    break;

                case "Egresos":
                    ContenidoFinanzas.Content =
                       new EgresosView(
                           new ClinicSyncDesktop.Services.EgresosApiService());
                    break;

                case "Graficos":
                    ContenidoFinanzas.Content = null;
                    break;
            }
        }

    }
}

