using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ClinicSyncDesktop.Controls.Finanzas;
using ClinicSyncDesktop.Models;

namespace ClinicSyncDesktop.Views
{
    public partial class FinanzasView : UserControl
    {
        private readonly FinanzasResumenTablaControl _vistaResumen;
        private readonly FinanzasIngresosTablaControl _vistaIngresos;
        private readonly FinanzasEgresosTablaControl _vistaEgresos;
        private readonly FinanzasGraficosControl _vistaGraficos;

        private readonly List<TransaccionFinanciera> _listaIngresos = new List<TransaccionFinanciera>();
        private readonly List<TransaccionFinanciera> _listaEgresos = new List<TransaccionFinanciera>();
        private readonly List<TransaccionFinanciera> _listaResumen = new List<TransaccionFinanciera>();

        private string _tabActivo = "Resumen";
        private string _filtroGlobal = "Hoy";
        private bool _sincronizandoFiltro = false;

        public FinanzasView()
        {
            InitializeComponent();

            _vistaResumen = new FinanzasResumenTablaControl();
            _vistaIngresos = new FinanzasIngresosTablaControl();
            _vistaEgresos = new FinanzasEgresosTablaControl();
            _vistaGraficos = new FinanzasGraficosControl();

            CargarDatosSemilla();

            _vistaIngresos.CargarDatos(_listaIngresos);
            _vistaEgresos.CargarDatos(_listaEgresos);
            _vistaResumen.CargarDatos(_listaResumen);
            _vistaGraficos.ActualizarDatos(_listaIngresos, _listaEgresos);

            // Sincronización bidireccional de los filtros (Hoy, Semana, Mes, Rango) entre las 4 subvistas
            _vistaResumen.FiltroCambiado += (s, f) => SincronizarFiltroGlobal(f);
            _vistaIngresos.FiltroCambiado += (s, f) => SincronizarFiltroGlobal(f);
            _vistaEgresos.FiltroCambiado += (s, f) => SincronizarFiltroGlobal(f);
            _vistaGraficos.FiltroCambiado += (s, f) => SincronizarFiltroGlobal(f);

            // Conectar eventos de la vista de gráficos
            _vistaGraficos.ExportarClick += (s, e) => MostrarModal(CtrlExportar);

            // Conectar eventos del botón AGREGAR NUEVO de la tabla de ingresos
            _vistaIngresos.AgregarNuevoClick += (s, e) =>
            {
                CtrlAgregarIngreso.Limpiar();
                MostrarSoloModal(CtrlAgregarIngreso);
            };

            // Conectar eventos del botón AGREGAR NUEVO de la tabla de egresos
            _vistaEgresos.AgregarNuevoClick += (s, e) =>
            {
                CtrlAgregarEgreso.Limpiar();
                MostrarSoloModal(CtrlAgregarEgreso);
            };

            // Conectar evento de EDITAR de la tabla de ingresos
            _vistaIngresos.EditarClick += (s, item) =>
            {
                CtrlEditarIngreso.Cargar(item);
                MostrarSoloModal(CtrlEditarIngreso);
            };

            // Conectar evento de EDITAR de la tabla de egresos
            _vistaEgresos.EditarClick += (s, item) =>
            {
                CtrlEditarEgreso.Cargar(item);
                MostrarSoloModal(CtrlEditarEgreso);
            };

            // Conectar evento de EDITAR desde la tabla Resumen
            _vistaResumen.EditarClick += (s, item) =>
            {
                if (item.EsIngreso)
                {
                    var ingreso = _listaIngresos.Find(i => i.Id == item.Id) ?? item;
                    CtrlEditarIngreso.Cargar(ingreso);
                    MostrarSoloModal(CtrlEditarIngreso);
                }
                else
                {
                    var egreso = _listaEgresos.Find(e => e.Id == item.Id) ?? item;
                    CtrlEditarEgreso.Cargar(egreso);
                    MostrarSoloModal(CtrlEditarEgreso);
                }
            };

            // Conectar simulación de error al guardar/agregar con Shift
            CtrlAgregarIngreso.ErrorSimuladoClick += (s, e) => MostrarPopupErrorIngreso();
            CtrlAgregarEgreso.ErrorSimuladoClick += (s, e) => MostrarPopupErrorEgreso();
            CtrlEditarEgreso.ErrorSimuladoClick += (s, e) => MostrarPopupErrorEdicion();
            CtrlEditarIngreso.ErrorSimuladoClick += (s, e) => MostrarPopupErrorEdicion();

            // Iniciar por defecto en la pestaña Resumen con filtro Hoy
            SincronizarFiltroGlobal("Hoy");
            CambiarPestaña("Resumen");
        }

