
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClinicSyncDesktop.Models;

namespace ClinicSyncDesktop.Views.Notificaciones;

public static class NotificacionesVisuales
{
    private static readonly Brush Azul =
        new SolidColorBrush(
            Color.FromRgb(23, 71, 90));

    private static readonly Brush Gris =
        new SolidColorBrush(
            Color.FromRgb(216, 220, 216));

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

        // ICONO
        grid.Children.Add(new TextBlock
        {
            Text = n.Tipo == "CITA" ? "▣" : "▤",
            FontSize = 23,
            Foreground = Azul,
            VerticalAlignment = VerticalAlignment.Center
        });

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
                Content = "×",
                ToolTip = "Archivar",
                Foreground = Azul,
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Cursor =
                    System.Windows.Input.Cursors.Hand,
                FontSize = 17
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
}

