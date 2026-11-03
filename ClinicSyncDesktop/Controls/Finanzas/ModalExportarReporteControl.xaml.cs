using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ClinicSyncDesktop.Controls.Finanzas
{
    public class ExportarConfigEventArgs : EventArgs
    {
        public string Formato { get; set; } = "PDF";
        public bool IncluirGraficos { get; set; }
    }

    public partial class ModalExportarReporteControl : UserControl
    {
        public event EventHandler<ExportarConfigEventArgs>? ExportarClick;
        public event EventHandler? CerrarClick;

        private string _formatoSeleccionado = "PDF";

        public ModalExportarReporteControl()
        {
            InitializeComponent();
            ActualizarSeleccionFormato();
        }

        public string FormatoSeleccionado => _formatoSeleccionado;
        public bool IncluirGraficos => ChkIncluirGraficos.IsChecked == true;

        private void ActualizarSeleccionFormato()
        {
            var brushAzul = (SolidColorBrush)FindResource("ColorAzul");

            if (_formatoSeleccionado == "PDF")
            {
                CardPdf.BorderBrush = brushAzul;
                CardPdf.Background = new SolidColorBrush(Color.FromRgb(0xEB, 0xF5, 0xF8));
                CardExcel.BorderBrush = Brushes.Transparent;
                CardExcel.Background = new SolidColorBrush(Color.FromRgb(0xEB, 0xF1, 0xF4));
            }
            else
            {
                CardExcel.BorderBrush = brushAzul;
                CardExcel.Background = new SolidColorBrush(Color.FromRgb(0xEB, 0xF5, 0xF8));
                CardPdf.BorderBrush = Brushes.Transparent;
                CardPdf.Background = new SolidColorBrush(Color.FromRgb(0xEB, 0xF1, 0xF4));
            }
        }

        private void CardPdf_Click(object sender, MouseButtonEventArgs e)
        {
            _formatoSeleccionado = "PDF";
            ActualizarSeleccionFormato();
        }

        private void CardExcel_Click(object sender, MouseButtonEventArgs e)
        {
            _formatoSeleccionado = "Excel";
            ActualizarSeleccionFormato();
        }

        private void BtnExportar_Click(object sender, RoutedEventArgs e)
        {
            ExportarClick?.Invoke(this, new ExportarConfigEventArgs
            {
                Formato = _formatoSeleccionado,
                IncluirGraficos = ChkIncluirGraficos.IsChecked == true
            });
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            CerrarClick?.Invoke(this, EventArgs.Empty);
        }
    }
}
