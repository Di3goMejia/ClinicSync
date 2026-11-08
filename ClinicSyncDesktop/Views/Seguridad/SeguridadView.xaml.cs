using ClinicSyncDesktop.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClinicSyncDesktop.Views
{
    public partial class SeguridadView : UserControl
    {
        // BOTÓN ACTUALMENTE SELECCIONADO
        private Button? botonSeleccionado;

        // COLOR ACTIVO
        private readonly Brush colorActivo = 
            new SolidColorBrush(Color.FromRgb(134, 197, 208));

        // COLOR INACTIVO
        private readonly Brush colorInactivo =
            new SolidColorBrush(Color.FromRgb(247, 249, 247));

        // ====================================
        // CONSTRUCTOR
        // ====================================

        public SeguridadView()
        {
            InitializeComponent();

            // USUARIOS SELECCIONADO POR DEFECTO
            SeleccionarPestana(BtnUsuarios);
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
            botonSeleccionado?.Background = colorInactivo;

            // ACTIVAR NUEVO BOTÓN
            boton.Background = colorActivo;
            botonSeleccionado = boton;

            // OBTENER MÓDULO
            string modulo = boton.Tag?.ToString() ?? "";
            AbrirModulo(modulo);
        }

        // ====================================
        // NAVEGACIÓN INTERNA DE FINANZAS
        // ====================================
        private void AbrirModulo(string modulo)
        {
            switch (modulo)
            {
                case "Usuarios":
                    ContenidoSeguridad.Content = new UsuariosView(
                        new UsuariosApiService());
                    break;

                case "Roles":
                    ContenidoSeguridad.Content = new RolesView(
                        new RolesApiService());
                    break;
            }
        }

    }
}

