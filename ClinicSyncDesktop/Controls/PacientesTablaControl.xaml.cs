using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClinicSyncDesktop.Models;

namespace ClinicSyncDesktop.Controls
{
    public partial class PacientesTablaControl : UserControl
    {
        private List<Paciente> _todos = new List<Paciente>();

        public event EventHandler? AgregarNuevoClick;
        public event EventHandler<Paciente>? EditarClick;

        public PacientesTablaControl()
        {
            InitializeComponent();
        }

        public void CargarDatos(IEnumerable<Paciente> lista)
        {
            _todos = lista != null ? new List<Paciente>(lista) : new List<Paciente>();
            MostrarLista();
        }

        public void Refrescar(IEnumerable<Paciente> lista)
        {
            _todos = lista != null ? new List<Paciente>(lista) : new List<Paciente>();
            MostrarLista();
        }

        public void MostrarLista()
        {
            if (ListaPacientes == null) return;

            string texto = TxtBuscar?.Text?.Trim().ToLower() ?? "";
            if (string.IsNullOrEmpty(texto))
            {
                ListaPacientes.ItemsSource = null;
                ListaPacientes.ItemsSource = _todos;
            }
            else
            {
                ListaPacientes.ItemsSource = null;
                ListaPacientes.ItemsSource = _todos.Where(p =>
                    (p.Nombre?.ToLower().Contains(texto) ?? false) ||
                    (p.Telefono?.ToLower().Contains(texto) ?? false) ||
                    (p.Correo?.ToLower().Contains(texto) ?? false)).ToList();
            }
        }

        private void TxtBuscar_TextChanged(object sender, TextChangedEventArgs e)
        {
            MostrarLista();
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            AgregarNuevoClick?.Invoke(this, EventArgs.Empty);
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button boton && boton.DataContext is Paciente seleccionado)
            {
                EditarClick?.Invoke(this, seleccionado);
            }
        }
    }
}
