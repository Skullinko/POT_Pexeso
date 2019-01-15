using System.ComponentModel;
using System.Runtime.CompilerServices;
using Pexeso.ClientGUI.Annotations;

namespace Pexeso.ClientGUI
{
    public class ListItem : INotifyPropertyChanged
    {
        public string Nick { get; set; }

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged(nameof(Playing));
            }
        }

        public string Playing => IsPlaying ? " is playing" : "";

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
