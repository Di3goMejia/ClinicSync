using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace ClinicSyncDesktop.Models
{
    public class MaterialInventario : INotifyPropertyChanged
    {
        private int _id;
        private string _nombre = "";
        private int _cantidad = 1;
        private bool _seleccionado;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(); }
        }

        public int Cantidad
        {
            get => _cantidad;
            set
            {
                _cantidad = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CantidadDisplay));
            }
        }

        public bool Seleccionado
        {
            get => _seleccionado;
            set
            {
                _seleccionado = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ControlesCantidadVisibilidad));
            }
        }

        public Visibility ControlesCantidadVisibilidad => Seleccionado ? Visibility.Visible : Visibility.Collapsed;

        public string CantidadDisplay => $"{Cantidad} c.";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
