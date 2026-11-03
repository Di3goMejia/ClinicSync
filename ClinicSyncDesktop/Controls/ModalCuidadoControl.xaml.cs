using System;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSyncDesktop.Controls
{
    public partial class ModalCuidadoControl : UserControl
    {
        public event EventHandler? SiClick;
        public event EventHandler? NoClick;

        public ModalCuidadoControl()
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

        private void BtnSi_Click(object sender, RoutedEventArgs e)
        {
            SiClick?.Invoke(this, EventArgs.Empty);
        }

        private void BtnNo_Click(object sender, RoutedEventArgs e)
        {
            NoClick?.Invoke(this, EventArgs.Empty);
        }
    }
}
