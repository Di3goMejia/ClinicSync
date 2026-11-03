using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClinicSyncDesktop.Models;

namespace ClinicSyncDesktop.Controls.Finanzas
{
    public partial class ModalMaterialesControl : UserControl
    {
        private List<MaterialInventario> _todos = new List<MaterialInventario>();

        public event EventHandler<string>? Confirmado;
        public event EventHandler? Cerrado;

        public string Titulo
        {
            get => TxtTituloModalMateriales.Text;
            set => TxtTituloModalMateriales.Text = value;
        }

        public ModalMaterialesControl()
        {
            InitializeComponent();

            // Materiales iniciales (Fiel réplica de la Imagen 2)
            _todos.Add(new MaterialInventario
            {
                Id = 1,
                Nombre = "Towel Up! Monoart®",
                Cantidad = 2,
                Seleccionado = true
            });

            _todos.Add(new MaterialInventario
            {
                Id = 2,
                Nombre = "Adhesivo dental PermaFlo®",
                Cantidad = 1,
                Seleccionado = false
            });

            _todos.Add(new MaterialInventario
            {
                Id = 3,
                Nombre = "Anestesia Dental 2% Lidocaína",
                Cantidad = 1,
                Seleccionado = false
            });

            FiltrarLista();
        }

        public void Resetear(string? materialSeleccionado = null)
        {
            if (TxtBuscarMaterial != null)
            {
                TxtBuscarMaterial.Text = "";
            }

            if (!string.IsNullOrWhiteSpace(materialSeleccionado) && materialSeleccionado != "Seleccionar" && materialSeleccionado != "Seleccione un material")
            {
                foreach (var m in _todos)
                {
                    m.Seleccionado = materialSeleccionado.Contains(m.Nombre, StringComparison.OrdinalIgnoreCase);
                }
            }

            FiltrarLista();
        }

        private void FiltrarLista()
        {
            if (ListaMateriales == null) return;

            string texto = TxtBuscarMaterial?.Text?.Trim().ToLower() ?? "";
            if (string.IsNullOrEmpty(texto))
            {
                ListaMateriales.ItemsSource = null;
                ListaMateriales.ItemsSource = _todos;
            }
            else
            {
                ListaMateriales.ItemsSource = null;
                ListaMateriales.ItemsSource = _todos.Where(m => m.Nombre.ToLower().Contains(texto)).ToList();
            }
        }

        private void TxtBuscarMaterial_TextChanged(object sender, TextChangedEventArgs e)
        {
            FiltrarLista();
        }

        private void BtnMenos_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is MaterialInventario material)
            {
                if (material.Cantidad > 1)
                {
                    material.Cantidad--;
                }
            }
        }

        private void BtnMas_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is MaterialInventario material)
            {
                material.Cantidad++;
                material.Seleccionado = true;
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            var seleccionados = _todos.Where(m => m.Seleccionado).ToList();
            string resumen = seleccionados.Count > 0
                ? string.Join(", ", seleccionados.Select(m => m.Nombre))
                : "Seleccione un material";

            Confirmado?.Invoke(this, resumen);
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Cerrado?.Invoke(this, EventArgs.Empty);
        }
    }
}
