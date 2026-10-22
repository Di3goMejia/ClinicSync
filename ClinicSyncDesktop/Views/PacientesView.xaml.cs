using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClinicSyncDesktop.Views
{
    public record Usuario(
       int IdUsuario, int IdEmpleado, string Identidad, string Nombres, string Apellidos,
       DateOnly FechaNacimiento, char Genero, string Telefono, string? CorreoContacto,
       string? ImagenPerfil, string NombreUsuario, string CorreoLogin, int IdRol,
       string Estado, DateTime FechaRegistro
    );

    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UserControl1 : UserControl
    {

        public UserControl1()
        {
            InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {


        }
    }
}
