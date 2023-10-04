using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Standard
{
    public class UniformResourceLocator : INotifyPropertyChanged
    {
        public String Domain { get; set; }

        public String Page { get; set; }

        public void SetDomain(String URL)
        {
            string[] Items = URL.Replace("/", "").Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            Domain = Items[0] + "://" + Items[1] + "/";

            if (Domain == "https://0.0.0.0/")
            {
                Domain = "https://localhost/";
            }
        }

        public void SetPage(String URL)
        {
            if (URL == "")
            {
                Page = "BLAZOR";
            }
            else
            {
                Page = URL;
            }
        }

        private String _URL;
        public String URL
        {
            get { return _URL; }
            set
            {
                _URL = value;
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
