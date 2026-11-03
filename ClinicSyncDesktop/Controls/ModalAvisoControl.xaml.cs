using System;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSyncDesktop.Controls
{
    public partial class ModalAvisoControl : UserControl
    {
        public event EventHandler? AceptarClick;
        public event EventHandler? CerrarClick;

        public ModalAvisoControl()
        {
            InitializeComponent();
        }

        public string Mensaje
        {
            get => TxtMensaje.Text;
            set => TxtMensaje.Text = value;
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
