using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClinicSyncDesktop.Models
{
    public class TransaccionFinanciera : INotifyPropertyChanged
    {
        private int _id;
        private string _fecha = "";
        private string _concepto = "";
        private string _tipo = "";
        private string _paciente = "";
        private string _tratamiento = "";
        private string _materiales = "";
        private string _medioPago = "Efectivo";
        private decimal _descuento = 0m;
        private decimal _total = 0m;
        private bool _esIngreso = true;
        private bool _activo = true;

        public string Tipo
        {
            get => _tipo;
            set { _tipo = value; OnPropertyChanged(); }
        }

        public bool Activo
        {
            get => _activo;
            set { _activo = value; OnPropertyChanged(); }
        }

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Fecha
        {
            get => _fecha;
            set { _fecha = value; OnPropertyChanged(); }
        }

        public string Concepto
        {
            get => _concepto;
            set { _concepto = value; OnPropertyChanged(); }
        }

        public string Paciente
        {
            get => _paciente;
            set { _paciente = value; OnPropertyChanged(); }
        }

        public string Tratamiento
        {
            get => _tratamiento;
            set { _tratamiento = value; OnPropertyChanged(); }
        }

        public string Materiales
        {
            get => _materiales;
            set { _materiales = value; OnPropertyChanged(); }
        }

        public string MedioPago
        {
            get => _medioPago;
            set { _medioPago = value; OnPropertyChanged(); }
        }

        public decimal Descuento
        {
            get => _descuento;
            set
            {
                _descuento = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DescuentoDisplay));
            }
        }

        public decimal Total
        {
            get => _total;
            set
            {
                _total = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TotalDisplay));
                OnPropertyChanged(nameof(ColorTotal));
            }
        }

        public bool EsIngreso
        {
            get => _esIngreso;
            set
            {
                _esIngreso = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ColorTotal));
            }
        }

        public string DescuentoDisplay => $"L {Descuento:N2}";
        public string TotalDisplay => $"L {Total:N2}";
        public string ColorTotal => EsIngreso ? "#20A382" : "#D9435A";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