        private void CargarDatosSemilla()
        {
            // ================== INGRESOS ==================
            // 1. Hoy (2026/06/19)
            _listaIngresos.Add(new TransaccionFinanciera
            {
                Id = 1,
                Fecha = "2026/06/19",
                Paciente = "Karem N. Sánchez",
                Tratamiento = "Endodoncia",
                Concepto = "Endodoncia",
                MedioPago = "Transferencia",
                Descuento = 0m,
                Total = 1500m,
                EsIngreso = true,
                Activo = true
            });

            _listaIngresos.Add(new TransaccionFinanciera
            {
                Id = 2,
                Fecha = "2026/06/19",
                Paciente = "Diego J. Mejía",
                Tratamiento = "Tartrectomías",
                Concepto = "Tartrectomías",
                MedioPago = "Efectivo",
                Descuento = 0m,
                Total = 800m,
                EsIngreso = true,
                Activo = true
            });

            // 2. Esta Semana (días anteriores de la misma semana)
            _listaIngresos.Add(new TransaccionFinanciera
            {
                Id = 3,
                Fecha = "2026/06/18",
                Paciente = "María Fernanda Gómez",
                Tratamiento = "Blanqueamiento Dental",
                Concepto = "Blanqueamiento Dental",
                MedioPago = "Tarjeta",
                Descuento = 0m,
                Total = 2200m,
                EsIngreso = true,
                Activo = true
            });

            _listaIngresos.Add(new TransaccionFinanciera
            {
                Id = 4,
                Fecha = "2026/06/16",
                Paciente = "Carlos Eduardo Rivera",
                Tratamiento = "Consulta y Diagnóstico",
                Concepto = "Consulta y Diagnóstico",
                MedioPago = "Efectivo",
                Descuento = 0m,
                Total = 500m,
                EsIngreso = true,
                Activo = true
            });

            // 3. Este Mes (días anteriores del mes de Junio)
            _listaIngresos.Add(new TransaccionFinanciera
            {
                Id = 5,
                Fecha = "2026/06/12",
                Paciente = "Ana Lucía Martínez",
                Tratamiento = "Ortodoncia Control",
                Concepto = "Ortodoncia Control",
                MedioPago = "Transferencia",
                Descuento = 0m,
                Total = 1800m,
                EsIngreso = true,
                Activo = true
            });

            _listaIngresos.Add(new TransaccionFinanciera
            {
                Id = 6,
                Fecha = "2026/06/08",
                Paciente = "Jorge Alberto Paz",
                Tratamiento = "Implantología y Prótesis",
                Concepto = "Implantología y Prótesis",
                MedioPago = "Tarjeta",
                Descuento = 0m,
                Total = 4500m,
                EsIngreso = true,
                Activo = true
            });

            _listaIngresos.Add(new TransaccionFinanciera
            {
                Id = 7,
                Fecha = "2026/06/03",
                Paciente = "Sofía Castro",
                Tratamiento = "Endodoncia Molar",
                Concepto = "Endodoncia Molar",
                MedioPago = "Transferencia",
                Descuento = 0m,
                Total = 1600m,
                EsIngreso = true,
                Activo = true
            });

            // 4. Mes Anterior (Mayo 2026 - fuera del filtro Este Mes)
            _listaIngresos.Add(new TransaccionFinanciera
            {
                Id = 8,
                Fecha = "2026/05/25",
                Paciente = "Camila Rivera",
                Tratamiento = "Profilaxis Infantil",
                Concepto = "Profilaxis Infantil",
                MedioPago = "Efectivo",
                Descuento = 0m,
                Total = 650m,
                EsIngreso = true,
                Activo = true
            });

            // ================== EGRESOS ==================
            // 1. Hoy (2026/06/19)
            _listaEgresos.Add(new TransaccionFinanciera
            {
                Id = 1,
                Fecha = "2026/06/19",
                Tipo = "Materiales",
                Concepto = "Towel Up! Monoart®",
                MedioPago = "Transferencia",
                Total = 450m,
                EsIngreso = false,
                Activo = true
            });

            _listaEgresos.Add(new TransaccionFinanciera
            {
                Id = 2,
                Fecha = "2026/06/19",
                Tipo = "Materiales",
                Concepto = "Compra Adhesivo dental",
                MedioPago = "Transferencia",
                Total = 380m,
                EsIngreso = false,
                Activo = true
            });

            // 2. Esta Semana (días anteriores de la misma semana)
            _listaEgresos.Add(new TransaccionFinanciera
            {
                Id = 3,
                Fecha = "2026/06/17",
                Tipo = "Materiales",
                Concepto = "Anestesia Dental 2% Lidocaína",
                MedioPago = "Efectivo",
                Total = 420m,
                EsIngreso = false,
                Activo = true
            });

            _listaEgresos.Add(new TransaccionFinanciera
            {
                Id = 4,
                Fecha = "2026/06/15",
                Tipo = "Servicios",
                Concepto = "Reparación pieza de mano",
                MedioPago = "Efectivo",
                Total = 300m,
                EsIngreso = false,
                Activo = true
            });

            // 3. Este Mes (días anteriores del mes de Junio)
            _listaEgresos.Add(new TransaccionFinanciera
            {
                Id = 5,
                Fecha = "2026/06/10",
                Tipo = "Servicios",
                Concepto = "Pago de luz clínica",
                MedioPago = "Efectivo",
                Total = 620m,
                EsIngreso = false,
                Activo = true
            });

            _listaEgresos.Add(new TransaccionFinanciera
            {
                Id = 6,
                Fecha = "2026/06/05",
                Tipo = "Servicios",
                Concepto = "Internet y telefonía clínica",
                MedioPago = "Transferencia",
                Total = 550m,
                EsIngreso = false,
                Activo = true
            });

            _listaEgresos.Add(new TransaccionFinanciera
            {
                Id = 7,
                Fecha = "2026/06/01",
                Tipo = "Materiales",
                Concepto = "Guantes y mascarillas quirúrgicas",
                MedioPago = "Transferencia",
                Total = 750m,
                EsIngreso = false,
                Activo = true
            });

            // 4. Mes Anterior (Mayo 2026)
            _listaEgresos.Add(new TransaccionFinanciera
            {
                Id = 8,
                Fecha = "2026/05/20",
                Tipo = "Servicios",
                Concepto = "Mantenimiento autoclave",
                MedioPago = "Cheque",
                Total = 850m,
                EsIngreso = false,
                Activo = true
            });

            // ================== RESUMEN SINCRONIZADO ==================
            // Agregar todos los ingresos y egresos al libro de resumen ordenados cronológicamente
            foreach (var ing in _listaIngresos)
            {
                _listaResumen.Add(new TransaccionFinanciera
                {
                    Id = ing.Id,
                    Fecha = ing.Fecha,
                    Concepto = ing.Tratamiento,
                    MedioPago = ing.MedioPago,
                    Total = ing.Total,
                    EsIngreso = true,
                    Activo = ing.Activo
                });
            }

            foreach (var egr in _listaEgresos)
            {
                _listaResumen.Add(new TransaccionFinanciera
                {
                    Id = egr.Id,
                    Fecha = egr.Fecha,
                    Concepto = egr.Concepto,
                    MedioPago = egr.MedioPago,
                    Total = egr.Total,
                    EsIngreso = false,
                    Activo = egr.Activo
                });
            }

            _listaResumen.Sort((a, b) => string.Compare(b.Fecha, a.Fecha, StringComparison.Ordinal));
        }

