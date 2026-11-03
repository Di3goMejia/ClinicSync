using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClinicSyncDesktop.Models;

namespace ClinicSyncDesktop.Controls
{
    public partial class TratamientoFormularioControl : UserControl
    {
        private Tratamiento? _tratamiento;
        private List<Tratamiento> _tratamientosExistentes = new List<Tratamiento>();
        private bool _esNuevo = false;

        public event EventHandler<Tratamiento>? Guardado;
        public event EventHandler? Cerrado;
        public event EventHandler<string>? AvisoRequerido;
        public event EventHandler<string>? ConfirmarDescarte;

        public bool HuboError { get; private set; } = false;

        public TratamientoFormularioControl()
        {
            InitializeComponent();
        }

        public void CargarTratamiento(Tratamiento t, string titulo, IEnumerable<Tratamiento>? existentes = null)
        {
            _tratamiento = t;
            _esNuevo = titulo.Contains("Nuevo", StringComparison.OrdinalIgnoreCase);
            _tratamientosExistentes = existentes != null ? new List<Tratamiento>(existentes) : new List<Tratamiento>();

            TxtTitulo.Text = titulo;
            TxtNombre.Text = t.Nombre ?? "";
            TxtPrecio.Text = t.Precio.HasValue ? t.Precio.Value.ToString("0.00") : "0.00";
            ChkActivo.IsChecked = t.Activo;
            ActualizarTextoEstado();
            HuboError = false;

            TxtNombre.Focus();
            TxtNombre.SelectAll();
        }

        private void ActualizarTextoEstado()
        {
            if (TxtEstado == null) return;
            TxtEstado.Text = (ChkActivo.IsChecked == true) ? "Habilitado" : "Deshabilitado";
        }

        private void ChkActivo_Cambio(object sender, RoutedEventArgs e)
        {
            ActualizarTextoEstado();
        }

        public void ForzarCierre()
        {
            Cerrado?.Invoke(this, EventArgs.Empty);
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = TxtNombre.Text?.Trim() ?? "";
            string precioTexto = TxtPrecio.Text?.Trim() ?? "";

            bool tieneContenido = !string.IsNullOrWhiteSpace(nombre) || !string.IsNullOrWhiteSpace(precioTexto);

            if (tieneContenido)
            {
                string mensaje = _esNuevo
                    ? "¿Seguro que desea cancelar el\nagregar un nuevo tratamiento?"
                    : "¿Seguro que desea\ndescartar esta edición?";

                if (ConfirmarDescarte != null)
                {
                    ConfirmarDescarte.Invoke(this, mensaje);
                    return;
                }
            }

            HuboError = false;
            Cerrado?.Invoke(this, EventArgs.Empty);
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            string nombre = TxtNombre.Text?.Trim() ?? "";
            string precioTexto = TxtPrecio.Text?.Trim() ?? "";

            // Validación: campos vacíos
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(precioTexto))
            {
                AvisoRequerido?.Invoke(this, "Se deben llenar los\ncampos obligatorios.");
                if (string.IsNullOrWhiteSpace(nombre))
                    TxtNombre.Focus();
                else
                    TxtPrecio.Focus();
                return;
            }

            // Validación: precio decimal >= 0
            if (!decimal.TryParse(precioTexto, out decimal precio) || precio < 0)
            {
                AvisoRequerido?.Invoke(this, "Se deben llenar los\ncampos obligatorios.");
                TxtPrecio.Focus();
                return;
            }

            // Validación: nombre no repetido
            int idActual = _tratamiento?.Id ?? 0;
            bool nombreRepetido = _tratamientosExistentes.Any(existente =>
                !ReferenceEquals(existente, _tratamiento) &&
                existente.Id != idActual &&
                string.Equals(existente.Nombre?.Trim(), nombre, StringComparison.OrdinalIgnoreCase));

            if (nombreRepetido)
            {
                AvisoRequerido?.Invoke(this, "Ya existe un tratamiento\ncon este nombre.");
                TxtNombre.Focus();
                TxtNombre.SelectAll();
                return;
            }

            if (_tratamiento == null)
            {
                _tratamiento = new Tratamiento();
            }

            _tratamiento.Nombre = nombre;
            _tratamiento.Precio = precio;
            _tratamiento.Activo = ChkActivo.IsChecked == true;

            HuboError = false;
            Guardado?.Invoke(this, _tratamiento);
        }
    }
}
