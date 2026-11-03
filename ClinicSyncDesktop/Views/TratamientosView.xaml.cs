using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;

namespace ClinicSyncDesktop.Views
{
    public partial class TratamientosView : UserControl
    {
        private List<Tratamiento> todos = new List<Tratamiento>();
        private bool _esCreacionNueva = false;

        public TratamientosView() : this(null) { }

        // Constructor que acepta el servicio de API (usado desde MenuLateral)
        public TratamientosView(TratamientosApiService? tratamientosService)
        {
            InitializeComponent();

            // Datos iniciales que replican exactamente la captura del diseño
            todos.Add(new Tratamiento { Id = 1, Nombre = "Endodoncia", Precio = 0m, Activo = true });
            todos.Add(new Tratamiento { Id = 2, Nombre = "Tartrectomías", Precio = 0m, Activo = false });
            todos.Add(new Tratamiento { Id = 3, Nombre = "Implantología y Prótesis", Precio = 0m, Activo = false });

            TablaTratamientos.CargarDatos(todos);
        }

        public IReadOnlyList<Tratamiento> TodosLosTratamientos => todos.AsReadOnly();

        // Clic en AGREGAR NUEVO desde la tabla
        private void Tabla_AgregarNuevoClick(object? sender, EventArgs e)
        {
            _esCreacionNueva = true;

            Tratamiento nuevo = new Tratamiento
            {
                Id = todos.Count + 1,
                Precio = 0m,
                Activo = true
            };

            CtrlFormulario.CargarTratamiento(nuevo, "Nuevo tratamiento:", todos);
            MostrarSoloModal(CtrlFormulario);
        }

        // Clic en el botón del engranaje para editar
        private void Tabla_EditarClick(object? sender, Tratamiento seleccionado)
        {
            if (seleccionado == null) return;
            _esCreacionNueva = false;

            CtrlFormulario.CargarTratamiento(seleccionado, "Editar información:", todos);
            MostrarSoloModal(CtrlFormulario);
        }

        // Se guardó exitosamente desde el formulario
        private void Formulario_Guardado(object? sender, Tratamiento guardado)
        {
            if (_esCreacionNueva && !todos.Contains(guardado))
            {
                todos.Add(guardado);
            }

            TablaTratamientos.Refrescar(todos);
            MostrarPopupExito();
        }

        // Se canceló o cerró el formulario
        private void Formulario_Cerrado(object? sender, EventArgs e)
        {
            OcultarModales();
        }

        // El formulario requiere mostrar aviso de validación
        private void Formulario_AvisoRequerido(object? sender, string mensaje)
        {
            CtrlAviso.Mensaje = mensaje;
            CtrlAviso.Visibility = Visibility.Visible;
            // No ocultamos el formulario para que permanezca visible debajo
        }

        // El formulario solicita confirmación para descartar cambios
        private void Formulario_ConfirmarDescarte(object? sender, string mensaje)
        {
            CtrlCuidado.Mensaje = mensaje;
            CtrlCuidado.Visibility = Visibility.Visible;
            // No ocultamos el formulario para que permanezca visible debajo
        }

        // Confirmar descarte: Sí
        private void ModalCuidado_SiClick(object? sender, EventArgs e)
        {
            CtrlCuidado.Visibility = Visibility.Collapsed;
            CtrlFormulario.ForzarCierre();
            OcultarModales();
        }

        // Confirmar descarte: No
        private void ModalCuidado_NoClick(object? sender, EventArgs e)
        {
            CtrlCuidado.Visibility = Visibility.Collapsed;
        }

        // Aceptar en popup de aviso
        private void ModalAviso_AceptarClick(object? sender, EventArgs e)
        {
            CtrlAviso.Visibility = Visibility.Collapsed;
        }

        // Aceptar / Cerrar en popup de éxito
        private void ModalExito_AceptarClick(object? sender, EventArgs e)
        {
            OcultarModales();
        }

        // Aceptar / Cerrar en popup de error
        private void ModalError_AceptarClick(object? sender, EventArgs e)
        {
            OcultarModales();
        }

        // Muestra el popup de éxito
        public void MostrarPopupExito(string mensaje = "Los cambios se\nguardaron éxitosamente.")
        {
            CtrlExito.Mensaje = mensaje;
            MostrarSoloModal(CtrlExito);
        }

        // Muestra el popup de error
        public void MostrarPopupError(string mensaje = "Los cambios NO se\nguardaron.")
        {
            CtrlError.Mensaje = mensaje;
            MostrarSoloModal(CtrlError);
        }

        private void MostrarSoloModal(FrameworkElement modal)
        {
            OverlayModal.Visibility = Visibility.Visible;
            CtrlFormulario.Visibility = Visibility.Collapsed;
            CtrlExito.Visibility = Visibility.Collapsed;
            CtrlError.Visibility = Visibility.Collapsed;
            CtrlAviso.Visibility = Visibility.Collapsed;
            CtrlCuidado.Visibility = Visibility.Collapsed;

            modal.Visibility = Visibility.Visible;
        }

        private void OcultarModales()
        {
            OverlayModal.Visibility = Visibility.Collapsed;
            CtrlFormulario.Visibility = Visibility.Collapsed;
            CtrlExito.Visibility = Visibility.Collapsed;
            CtrlError.Visibility = Visibility.Collapsed;
            CtrlAviso.Visibility = Visibility.Collapsed;
            CtrlCuidado.Visibility = Visibility.Collapsed;
        }
    }
}
