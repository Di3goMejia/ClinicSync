using ClinicSyncDesktop.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClinicSyncDesktop.Views.Seguridad
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class SeguridadView : UserControl
    {
        // BOTÓN ACTUALMENTE SELECCIONADO
        /// <summary>
        /// The boton seleccionado
        /// </summary>
        private Button? botonSeleccionado;

        // COLOR ACTIVO
        /// <summary>
        /// The color activo
        /// </summary>
        private readonly Brush colorActivo = 
            new SolidColorBrush(Color.FromRgb(134, 197, 208));

        // COLOR INACTIVO
        /// <summary>
        /// The color inactivo
        /// </summary>
        private readonly Brush colorInactivo =
            new SolidColorBrush(Color.FromRgb(247, 249, 247));

        // ====================================
        // CONSTRUCTOR
        // ====================================

        /// <summary>
        /// Initializes a new instance of the <see cref="SeguridadView"/> class.
        /// </summary>
        public SeguridadView()
        {
            InitializeComponent();

            // USUARIOS SELECCIONADO POR DEFECTO
            SeleccionarPestana(BtnUsuarios);
        }

        // ====================================
        // EVENTO DE BOTONES
        // ====================================

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

        // ====================================
        // CAMBIAR PESTAÑA
        // ====================================

        /// <summary>
        /// Seleccionars the pestana.
        /// </summary>
        /// <param name="boton">The boton.</param>
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
        /// <summary>
        /// Abrirs the modulo.
        /// </summary>
        /// <param name="modulo">The modulo.</param>
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

