
using ClinicSyncDesktop.Services;
using ClinicSyncDesktop.Views;
using ClinicSyncDesktop.Data;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ClinicSync.Views
{
    public partial class MenuPrincipal : Window
    {
        private bool cargandoPerfil;
        private bool cerrandoSesion;
        private bool abriendoModulo;

        public MenuPrincipal()
        {
            InitializeComponent();

            NombrePerfil.Text = "";
            RolPerfil.Text = "";

            AjustarModulos(false);

            Loaded += MenuPrincipal_Loaded;
        }

        // CONSULTAR PERFIL Y PERMISOS EN LA API
        private async void MenuPrincipal_Loaded(
            object sender, RoutedEventArgs e)
        {
            Loaded -= MenuPrincipal_Loaded;

            try
            {
                await CargarPerfilAsync();
            }
            catch (Exception ex)
            {
                if (!cerrandoSesion)
                {
                    AjustarModulos(false);

                    MessageBox.Show(
                        ex.Message,
                        "No se pudo cargar el perfil",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
        }

        private async Task CargarPerfilAsync()
        {
            PerfilActual perfil = await ClienteApi.ObtenerPerfilAsync();

            if (cerrandoSesion)
                return;

            NombrePerfil.Text = perfil.NombreUsuario;

            string rol = perfil.Rol.Trim();

            RolPerfil.Text = rol.ToLowerInvariant() switch
            {
                "administrador" => "Administrador",
                "empleado" => "Empleado",
                "empleado sin finanzas" => "Empleado",
                "empleado-sin-finanzas" => "Empleado",
                "empleado_sin_finanzas" => "Empleado",
                _ => rol
            };

            AjustarModulos(perfil.TieneFinanzas);
        }

        // ACOMODAR LOS BOTONES DEL MENÚ
        private void AjustarModulos(bool tieneFinanzas)
        {
            BtnFinanzas.Visibility = tieneFinanzas
                ? Visibility.Visible
                : Visibility.Collapsed;

            BtnInventario.HorizontalAlignment =
                HorizontalAlignment.Center;

            BtnAgenda.HorizontalAlignment =
                HorizontalAlignment.Center;

            if (tieneFinanzas)
            {
                Grid.SetColumn(BtnInventario, 0);
                Grid.SetColumnSpan(BtnInventario, 1);

                Grid.SetColumn(BtnAgenda, 2);
                Grid.SetColumnSpan(BtnAgenda, 1);

                BtnInventario.Margin =
                    new Thickness(53, 67, 54, -27);

                BtnAgenda.Margin =
                    new Thickness(54, 67, 53, -27);
            }
            else
            {
                Grid.SetColumn(BtnInventario, 0);
                Grid.SetColumnSpan(BtnInventario, 2);

                Grid.SetColumn(BtnAgenda, 1);
                Grid.SetColumnSpan(BtnAgenda, 2);

                BtnInventario.Margin =
                    new Thickness(15, 67, 15, -27);

                BtnAgenda.Margin =
                    new Thickness(15, 67, 15, -27);
            }
        }

        private bool EsModuloFinanzas(string modulo)
        {
            string nombre = modulo.Trim().ToUpperInvariant();

            return nombre.Contains("FINANZAS") ||
                   nombre.Contains("INGRESOS") ||
                   nombre.Contains("EGRESOS") ||
                   nombre.Contains("GRAFICOS") ||
                   nombre.Contains("GRÁFICOS");
        }

        private void Imagen_ImageFailed(
            object sender, ExceptionRoutedEventArgs e)
        {
            MessageBox.Show(
                e.ErrorException.Message,
                "Error al cargar imagen",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private async void BotonModulo_Click(
            object sender, RoutedEventArgs e)
        {
            if (sender is Button boton && boton.Tag != null)
                await AbrirModuloAsync(boton.Tag.ToString()!);
        }

        private async Task AbrirModuloAsync(string modulo)
        {
            if (cerrandoSesion || abriendoModulo ||
                string.IsNullOrWhiteSpace(modulo))
            {
                return;
            }

            abriendoModulo = true;

            try
            {
                CerrarPaneles();

                if (EsModuloFinanzas(modulo))
                {
                    var perfil = await ClienteApi.ObtenerPerfilAsync();

                    if (cerrandoSesion)
                        return;

                    AjustarModulos(perfil.TieneFinanzas);

                    if (!perfil.TieneFinanzas)
                    {
                        MessageBox.Show(
                            "No tienes permiso para acceder a Finanzas.",
                            "Acceso denegado",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }
                }

                if (modulo == "Inicio")
                {
                    MostrarInicio();
                    return;
                }

                // Si la pestaña ya existe, volver a seleccionarla.
                foreach (TabItem pestana in Pestanas.Items)
                {
                    if (pestana.Tag?.ToString() == modulo)
                    {
                        Pestanas.SelectedItem = pestana;
                        MostrarModulo(modulo);
                        return;
                    }
                }

                // Crear el contenido correspondiente al módulo.
                FrameworkElement contenido;


                if (modulo == "Tratamientos")
                {
                    var tratamientosService = new TratamientosApiService();
                    var tratamientosView = new TratamientosView(tratamientosService);

                    tratamientosView.VolverAlMenuSolicitado += (s, e) =>
                    {
                        MostrarInicio();
                    };

                    contenido = tratamientosView;
                }
                else
                {
                    // Contenido provisional para los demás módulos.
                    StackPanel panelGenerico = new StackPanel
                    {
                        Margin = new Thickness(30)
                    };

                    panelGenerico.Children.Add(new TextBlock
                    {
                        Text = "Módulo de " + modulo,
                        FontSize = 25,
                        FontWeight = FontWeights.Bold
                    });

                    Button regresarGenerico = new Button
                    {
                        Content = "Volver al menú principal",
                        Width = 220,
                        Height = 40,
                        Margin = new Thickness(0, 20, 0, 0),
                        HorizontalAlignment =
                            HorizontalAlignment.Left
                    };

                    regresarGenerico.Click +=
                        (s, e) => MostrarInicio();

                    panelGenerico.Children.Add(regresarGenerico);

                    contenido = panelGenerico;
                }

                // Crear la pestaña del módulo.
                TabItem nuevaPestana = new TabItem
                {
                    Header = modulo,
                    Tag = modulo,
                    Content = contenido
                };

                Pestanas.Items.Add(nuevaPestana);
                Pestanas.SelectedItem = nuevaPestana;

                MostrarModulo(modulo);
            }
            catch (Exception ex)
            {
                if (!cerrandoSesion)
                {
                    MessageBox.Show(
                        ex.Message,
                        "No se pudo abrir el módulo",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            finally
            {
                abriendoModulo = false;
            }
        }

        private void MostrarModulo(string modulo)
        {
            VistaInicio.Visibility = Visibility.Collapsed;
            Pestanas.Visibility = Visibility.Visible;
            EncabezadoPrincipal.Visibility = Visibility.Collapsed;

            TituloPantalla.Text = modulo;

            CerrarPaneles();
        }

        private void MostrarInicio()
        {
            VistaInicio.Visibility = Visibility.Visible;
            Pestanas.Visibility = Visibility.Collapsed;
            EncabezadoPrincipal.Visibility = Visibility.Visible;

            TituloPantalla.Text = "Menú Principal";

            CerrarPaneles();
        }

        private void Pestanas_SelectionChanged(
            object sender, SelectionChangedEventArgs e)
        {
            if (e.Source != Pestanas)
                return;

            if (Pestanas.SelectedItem is TabItem pestana &&
                pestana.Tag != null)
            {
                MostrarModulo(pestana.Tag.ToString()!);
            }
        }

        private void Campana_Click(
            object sender, RoutedEventArgs e)
        {
            bool abierto =
                PanelNotificaciones.Visibility == Visibility.Visible;

            CerrarPaneles();

            PanelNotificaciones.Visibility = abierto
                ? Visibility.Collapsed
                : Visibility.Visible;
        }

        private async void PerfilMenu_Click(
            object sender, RoutedEventArgs e)
        {
            if (cargandoPerfil || cerrandoSesion)
                return;

            bool abierto =
                PanelPerfil.Visibility == Visibility.Visible;

            CerrarPaneles();

            if (abierto)
                return;

            cargandoPerfil = true;

            try
            {
                await CargarPerfilAsync();

                if (!cerrandoSesion)
                {
                    CerrarPaneles();
                    PanelPerfil.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                if (!cerrandoSesion)
                {
                    MessageBox.Show(
                        ex.Message,
                        "No se pudo cargar el perfil",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            finally
            {
                cargandoPerfil = false;
            }
        }

        private void CerrarPaneles()
        {
            PanelPerfil.Visibility = Visibility.Collapsed;
            PanelNotificaciones.Visibility = Visibility.Collapsed;
        }

        private async void AjustesPerfil_Click(
            object sender, RoutedEventArgs e)
        {
            await AbrirModuloAsync("Perfil");
        }

        private void MarcarLeidas_Click(
            object sender, RoutedEventArgs e)
        {
            PuntoNoLeida.Visibility = Visibility.Collapsed;

            MessageBox.Show(
                "Las notificaciones se marcaron como leídas.",
                "ClinicSync");
        }

        private async void VerTodas_Click(
            object sender, RoutedEventArgs e)
        {
            await AbrirModuloAsync("Notificaciones");
        }

        private async void CerrarSesion_Click(
            object sender, RoutedEventArgs e)
        {
            if (cerrandoSesion)
                return;

            var respuesta = MessageBox.Show(
                "¿Deseas cerrar tu sesión?",
                "ClinicSync",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (respuesta != MessageBoxResult.Yes)
                return;

            cerrandoSesion = true;
            IsEnabled = false;

            CerrarPaneles();

            try
            {
                await ClienteApi.CerrarSesionAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Se cerró la sesión local, pero no se pudo confirmar " +
                    "el cierre en el servidor:\n\n" + ex.Message,
                    "ClinicSync",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }

            ClienteApi.LimpiarSesion();

            Login login = new Login();

            Application.Current.MainWindow = login;
            login.Show();

            Close();
        }
    }
}
