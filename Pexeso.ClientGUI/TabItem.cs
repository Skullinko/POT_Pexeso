using System.ComponentModel;
using System.Runtime.CompilerServices;
using Pexeso.ClientGUI.Annotations;

namespace Pexeso.ClientGUI
{
    public sealed class TabItem : INotifyPropertyChanged
    {
        public string Header { get; set; }

        private string _content;
        
        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}