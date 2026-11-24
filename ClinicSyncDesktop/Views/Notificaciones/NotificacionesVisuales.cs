using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ClinicSyncDesktop.Models;

namespace ClinicSyncDesktop.Views.Notificaciones;

/// <summary>
/// 
/// </summary>
public static class NotificacionesVisuales
{
    /// <summary>
    /// The azul
    /// </summary>
    private static readonly Brush Azul =
        new SolidColorBrush(
            Color.FromRgb(23, 71, 90));

    /// <summary>
    /// The gris
    /// </summary>
    private static readonly Brush Gris =
        new SolidColorBrush(
            Color.FromRgb(216, 220, 216));

    /// <summary>
    /// Crears the tarjeta.
    /// </summary>
    /// <param name="n">The n.</param>
    /// <param name="leer">The leer.</param>
    /// <param name="archivar">The archivar.</param>
    /// <param name="grande">if set to <c>true</c> [grande].</param>
    /// <returns></returns>
    public static Border CrearTarjeta(
        Notificacion n,
        Func<Task> leer,
        Func<Task>? archivar,
        bool grande = false)
    {
        var tarjeta = new Border
        {
            Background = n.Leida
                ? new SolidColorBrush(
                    Color.FromRgb(237, 240, 237))
                : Gris,

            CornerRadius = new CornerRadius(10),
            Padding = new Thickness(9),
            Margin = new Thickness(3, 4, 3, 5)
        };

        var grid = new Grid();

        grid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = new GridLength(35)
            });

        grid.ColumnDefinitions.Add(
            new ColumnDefinition());

        grid.ColumnDefinitions.Add(
            new ColumnDefinition
            {
                Width = GridLength.Auto
            });

        string iconoTipo = n.Tipo == "CITA" ? "cita-proxima.png" : "stock-bajo.png";
        var icono = CrearIcono(iconoTipo, 24);
        icono.HorizontalAlignment = HorizontalAlignment.Left;
        grid.Children.Add(icono);

        // CONTENIDO
        var info = new StackPanel();

        Grid.SetColumn(info, 1);
        grid.Children.Add(info);

        info.Children.Add(new TextBlock
        {
            Text = n.Titulo,
            Foreground = Azul,
            FontSize = grande ? 13 : 11,
            FontWeight = n.Leida
                ? FontWeights.Normal
                : FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap
        });

        if (grande)
        {
            info.Children.Add(new TextBlock
            {
                Text = n.Descripcion,
                Foreground = Brushes.Gray,
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 4, 0, 0)
            });
        }

        info.Children.Add(new TextBlock
        {
            Text = n.FechaTexto,
            Foreground = Brushes.Gray,
            FontSize = 10,
            Margin = new Thickness(0, 4, 0, 0)
        });

        // ACCIONES
        var botones = new StackPanel
        {
            Orientation = Orientation.Vertical
        };

        Grid.SetColumn(botones, 2);
        grid.Children.Add(botones);

        if (!n.Leida)
        {
            var punto = new Button
            {
                Content = "●",
                ToolTip = "Marcar como leída",
                Foreground = new SolidColorBrush(
                    Color.FromRgb(39, 164, 174)),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor =
                    System.Windows.Input.Cursors.Hand,
                FontSize = 17
            };

            punto.Click += async (_, _) =>
            {
                punto.IsEnabled = false;

                try
                {
                    await leer();
                }
                finally
                {
                    punto.IsEnabled = true;
                }
            };

            botones.Children.Add(punto);
        }

        if (archivar is not null && grande)
        {
            var btn = new Button
            {
                Content = CrearIcono("archivar-notificacion.png", 16),
                ToolTip = "Archivar",
                Width = 26,
                Height = 24,
                Padding = new Thickness(0),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor =
                    System.Windows.Input.Cursors.Hand
            };

            btn.Click += async (_, _) =>
            {
                btn.IsEnabled = false;

                try
                {
                    await archivar();
                }
                finally
                {
                    btn.IsEnabled = true;
                }
            };

            botones.Children.Add(btn);
        }

        tarjeta.Child = grid;
        return tarjeta;
    }

    /// <summary>
    /// Crears the icono.
    /// </summary>
    /// <param name="archivo">The archivo.</param>
    /// <param name="tamano">The tamano.</param>
    /// <returns></returns>
    private static Image CrearIcono(string archivo, int tamano)
    {
        var ruta = new Uri(
            $"pack://application:,,,/Resources/ImagenesNotificaciones/{archivo}",
            UriKind.Absolute);

        var imagen = new BitmapImage();
        imagen.BeginInit();
        imagen.UriSource = ruta;
        imagen.DecodePixelWidth = tamano * 2;
        imagen.EndInit();

        return new Image
        {
            Source = imagen,
            Width = tamano,
            Height = tamano,
            Stretch = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Center
        };
    }
}
