using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClinicSyncDesktop.Models
{
    public class Tratamiento : INotifyPropertyChanged
    {
        private int _id;
        private string _nombre = "";
        private string? _descripcion;
        private decimal? _precio;
        private bool _activo;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        /// <summary>Alias para Id; usado por los servicios de API.</summary>
        public int IdTratamiento
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); OnPropertyChanged(nameof(Id)); }
        }

        public string? Descripcion
        {
            get => _descripcion;
            set { _descripcion = value; OnPropertyChanged(); }
        }

        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; OnPropertyChanged(); }
        }

        public decimal? Precio
        {
            get => _precio;
            set
            {
                _precio = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PrecioDisplay));
            }
        }

        public bool Activo
        {
            get => _activo;
            set { _activo = value; OnPropertyChanged(); }
        }

        public string PrecioDisplay => Precio.HasValue ? $"L {Precio.Value:N2}" : "";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
