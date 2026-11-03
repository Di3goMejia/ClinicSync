using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ClinicSyncDesktop.Controls.Finanzas
{
    public partial class ModalGenerandoReporteControl : UserControl
    {
        public event EventHandler? CancelarClick;
        public event EventHandler<bool>? GeneracionCompletada;

        private DispatcherTimer? _timer;
        private double _progreso = 0;
        private bool _simularFallo = false;

        public ModalGenerandoReporteControl()
        {
            InitializeComponent();
        }

        public void IniciarGeneracion(bool simularFallo = false)
        {
            _simularFallo = simularFallo;
            _progreso = 0;
            ActualizarBarra(0);

            _timer?.Stop();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(40)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        public void Detener()
        {
            _timer?.Stop();
            _timer = null;
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            _progreso += 2.0;
            ActualizarBarra(_progreso);

            if (_progreso >= 100)
            {
                Detener();
                GeneracionCompletada?.Invoke(this, !_simularFallo);
            }
        }

        private void ActualizarBarra(double porcentaje)
        {
            BarraProgreso.Value = Math.Min(100, Math.Max(0, porcentaje));
            double totalWidth = BarraProgreso.ActualWidth > 0 ? BarraProgreso.ActualWidth : 368;
            ProgresoAnimado.Width = (porcentaje / 100.0) * totalWidth;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Detener();
            CancelarClick?.Invoke(this, EventArgs.Empty);
        }
    }
}
