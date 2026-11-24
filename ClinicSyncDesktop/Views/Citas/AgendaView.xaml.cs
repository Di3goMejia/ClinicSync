using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClinicSyncDesktop.Models;
using ClinicSyncDesktop.Services;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
using ClinicSyncDesktop.Views.Citas;

namespace ClinicSyncDesktop.Views
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="System.Windows.Controls.UserControl" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class AgendaView : UserControl
    {
        /// <summary>
        /// The agenda service
        /// </summary>
        private readonly AgendaApiService agendaService = new();

        // Cantidad de próximas citas que se muestran (Ver más la aumenta).
        /// <summary>
        /// The cantidad proximas
        /// </summary>
        private int cantidadProximas = 4;
        /// <summary>
        /// 
        /// </summary>
        private enum VistaCalendario { Dia, Semana, Mes }

        /// <summary>
        /// The vista actual
        /// </summary>
        private VistaCalendario vistaActual = VistaCalendario.Mes;

        // Fecha que se está mostrando (el día, o algún día de la semana/mes).
        /// <summary>
        /// The fecha referencia
        /// </summary>
        private DateTime fechaReferencia = DateTime.Today;

        /// <summary>
        /// The es
        /// </summary>
        private static readonly CultureInfo Es = new("es-ES");

        // Citas del rango que se está mostrando.
        /// <summary>
        /// The eventos
        /// </summary>
        private List<EventoAgenda> eventos = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="AgendaView"/> class.
        /// </summary>
        public AgendaView()
        {
            InitializeComponent();
            ActualizarEncabezado();
            Loaded += AgendaView_Loaded;
        }

        // MOSTRAR NOTIFICACIONES
        /// <summary>
        /// Mostrars the notificacion.
        /// </summary>
        /// <param name="mensaje">The mensaje.</param>
        /// <param name="exitoso">if set to <c>true</c> [exitoso].</param>
        private void MostrarNotificacion(string mensaje, bool exitoso)
        {
            AvisoNotificacion aviso =
                new AvisoNotificacion(mensaje, exitoso);

            PanelAvisos.Visibility = Visibility.Visible;

            PanelAvisos.Children.Add(aviso);
        }

        /// <summary>
        /// Handles the Loaded event of the AgendaView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void AgendaView_Loaded(
         object sender,
         RoutedEventArgs e)
        {
            Loaded -= AgendaView_Loaded;
            await CargarProximasAsync();
            await CargarEventosAsync();
        }

        // CARGAR PRÓXIMAS CITAS
        /// <summary>
        /// Cargars the proximas asynchronous.
        /// </summary>
        private async Task CargarProximasAsync()
        {
            try
            {
                List<ProximaCita> citas =
                    await agendaService.ProximasAsync(cantidadProximas);

                listaProximas.ItemsSource = citas;
            }
            catch (Exception ex)
            {
                MostrarNotificacion("No fue posible cargar las próximas citas.", false);
            }
        }

        // + AGREGAR NUEVA CITA
        /// <summary>
        /// Handles the Click event of the btnAgregarCita control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void btnAgregarCita_Click(
            object sender,
            RoutedEventArgs e)
        {
            var ventana = new AgregarCitaWindow
            {
                Owner = Window.GetWindow(this)
            };

            if (ventana.ShowDialog() == true)
            {
                await CargarProximasAsync();
                await CargarEventosAsync();

                MostrarNotificacion("La cita se agregó con éxito.", true);
            }
        }

        // PONER EL TÍTULO Y RESALTAR EL BOTÓN ACTIVO
        /// <summary>
        /// Actualizars the encabezado.
        /// </summary>
        private void ActualizarEncabezado()
        {
            switch (vistaActual)
            {
                case VistaCalendario.Mes:
                    txtTituloCalendario.Text = Mayuscula(
                        fechaReferencia.ToString("MMMM yyyy", Es));
                    break;

                case VistaCalendario.Semana:
                    int dif = ((int)fechaReferencia.DayOfWeek + 6) % 7;
                    DateTime lunes = fechaReferencia.AddDays(-dif);
                    DateTime domingo = lunes.AddDays(6);

                    txtTituloCalendario.Text = Mayuscula(
                        $"{lunes:dd MMM} - {domingo:dd MMM yyyy}"
                            .Replace(lunes.ToString("dd MMM"),
                                     lunes.ToString("dd MMM", Es))
                            .Replace(domingo.ToString("dd MMM yyyy"),
                                     domingo.ToString("dd MMM yyyy", Es)));
                    break;

                default:
                    txtTituloCalendario.Text = Mayuscula(
                        fechaReferencia.ToString("dddd d MMMM yyyy", Es));
                    break;
            }

            Resaltar(btnVistaDia, vistaActual == VistaCalendario.Dia);
            Resaltar(btnVistaSemana, vistaActual == VistaCalendario.Semana);
            Resaltar(btnVistaMes, vistaActual == VistaCalendario.Mes);
            
        }

        /// <summary>
        /// Mayusculas the specified texto.
        /// </summary>
        /// <param name="texto">The texto.</param>
        /// <returns></returns>
        private static string Mayuscula(string texto) =>
            string.IsNullOrEmpty(texto)
                ? texto
                : char.ToUpper(texto[0], Es) + texto.Substring(1);

        /// <summary>
        /// Resaltars the specified boton.
        /// </summary>
        /// <param name="boton">The boton.</param>
        /// <param name="activo">if set to <c>true</c> [activo].</param>
        private static void Resaltar(Button boton, bool activo)
        {
            boton.Background = activo
                ? new SolidColorBrush(Color.FromRgb(19, 141, 170))
                : new SolidColorBrush(Color.FromRgb(146, 212, 222));

            boton.Foreground = activo
                ? Brushes.White
                : new SolidColorBrush(Color.FromRgb(23, 71, 90));
        }

        // CAMBIAR ENTRE DÍA / SEMANA / MES
        /// <summary>
        /// Handles the Click event of the Vista control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Vista_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button boton)
                return;

            vistaActual = (boton.Tag as string) switch
            {
                "Dia" => VistaCalendario.Dia,
                "Semana" => VistaCalendario.Semana,
                _ => VistaCalendario.Mes
            };

            ActualizarEncabezado();
            await CargarEventosAsync();
        }

        // ANTERIOR / SIGUIENTE
        /// <summary>
        /// Handles the Click event of the Anterior control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Anterior_Click(object sender, RoutedEventArgs e) =>
            await Mover(-1);

        /// <summary>
        /// Handles the Click event of the Siguiente control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void Siguiente_Click(object sender, RoutedEventArgs e) =>
                await Mover(1);

        /// <summary>
        /// Movers the specified paso.
        /// </summary>
        /// <param name="paso">The paso.</param>
        private async Task Mover(int paso)
            {
                fechaReferencia = vistaActual switch
                {
                    VistaCalendario.Dia => fechaReferencia.AddDays(paso),
                    VistaCalendario.Semana => fechaReferencia.AddDays(7 * paso),
                    _ => fechaReferencia.AddMonths(paso)
                };

                ActualizarEncabezado();
                await CargarEventosAsync();
            }

        // DIBUJAR EL CALENDARIO SEGÚN LA VISTA
        /// <summary>
        /// Dibujars the calendario.
        /// </summary>
        private void DibujarCalendario()
        {
            areaCalendario.ClipToBounds = true;

            switch (vistaActual)
            {
                case VistaCalendario.Mes:
                    DibujarMes();
                    break;

                case VistaCalendario.Semana:
                    DibujarSemana();
                    break;

                default:
                    DibujarDia();
                    break;
            }
        }

        // CUADRÍCULA DEL MES (semana de lunes a domingo)
        /// <summary>
        /// Dibujars the mes.
        /// </summary>
        private void DibujarMes()
        {
            DateTime primero = new DateTime(
                fechaReferencia.Year, fechaReferencia.Month, 1);

            int desfase = ((int)primero.DayOfWeek + 6) % 7;
            DateTime inicio = primero.AddDays(-desfase);

            int diasDelMes = DateTime.DaysInMonth(
                primero.Year, primero.Month);

            int semanas = (int)Math.Ceiling((desfase + diasDelMes) / 7.0);

            var cuadricula = new Grid();

            for (int c = 0; c < 7; c++)
            {
                cuadricula.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
            }

            cuadricula.RowDefinitions.Add(new RowDefinition
            {
                Height = GridLength.Auto
            });

            for (int s = 0; s < semanas; s++)
            {
                cuadricula.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(1, GridUnitType.Star)
                });
            }

            // NOMBRES DE LOS DÍAS
            string[] nombres = { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };

            for (int c = 0; c < 7; c++)
            {
                var encabezado = new TextBlock
                {
                    Text = nombres[c],
                    Foreground = new SolidColorBrush(Color.FromRgb(23, 71, 90)),
                    FontWeight = FontWeights.Bold,
                    FontSize = 13,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 8, 0, 8)
                };

                Grid.SetRow(encabezado, 0);
                Grid.SetColumn(encabezado, c);
                cuadricula.Children.Add(encabezado);
            }

            // CELDAS DE LOS DÍAS
            for (int s = 0; s < semanas; s++)
            {
                for (int c = 0; c < 7; c++)
                {
                    DateTime dia = inicio.AddDays(s * 7 + c);
                    bool delMes = dia.Month == primero.Month;
                    bool esHoy = dia.Date == DateTime.Today;

                    var celda = new Border
                    {
                        BorderBrush = new SolidColorBrush(
                            Color.FromRgb(227, 230, 226)),
                        BorderThickness = new Thickness(0.5),
                        Background = delMes
                            ? Brushes.White
                            : new SolidColorBrush(Color.FromRgb(247, 248, 246)),
                        Tag = dia.Date
                    };

                    var numero = new TextBlock
                    {
                        Text = dia.Day.ToString(),
                        FontSize = 12,
                        FontWeight = FontWeights.SemiBold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = esHoy
                            ? Brushes.White
                            : new SolidColorBrush(delMes
                                ? Color.FromRgb(23, 71, 90)
                                : Color.FromRgb(174, 176, 177))
                    };

                    var marcaNumero = new Border
                    {
                        Width = 22,
                        Height = 22,
                        CornerRadius = new CornerRadius(11),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Top,
                        Margin = new Thickness(5, 4, 0, 0),
                        Background = esHoy
                            ? new SolidColorBrush(Color.FromRgb(19, 141, 170))
                            : Brushes.Transparent,
                        Child = numero
                    };

                    // Número del día + lista de citas de ese día.
                    var contenido = new Grid();
                    contenido.RowDefinitions.Add(new RowDefinition
                    {
                        Height = GridLength.Auto
                    });
                    contenido.RowDefinitions.Add(new RowDefinition
                    {
                        Height = new GridLength(1, GridUnitType.Star)
                    });

                    Grid.SetRow(marcaNumero, 0);
                    contenido.Children.Add(marcaNumero);

                    var pila = new StackPanel
                    {
                        Margin = new Thickness(3, 0, 3, 3),
                        ClipToBounds = true
                    };

                    var delDia = EventosDelDia(dia);
                    int maximo = 3;

                    foreach (var ev in delDia.Take(maximo))
                        pila.Children.Add(CrearBarraCita(ev));

                    if (delDia.Count > maximo)
                    {
                        pila.Children.Add(new TextBlock
                        {
                            Text = $"+{delDia.Count - maximo} más",
                            FontSize = 10,
                            Foreground = new SolidColorBrush(
                                Color.FromRgb(23, 71, 90)),
                            Margin = new Thickness(3, 1, 0, 0)
                        });
                    }

                    Grid.SetRow(pila, 1);
                    contenido.Children.Add(pila);

                    celda.Child = contenido;

                    Grid.SetRow(celda, s + 1);
                    Grid.SetColumn(celda, c);
                    cuadricula.Children.Add(celda);
                }
            }

            areaCalendario.Child = cuadricula;
        }

        // VISTA SEMANA (7 columnas, de lunes a domingo)
        /// <summary>
        /// Dibujars the semana.
        /// </summary>
        private void DibujarSemana()
        {
            int dif = ((int)fechaReferencia.DayOfWeek + 6) % 7;
            DateTime lunes = fechaReferencia.Date.AddDays(-dif);

            var cuadricula = new Grid();

            for (int c = 0; c < 7; c++)
            {
                cuadricula.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(1, GridUnitType.Star)
                });
            }

            cuadricula.RowDefinitions.Add(new RowDefinition
            {
                Height = GridLength.Auto
            });
            cuadricula.RowDefinitions.Add(new RowDefinition
            {
                Height = new GridLength(1, GridUnitType.Star)
            });

            string[] nombres = { "Lun", "Mar", "Mié", "Jue", "Vie", "Sáb", "Dom" };

            for (int c = 0; c < 7; c++)
            {
                DateTime dia = lunes.AddDays(c);
                bool esHoy = dia == DateTime.Today;

                // ENCABEZADO: nombre del día + número
                var encabezado = new StackPanel
                {
                    Margin = new Thickness(0, 8, 0, 8),
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                encabezado.Children.Add(new TextBlock
                {
                    Text = nombres[c],
                    Foreground = new SolidColorBrush(Color.FromRgb(23, 71, 90)),
                    FontWeight = FontWeights.Bold,
                    FontSize = 13,
                    HorizontalAlignment = HorizontalAlignment.Center
                });

                encabezado.Children.Add(new Border
                {
                    Width = 26,
                    Height = 26,
                    CornerRadius = new CornerRadius(13),
                    Margin = new Thickness(0, 4, 0, 0),
                    Background = esHoy
                        ? new SolidColorBrush(Color.FromRgb(19, 141, 170))
                        : Brushes.Transparent,
                    Child = new TextBlock
                    {
                        Text = dia.Day.ToString(),
                        FontSize = 13,
                        FontWeight = FontWeights.SemiBold,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = esHoy
                            ? Brushes.White
                            : new SolidColorBrush(Color.FromRgb(23, 71, 90))
                    }
                });

                Grid.SetRow(encabezado, 0);
                Grid.SetColumn(encabezado, c);
                cuadricula.Children.Add(encabezado);

                // COLUMNA CON LAS CITAS DEL DÍA
                var pila = new StackPanel
                {
                    Margin = new Thickness(3)
                };

                foreach (var ev in EventosDelDia(dia))
                    pila.Children.Add(CrearBarraCita(ev));

                var columna = new Border
                {
                    BorderBrush = new SolidColorBrush(Color.FromRgb(227, 230, 226)),
                    BorderThickness = new Thickness(0.5),
                    Background = Brushes.White,
                    Tag = dia,
                    Child = new ScrollViewer
                    {
                        VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                        Content = pila
                    }
                };

                Grid.SetRow(columna, 1);
                Grid.SetColumn(columna, c);
                cuadricula.Children.Add(columna);
            }

            areaCalendario.Child = cuadricula;
        }

        // VISTA DÍA (un solo día con sus citas)
        /// <summary>
        /// Dibujars the dia.
        /// </summary>
        private void DibujarDia()
        {
            DateTime dia = fechaReferencia.Date;
            var delDia = EventosDelDia(dia);

            var pila = new StackPanel
            {
                Margin = new Thickness(16)
            };

            if (delDia.Count == 0)
            {
                pila.Children.Add(new TextBlock
                {
                    Text = "No hay citas para este día.",
                    FontSize = 14,
                    Foreground = new SolidColorBrush(Color.FromRgb(23, 71, 90)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 40, 0, 0)
                });
            }
            else
            {
                foreach (var ev in delDia)
                    pila.Children.Add(CrearFilaDia(ev));
            }

            areaCalendario.Child = new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(8),
                Child = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = pila
                }
            };
        }

        // FILA GRANDE DE UNA CITA PARA LA VISTA DÍA
        /// <summary>
        /// Crears the fila dia.
        /// </summary>
        /// <param name="ev">The ev.</param>
        /// <returns></returns>
        private Border CrearFilaDia(EventoAgenda ev)
        {
            Color color = Color.FromRgb(50, 165, 171);

            if (!string.IsNullOrWhiteSpace(ev.Color))
            {
                try
                {
                    color = (Color)ColorConverter.ConvertFromString(ev.Color);
                }
                catch (FormatException)
                {
                    // Si el color no es válido, se usa el turquesa.
                }
            }

            string horario;

            if (ev.TodoElDia)
                horario = "Todo el día";
            else if (ev.FechaFinal.Date == ev.FechaInicio.Date)
                horario = $"{ev.FechaInicio:HH:mm} - {ev.FechaFinal:HH:mm}";
            else
                horario = $"{ev.FechaInicio:HH:mm} - {ev.FechaFinal:dd/MM HH:mm}";

            var textos = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0)
            };

            textos.Children.Add(new TextBlock
            {
                Text = ev.Titulo,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(23, 71, 90)),
                TextTrimming = TextTrimming.CharacterEllipsis
            });

            textos.Children.Add(new TextBlock
            {
                Text = horario,
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(23, 71, 90)),
                Margin = new Thickness(0, 2, 0, 0)
            });

            var contenido = new DockPanel();

            var franja = new Border
            {
                Width = 6,
                CornerRadius = new CornerRadius(3),
                Background = new SolidColorBrush(color)
            };

            DockPanel.SetDock(franja, Dock.Left);
            contenido.Children.Add(franja);
            contenido.Children.Add(textos);

            var fila = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(244, 246, 243)),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(10),
                Margin = new Thickness(0, 0, 0, 8),
                Cursor = System.Windows.Input.Cursors.Hand,
                Child = contenido
            };

            fila.MouseLeftButtonUp += (s, e) =>
                Evento_Click(ev, fila);

            return fila;
        }


        // RANGO DE FECHAS QUE NECESITA LA VISTA ACTUAL (hasta = límite exclusivo)
        /// <summary>
        /// Obteners the rango.
        /// </summary>
        /// <returns></returns>
        private (DateTime desde, DateTime hasta) ObtenerRango()
        {
            switch (vistaActual)
            {
                case VistaCalendario.Mes:
                    DateTime primero = new DateTime(
                        fechaReferencia.Year, fechaReferencia.Month, 1);
                    int desfase = ((int)primero.DayOfWeek + 6) % 7;
                    DateTime inicio = primero.AddDays(-desfase);
                    int dias = DateTime.DaysInMonth(primero.Year, primero.Month);
                    int semanas = (int)Math.Ceiling((desfase + dias) / 7.0);
                    return (inicio, inicio.AddDays(semanas * 7));

                case VistaCalendario.Semana:
                    int dif = ((int)fechaReferencia.DayOfWeek + 6) % 7;
                    DateTime lunes = fechaReferencia.Date.AddDays(-dif);
                    return (lunes, lunes.AddDays(7));

                default:
                    return (fechaReferencia.Date, fechaReferencia.Date.AddDays(1));
            }
        }

        // PEDIR LAS CITAS A LA API Y REDIBUJAR
        /// <summary>
        /// Cargars the eventos asynchronous.
        /// </summary>
        private async Task CargarEventosAsync()
        {
            try
            {
                var (desde, hasta) = ObtenerRango();
                eventos = await agendaService.ListarAsync(desde, hasta);
                DibujarCalendario();
            }
            catch (Exception ex)
            {
                MostrarNotificacion("No fue posible cargar el calendario.", false);
            }
        }

        // MENÚ DE OPCIONES DE UNA CITA
        // MENÚ PERSONALIZADO DE OPCIONES DE UNA CITA
        /// <summary>
        /// Eventoes the click.
        /// </summary>
        /// <param name="evento">The evento.</param>
        /// <param name="elemento">The elemento.</param>
        private void Evento_Click(
            EventoAgenda evento,
            FrameworkElement elemento)
        {
            // CONTENEDOR PRINCIPAL
            Border contenedor = new Border
            {
                Background = new SolidColorBrush(
                    Color.FromArgb(235, 184, 213, 212)),
                CornerRadius = new CornerRadius(5),
                Padding = new Thickness(10),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    BlurRadius = 12,
                    ShadowDepth = 3,
                    Opacity = 0.25
                }
            };

            // OPCIONES HORIZONTALES
            StackPanel opciones = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            // BOTÓN EDITAR
            Button btnEditar = new Button
            {
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
            {
                new TextBlock
                {
                    Text = "✎",
                    FontSize = 17,
                    Margin = new Thickness(0, 0, 7, 0)
                },
                new TextBlock
                {
                    Text = "Editar",
                    FontSize = 14,
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
                },
                Background = new SolidColorBrush(
                    Color.FromRgb(230, 239, 237)),
                Foreground = new SolidColorBrush(
                    Color.FromRgb(23, 71, 90)),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(7, 3, 7, 3),
                Cursor = System.Windows.Input.Cursors.Hand,
                Margin = new Thickness(0, 0, 8, 0)
            };

            // BOTÓN CANCELAR
            Button btnCancelar = new Button
            {
                Content = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Children =
            {
                new TextBlock
                {
                    Text = "✕",
                    FontSize = 18,
                    Margin = new Thickness(0, 0, 7, 0)
                },
                new TextBlock
                {
                    Text = "Cancelar",
                    FontSize = 14,
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
                },
                Background = new SolidColorBrush(
                    Color.FromRgb(230, 239, 237)),
                Foreground = new SolidColorBrush(
                    Color.FromRgb(23, 71, 90)),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(7, 3, 7, 3),
                Cursor = System.Windows.Input.Cursors.Hand
            };

            opciones.Children.Add(btnEditar);
            opciones.Children.Add(btnCancelar);

            contenedor.Child = opciones;

            // POPUP PERSONALIZADO
            Popup menu = new Popup
            {
                Child = contenedor,
                PlacementTarget = elemento,
                Placement = PlacementMode.Left,
                HorizontalOffset = -5,
                VerticalOffset = 0,
                AllowsTransparency = true,
                StaysOpen = false
            };

            // ACCIÓN EDITAR
            btnEditar.Click += async (s, e) =>
            {
                menu.IsOpen = false;

                ModificarCitaWindow ventana =
                    new ModificarCitaWindow(evento.IdEvento)
                    {
                        Owner = Window.GetWindow(this)
                    };

                if (ventana.ShowDialog() == true)
                {
                    await CargarProximasAsync();
                    await CargarEventosAsync();

                    MostrarNotificacion(
                        "La cita se modificó con éxito.",
                        true);
                }
            };

            // ACCIÓN CANCELAR
            btnCancelar.Click += async (s, e) =>
            {
                menu.IsOpen = false;

                ConfirmarDescarteWindow confirmacion =
                    new ConfirmarDescarteWindow(
                        "¿Seguro que desea cancelar esta cita?")
                    {
                        Owner = Window.GetWindow(this),
                        Title = "CANCELAR CITA"
                    };

                if (confirmacion.ShowDialog() != true)
                    return;

                try
                {
                    await agendaService.CancelarAsync(evento.IdEvento);

                    await CargarProximasAsync();
                    await CargarEventosAsync();

                    MostrarNotificacion(
                        "La cita se canceló con éxito.",
                        true);
                }
                catch (Exception)
                {
                    MostrarNotificacion(
                        "No fue posible cancelar la cita.",
                        false);
                }
            };

            menu.IsOpen = true;
        }

        // CITAS QUE CAEN EN UN DÍA (incluye las de varios días)
        /// <summary>
        /// Eventoses the delete dia.
        /// </summary>
        /// <param name="dia">The dia.</param>
        /// <returns></returns>
        private List<EventoAgenda> EventosDelDia(DateTime dia)
        {
            DateTime ini = dia.Date;
            DateTime fin = ini.AddDays(1);

            return eventos
                .Where(ev => ev.FechaInicio < fin &&
                             ev.FechaFinal >= ini &&
                             !(ev.FechaFinal == ini && ev.FechaFinal > ev.FechaInicio))
                .OrderBy(ev => ev.FechaInicio)
                .ToList();
        }

        // BARRITA DE COLOR DE UNA CITA
        /// <summary>
        /// Crears the barra cita.
        /// </summary>
        /// <param name="ev">The ev.</param>
        /// <returns></returns>
        private Border CrearBarraCita(EventoAgenda ev)
        {
            Color color = Color.FromRgb(50, 165, 171);

            if (!string.IsNullOrWhiteSpace(ev.Color))
            {
                try
                {
                    color = (Color)ColorConverter.ConvertFromString(ev.Color);
                }
                catch (FormatException)
                {
                    // Si el color no es válido, se usa el turquesa.
                }
            }

            string texto = ev.TodoElDia
                ? ev.Titulo
                : $"{ev.FechaInicio:HH:mm} {ev.Titulo}";

            var barra = new Border
            {
                Background = new SolidColorBrush(color),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(4, 1, 4, 1),
                Margin = new Thickness(0, 0, 0, 2),
                Cursor = System.Windows.Input.Cursors.Hand,
                ToolTip = ev.Titulo,
                Child = new TextBlock
                {
                    Text = texto,
                    FontSize = 10,
                    Foreground = Brushes.White,
                    TextTrimming = TextTrimming.CharacterEllipsis
                }
            };

            barra.MouseLeftButtonUp += (s, e) =>
                Evento_Click(ev, barra);

            return barra;
        }
        // VER MÁS...
        /// <summary>
        /// Handles the Click event of the btnVerMas control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private async void btnVerMas_Click(
            object sender,
            RoutedEventArgs e)
        {
            cantidadProximas += 4;
            await CargarProximasAsync();
        }
    }
}