        private void SincronizarFiltroGlobal(string nuevoFiltro)
        {
            if (_sincronizandoFiltro) return;
            _sincronizandoFiltro = true;

            try
            {
                _filtroGlobal = nuevoFiltro;

                if (_vistaResumen.FiltroActual != nuevoFiltro)
                    _vistaResumen.CambiarFiltro(nuevoFiltro);

                if (_vistaIngresos.FiltroActual != nuevoFiltro)
                    _vistaIngresos.CambiarFiltro(nuevoFiltro);

                if (_vistaEgresos.FiltroActual != nuevoFiltro)
                    _vistaEgresos.CambiarFiltro(nuevoFiltro);

                if (_vistaGraficos.FiltroActual != nuevoFiltro)
                    _vistaGraficos.CambiarFiltro(nuevoFiltro);
            }
            finally
            {
                _sincronizandoFiltro = false;
            }
        }

        private void Tab_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                if (border == TabResumen) CambiarPestaña("Resumen");
                else if (border == TabIngresos) CambiarPestaña("Ingresos");
                else if (border == TabEgresos) CambiarPestaña("Egresos");
                else if (border == TabGraficos) CambiarPestaña("Graficos");
            }
        }

        public void CambiarPestaña(string nombrePestaña)
        {
            _tabActivo = nombrePestaña;

            var brushSeleccionado = (SolidColorBrush)FindResource("ColorMenuSeleccionado");
            var brushInactivo = new SolidColorBrush(Color.FromRgb(0xF0, 0xF1, 0xEE));

            // Actualizar estilo visual de las pestañas
            TabResumen.Background = nombrePestaña == "Resumen" ? brushSeleccionado : brushInactivo;
            TxtTabResumen.FontWeight = nombrePestaña == "Resumen" ? FontWeights.Bold : FontWeights.SemiBold;

            TabIngresos.Background = nombrePestaña == "Ingresos" ? brushSeleccionado : brushInactivo;
            TxtTabIngresos.FontWeight = nombrePestaña == "Ingresos" ? FontWeights.Bold : FontWeights.SemiBold;

            TabEgresos.Background = nombrePestaña == "Egresos" ? brushSeleccionado : brushInactivo;
            TxtTabEgresos.FontWeight = nombrePestaña == "Egresos" ? FontWeights.Bold : FontWeights.SemiBold;

            TabGraficos.Background = nombrePestaña == "Graficos" ? brushSeleccionado : brushInactivo;
            TxtTabGraficos.FontWeight = nombrePestaña == "Graficos" ? FontWeights.Bold : FontWeights.SemiBold;

            // Ocultar botón superior en Gráficos porque Gráficos ya tiene su botón de Exportar integrado
            if (BtnExportarSuperior != null)
            {
                BtnExportarSuperior.Visibility = nombrePestaña == "Graficos" ? Visibility.Collapsed : Visibility.Visible;
            }

            // Intercambiar UserControl en el contenedor
            ContenedorSubvista.Content = nombrePestaña switch
            {
                "Resumen" => _vistaResumen,
                "Ingresos" => _vistaIngresos,
                "Egresos" => _vistaEgresos,
                "Graficos" => _vistaGraficos,
                _ => _vistaResumen
            };

            // Asegurar que la vista activa herede y aplique el filtro actual
            switch (nombrePestaña)
            {
                case "Resumen":
                    _vistaResumen.CambiarFiltro(_filtroGlobal);
                    break;
                case "Ingresos":
                    _vistaIngresos.CambiarFiltro(_filtroGlobal);
                    break;
                case "Egresos":
                    _vistaEgresos.CambiarFiltro(_filtroGlobal);
                    break;
                case "Graficos":
                    _vistaGraficos.ActualizarDatos(_listaIngresos, _listaEgresos);
                    _vistaGraficos.CambiarFiltro(_filtroGlobal);
                    break;
            }
        }

        // ============ FLUJO MODAL DE AGREGAR Y EDITAR INGRESO Y EGRESO ============

        private void CtrlAgregarIngreso_AbrirSelectorMateriales(object? sender, EventArgs e)
        {
            CtrlMateriales.Titulo = "Agregar nuevo ingreso:";
            CtrlMateriales.Resetear();
            CtrlMateriales.Visibility = Visibility.Visible;
        }

        private void CtrlAgregarEgreso_AbrirSelectorMateriales(object? sender, EventArgs e)
        {
            CtrlMateriales.Titulo = "Agregar nuevo egreso:";
            CtrlMateriales.Resetear();
            CtrlMateriales.Visibility = Visibility.Visible;
        }

        private void CtrlEditarEgreso_AbrirSelectorMateriales(object? sender, EventArgs e)
        {
            CtrlMateriales.Titulo = "Editar información:";
            CtrlMateriales.Resetear();
            CtrlMateriales.Visibility = Visibility.Visible;
        }

        private void CtrlMateriales_Confirmado(object? sender, string resumen)
        {
            CtrlMateriales.Visibility = Visibility.Collapsed;
            if (CtrlAgregarEgreso.Visibility == Visibility.Visible)
            {
                CtrlAgregarEgreso.SetMaterial(resumen);
            }
            else if (CtrlEditarEgreso.Visibility == Visibility.Visible)
            {
                CtrlEditarEgreso.SetMaterial(resumen);
            }
            else
            {
                CtrlAgregarIngreso.SetMateriales(resumen);
            }
        }

        private void CtrlMateriales_Cerrado(object? sender, EventArgs e)
        {
            CtrlMateriales.Visibility = Visibility.Collapsed;
        }

        // Pop-up AVISO! (Validaciones) - aparece superpuesto sin cerrar el formulario debajo
        private void CtrlFormulario_AvisoRequerido(object? sender, string mensaje)
        {
            CtrlAviso.Titulo = "AVISO!";
            CtrlAviso.Mensaje = mensaje;
            CtrlAviso.Visibility = Visibility.Visible;
        }

        private void ModalAviso_AceptarClick(object? sender, EventArgs e)
        {
            CtrlAviso.Visibility = Visibility.Collapsed;
        }

        // Pop-up CUIDADO! (Confirmación de descarte de cambios)
        private void CtrlFormulario_ConfirmarDescarte(object? sender, string mensaje)
        {
            CtrlCuidado.Titulo = "CUIDADO!";
            CtrlCuidado.Mensaje = mensaje;
            CtrlCuidado.Visibility = Visibility.Visible;
        }

        private void CtrlFormulario_Cerrado(object? sender, EventArgs e)
        {
            OcultarModales();
        }

        private void ModalCuidado_SiClick(object? sender, EventArgs e)
        {
            CtrlCuidado.Visibility = Visibility.Collapsed;
            OcultarModales();
        }

        private void ModalCuidado_NoClick(object? sender, EventArgs e)
        {
            CtrlCuidado.Visibility = Visibility.Collapsed;
        }

        private void CtrlEditarIngreso_ImprimirFacturaClick(object? sender, EventArgs e)
        {
            CtrlExito.Titulo = "ÉXITO!";
            CtrlExito.Mensaje = "Factura enviada\na impresión.";
            MostrarSoloModal(CtrlExito);
        }

        private void CtrlAgregarIngreso_IngresoAgregado(object? sender, TransaccionFinanciera nuevo)
        {
            int nuevoId = (_listaIngresos.Count > 0 ? _listaIngresos.Max(x => x.Id) + 1 : 1);
            nuevo.Id = nuevoId;
            _listaIngresos.Insert(0, nuevo);

            _listaResumen.Insert(0, new TransaccionFinanciera
            {
                Id = nuevo.Id,
                Fecha = nuevo.Fecha,
                Concepto = nuevo.Tratamiento,
                MedioPago = nuevo.MedioPago,
                Total = nuevo.Total,
                EsIngreso = true,
                Activo = nuevo.Activo
            });

            _vistaIngresos.Refrescar(_listaIngresos);
            _vistaResumen.Refrescar(_listaResumen);
            _vistaGraficos.ActualizarDatos(_listaIngresos, _listaEgresos);

            CtrlExito.Titulo = "ÉXITO!";
            CtrlExito.Mensaje = "El ingreso se\nagregó con éxito.";
            MostrarSoloModal(CtrlExito);
        }

        private void CtrlAgregarEgreso_EgresoAgregado(object? sender, TransaccionFinanciera nuevo)
        {
            int nuevoId = (_listaEgresos.Count > 0 ? _listaEgresos.Max(x => x.Id) + 1 : 1);
            nuevo.Id = nuevoId;
            _listaEgresos.Insert(0, nuevo);

            _listaResumen.Insert(0, new TransaccionFinanciera
            {
                Id = nuevo.Id,
                Fecha = nuevo.Fecha,
                Concepto = nuevo.Concepto,
                MedioPago = nuevo.MedioPago,
                Total = nuevo.Total,
                EsIngreso = false,
                Activo = nuevo.Activo
            });

            _vistaEgresos.Refrescar(_listaEgresos);
            _vistaResumen.Refrescar(_listaResumen);
            _vistaGraficos.ActualizarDatos(_listaIngresos, _listaEgresos);

            CtrlExito.Titulo = "ÉXITO!";
            CtrlExito.Mensaje = "El egreso se\nagregó con éxito.";
            MostrarSoloModal(CtrlExito);
        }

        private void CtrlEditarIngreso_GuardarClick(object? sender, TransaccionFinanciera editado)
        {
            var itemIngreso = _listaIngresos.Find(i => i.Id == editado.Id);
            if (itemIngreso != null)
            {
                itemIngreso.Activo = editado.Activo;
                itemIngreso.Paciente = editado.Paciente;
                itemIngreso.Tratamiento = editado.Tratamiento;
                itemIngreso.Concepto = editado.Tratamiento;
                itemIngreso.Fecha = editado.Fecha;
                itemIngreso.Descuento = editado.Descuento;
                itemIngreso.MedioPago = editado.MedioPago;
                itemIngreso.Total = editado.Total;
            }

            var itemResumen = _listaResumen.Find(r => r.Id == editado.Id && r.EsIngreso);
            if (itemResumen != null)
            {
                itemResumen.Concepto = editado.Tratamiento;
                itemResumen.MedioPago = editado.MedioPago;
                itemResumen.Fecha = editado.Fecha;
                itemResumen.Total = editado.Total;
                itemResumen.Activo = editado.Activo;
            }

            _vistaIngresos.Refrescar(_listaIngresos);
            _vistaResumen.Refrescar(_listaResumen);
            _vistaGraficos.ActualizarDatos(_listaIngresos, _listaEgresos);

            CtrlExito.Titulo = "ÉXITO!";
            CtrlExito.Mensaje = "Los cambios se\nguardaron éxitosamente.";
            MostrarSoloModal(CtrlExito);
        }

        private void CtrlEditarEgreso_GuardarClick(object? sender, TransaccionFinanciera editado)
        {
            var itemEgreso = _listaEgresos.Find(e => e.Id == editado.Id);
            if (itemEgreso != null)
            {
                itemEgreso.Activo = editado.Activo;
                itemEgreso.Tipo = editado.Tipo;
                itemEgreso.Concepto = editado.Concepto;
                itemEgreso.Fecha = editado.Fecha;
                itemEgreso.MedioPago = editado.MedioPago;
                itemEgreso.Total = editado.Total;
            }

            var itemResumen = _listaResumen.Find(r => r.Id == editado.Id && !r.EsIngreso);
            if (itemResumen != null)
            {
                itemResumen.Concepto = editado.Concepto;
                itemResumen.MedioPago = editado.MedioPago;
                itemResumen.Fecha = editado.Fecha;
                itemResumen.Total = editado.Total;
                itemResumen.Activo = editado.Activo;
            }

            _vistaEgresos.Refrescar(_listaEgresos);
            _vistaResumen.Refrescar(_listaResumen);
            _vistaGraficos.ActualizarDatos(_listaIngresos, _listaEgresos);

            CtrlExito.Titulo = "ÉXITO!";
            CtrlExito.Mensaje = "Los cambios se\nguardaron éxitosamente.";
            MostrarSoloModal(CtrlExito);
        }

        public void MostrarPopupErrorEgreso()
        {
            CtrlError.Titulo = "ERROR!";
            CtrlError.Mensaje = "El egreso NO\nse agregó.";
            CtrlError.Detalle = null;
            MostrarSoloModal(CtrlError);
        }

        public void MostrarPopupErrorEdicion()
        {
            CtrlError.Titulo = "ERROR!";
            CtrlError.Mensaje = "Los cambios NO se\nguardaron.";
            CtrlError.Detalle = null;
            MostrarSoloModal(CtrlError);
        }

        public void MostrarPopupErrorIngreso()
        {
            CtrlError.Titulo = "ERROR!";
            CtrlError.Mensaje = "El ingreso NO\nse agregó.";
            CtrlError.Detalle = null;
            MostrarSoloModal(CtrlError);
        }

        // ============ FLUJO MODAL DE EXPORTACIÓN ============

        public void BtnAbrirExportar_Click(object sender, RoutedEventArgs e)
        {
            MostrarSoloModal(CtrlExportar);
        }

        private void CtrlExportar_ExportarClick(object? sender, ExportarConfigEventArgs e)
        {
            MostrarSoloModal(CtrlGenerando);
            CtrlGenerando.IniciarGeneracion(simularFallo: false);
        }

        private void CtrlGenerando_GeneracionCompletada(object? sender, bool exitoso)
        {
            if (exitoso)
            {
                CtrlExito.Titulo = "ÉXITO!";
                CtrlExito.Mensaje = "El reporte se\ndescargó con éxito.";
                MostrarSoloModal(CtrlExito);
            }
            else
            {
                CtrlError.Titulo = "ERROR!";
                CtrlError.Mensaje = "El reporte NO se\ngeneró.";
                CtrlError.Detalle = "Error de conexión.";
                MostrarSoloModal(CtrlError);
            }
        }

        private void MostrarModal(FrameworkElement modal)
        {
            MostrarSoloModal(modal);
        }

        private void MostrarSoloModal(FrameworkElement modal)
        {
            OverlayModal.Visibility = Visibility.Visible;
            CtrlExportar.Visibility = Visibility.Collapsed;
            CtrlGenerando.Visibility = Visibility.Collapsed;
            CtrlAgregarIngreso.Visibility = Visibility.Collapsed;
            CtrlEditarIngreso.Visibility = Visibility.Collapsed;
            CtrlAgregarEgreso.Visibility = Visibility.Collapsed;
            CtrlEditarEgreso.Visibility = Visibility.Collapsed;
            CtrlMateriales.Visibility = Visibility.Collapsed;
            CtrlAviso.Visibility = Visibility.Collapsed;
            CtrlCuidado.Visibility = Visibility.Collapsed;
            CtrlExito.Visibility = Visibility.Collapsed;
            CtrlError.Visibility = Visibility.Collapsed;

            modal.Visibility = Visibility.Visible;
        }

        private void Modal_CerrarClick(object? sender, EventArgs e)
        {
            CtrlGenerando.Detener();
            OcultarModales();
        }

        private void OcultarModales()
        {
            OverlayModal.Visibility = Visibility.Collapsed;
            CtrlExportar.Visibility = Visibility.Collapsed;
            CtrlGenerando.Visibility = Visibility.Collapsed;
            CtrlAgregarIngreso.Visibility = Visibility.Collapsed;
            CtrlEditarIngreso.Visibility = Visibility.Collapsed;
            CtrlAgregarEgreso.Visibility = Visibility.Collapsed;
            CtrlEditarEgreso.Visibility = Visibility.Collapsed;
            CtrlMateriales.Visibility = Visibility.Collapsed;
            CtrlAviso.Visibility = Visibility.Collapsed;
            CtrlCuidado.Visibility = Visibility.Collapsed;
            CtrlExito.Visibility = Visibility.Collapsed;
            CtrlError.Visibility = Visibility.Collapsed;
        }
    }
}
