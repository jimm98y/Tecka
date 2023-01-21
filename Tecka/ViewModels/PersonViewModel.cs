using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Tecka.ViewModels
{
    public class PersonViewModel : INotifyPropertyChanged
    {
        public int ID 
        { 
            get
            {
                return $"{Name};{Birthday.Date.ToUniversalTime().Ticks}".GetHashCode();
            }
        }

        public string PersonID { get; private set; }

        private string _name;

        public string Name
        {
            get { return _name; }
            set { _name = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name))); }
        }

        private DateTime _birthday;

        public DateTime Birthday
        {
            get { return _birthday; }
            set { _birthday = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Birthday))); }
        }

        private string _birthdayAsString;

        public string BirthdayAsString
        {
            get { return _birthdayAsString; }
            set { _birthdayAsString = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BirthdayAsString))); }
        }

        public PersonViewModel(string personID, string name, DateTime birthday)
        {
            this.PersonID = personID ?? throw new ArgumentNullException(nameof(personID));
            this.Name = name;
            this.Birthday = birthday;
            this.BirthdayAsString = birthday.Date.ToString(System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern);
        }

        public ObservableCollection<CertificateViewModel> Certificates { get; private set; } = new ObservableCollection<CertificateViewModel>();

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
