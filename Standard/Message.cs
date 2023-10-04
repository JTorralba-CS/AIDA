using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Standard
{
    public class Message : INotifyPropertyChanged
    {
        public DateTime Time { get; set; }

        private String _User;
        public String User
        {
            get { return _User; }
            set
            {
                _User = value;
                OnPropertyChanged();
            }
        }

        private String _Content;
        public String Content
        {
            get { return _Content; }
            set
            {
                _Content = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] String PropertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }
    }
}
