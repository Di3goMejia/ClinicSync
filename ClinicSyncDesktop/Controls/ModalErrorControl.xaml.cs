using System;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSyncDesktop.Controls
{
    public partial class ModalErrorControl : UserControl
    {
        public event EventHandler? AceptarClick;
        public event EventHandler? CerrarClick;

        public ModalErrorControl()
        {
            InitializeComponent();
        }

        public string Mensaje
        {
            get => TxtMensaje.Text;
            set => TxtMensaje.Text = value;
        }

        public string? Detalle
        {
            get => TxtDetalle.Text;
            set
            {
                TxtDetalle.Text = value ?? "";
                TxtDetalle.Visibility = string.IsNullOrWhiteSpace(value) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public string Titulo
        {
            get => TxtTitulo.Text;
            set => TxtTitulo.Text = value;
        }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            AceptarClick?.Invoke(this, EventArgs.Empty);
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            CerrarClick?.Invoke(this, EventArgs.Empty);
        }
    }
}
