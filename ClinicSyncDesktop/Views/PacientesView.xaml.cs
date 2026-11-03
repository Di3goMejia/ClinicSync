using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ClinicSyncDesktop.Models;

namespace ClinicSyncDesktop.Views
{
    public partial class PacientesView : UserControl
    {
        private List<Paciente> pacientes = new List<Paciente>();

        public PacientesView()
        {
            InitializeComponent();

            // Datos de ejemplo para pacientes
            pacientes.Add(new Paciente { Id = 1, Nombre = "María Fernanda Gómez", Telefono = "+504 9876-5432", Correo = "maria.gomez@clinicsync.hn", Activo = true, UltimaCita = "2026-10-15" });
            pacientes.Add(new Paciente { Id = 2, Nombre = "Carlos Eduardo Rivera", Telefono = "+504 3321-9876", Correo = "carlos.rivera@clinicsync.hn", Activo = true, UltimaCita = "2026-10-22" });
            pacientes.Add(new Paciente { Id = 3, Nombre = "Ana Lucía Martínez", Telefono = "+504 8765-4321", Correo = "ana.martinez@clinicsync.hn", Activo = false, UltimaCita = "2026-09-10" });

            TablaPacientes.CargarDatos(pacientes);
        }

        private void Tabla_AgregarNuevoClick(object? sender, EventArgs e)
        {
            CtrlAviso.Titulo = "INFORMACIÓN";
            CtrlAviso.Mensaje = "El registro de nuevos pacientes\nestará disponible en el siguiente módulo.";
            OverlayModal.Visibility = Visibility.Visible;
            CtrlAviso.Visibility = Visibility.Visible;
        }

        private void Tabla_EditarClick(object? sender, Paciente seleccionado)
        {
            CtrlAviso.Titulo = "INFORMACIÓN";
            CtrlAviso.Mensaje = $"Edición del paciente:\n{seleccionado.Nombre}";
            OverlayModal.Visibility = Visibility.Visible;
            CtrlAviso.Visibility = Visibility.Visible;
        }

        private void Modal_CerrarClick(object? sender, EventArgs e)
        {
            OverlayModal.Visibility = Visibility.Collapsed;
            CtrlExito.Visibility = Visibility.Collapsed;
            CtrlAviso.Visibility = Visibility.Collapsed;
        }
    }
}
