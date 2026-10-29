
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClinicSyncDesktop.Views
{
    public partial class FinanzasView : UserControl
    {
        // BOTÓN SELECCIONADO
        private Button? botonSeleccionado;

        // COLOR DEL BOTÓN ACTIVO
        private readonly Brush colorActivo =
            new SolidColorBrush(
                Color.FromRgb(134, 197, 208));

        // COLOR DE LOS BOTONES INACTIVOS
        private readonly Brush colorInactivo =
            new SolidColorBrush(
                Color.FromRgb(247, 249, 247));

        // ====================================
        // CONSTRUCTOR
        // ====================================

        public FinanzasView()
        {
            InitializeComponent();

            // SELECCIONAR INGRESOS AL INICIAR
            SeleccionarPestana(BtnIngresos);
        }

        // ====================================
        // EVENTO DE LOS BOTONES
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
        // SELECCIONAR PESTAÑA
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

            // COLOREAR BOTÓN ACTUAL
            boton.Background = colorActivo;

            // GUARDAR BOTÓN SELECCIONADO
            botonSeleccionado = boton;

            // OBTENER NOMBRE DEL MÓDULO
            string modulo =
                boton.Tag?.ToString() ?? "";

            // ABRIR MÓDULO SELECCIONADO
            AbrirModulo(modulo);
        }

        // ====================================
        // NAVEGACIÓN DE FINANZAS
        // ====================================

        private void AbrirModulo(string modulo)
        {
            switch (modulo)
            {
                // RESUMEN
                case "Resumen":

                    ContenidoFinanzas.Content = null;

                    break;

                // INGRESOS
                case "Ingresos":

                    ContenidoFinanzas.Content = null;

                    break;

                // EGRESOS
                case "Egresos":

                    ContenidoFinanzas.Content = null;

                    break;

                // GRÁFICOS
                case "Graficos":

                    ContenidoFinanzas.Content = null;

                    break;
            }
        }
    }
}

