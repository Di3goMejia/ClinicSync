using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClinicSyncDesktop.Models;

namespace ClinicSyncDesktop.Controls
{
    public partial class TratamientosTablaControl : UserControl
    {
        private List<Tratamiento> _todos = new List<Tratamiento>();

        public event EventHandler? AgregarNuevoClick;
        public event EventHandler<Tratamiento>? EditarClick;

        public TratamientosTablaControl()
        {
            InitializeComponent();
        }

        public void CargarDatos(IEnumerable<Tratamiento> lista)
        {
            _todos = lista != null ? new List<Tratamiento>(lista) : new List<Tratamiento>();
            MostrarLista();
        }

        public void Refrescar(IEnumerable<Tratamiento> lista)
        {
            _todos = lista != null ? new List<Tratamiento>(lista) : new List<Tratamiento>();
            MostrarLista();
        }

        public void MostrarLista()
        {
            if (ListaTratamientos == null) return;

            string texto = TxtBuscar?.Text?.Trim().ToLower() ?? "";
            if (string.IsNullOrEmpty(texto))
            {
                ListaTratamientos.ItemsSource = null;
                ListaTratamientos.ItemsSource = _todos;
            }
            else
            {
                ListaTratamientos.ItemsSource = null;
                ListaTratamientos.ItemsSource = _todos.Where(t => t.Nombre.ToLower().Contains(texto)).ToList();
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
            if (sender is Button boton && boton.DataContext is Tratamiento seleccionado)
            {
                EditarClick?.Invoke(this, seleccionado);
            }
        }
    }
}
