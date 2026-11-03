using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ClinicSyncDesktop.Models
{
    public class Paciente : INotifyPropertyChanged
    {
        private int _id;
        private string _nombre = "";
        private string _telefono = "";
        private string _correo = "";
        private bool _activo = true;
        private string _ultimaCita = "";

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

        public string Telefono
        {
            get => _telefono;
            set { _telefono = value; OnPropertyChanged(); }
        }

        public string Correo
        {
            get => _correo;
            set { _correo = value; OnPropertyChanged(); }
        }

        public bool Activo
        {
            get => _activo;
            set { _activo = value; OnPropertyChanged(); }
        }

        public string UltimaCita
        {
            get => _ultimaCita;
            set { _ultimaCita = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

namespace ClinicSyncDesktop
{
    // Alias de compatibilidad hacia atrás
    public class Paciente : Models.Paciente { }
}
