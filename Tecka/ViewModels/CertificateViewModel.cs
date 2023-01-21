using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Tecka.API;
using Tecka.Utility;

namespace Tecka.ViewModels
{
    public class CertificateDetailItemViewModel : INotifyPropertyChanged
    {
        private string _header;

        public string Header
        {
            get { return _header; }
            set { _header = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Header))); }
        }

        private string _text;

        public string Text
        {
            get { return _text; }
            set { _text = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Text))); }
        }

        public CertificateDetailItemViewModel(string header, string text)
        {
            this.Header = header;
            this.Text = text;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class CertificateViewModel : INotifyPropertyChanged
    {
        private DecodedCertificate _certificate;

        public int ID
        {
            get
            {
                return QrData.GetHashCode();
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

        private string _qrData;

        public string QrData
        {
            get { return _qrData; }
            set { _qrData = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(QrData))); }
        }

        private string _type;

        public string Type
        {
            get { return _type; }
            set { _type = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Type))); }
        }

        private string _typeInfo;

        public string TypeInfo
        {
            get { return _typeInfo; }
            set { _typeInfo = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TypeInfo))); }
        }

        private string _remaining;

        public string Remaining
        {
            get { return _remaining; }
            set { _remaining = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Remaining))); }
        }

        private ObservableCollection<CertificateDetailItemViewModel> _details = null;

        public ObservableCollection<CertificateDetailItemViewModel> Details
        {
            get { return _details; }
            set { _details = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Details))); }
        }

        public async Task LoadAsync(string qrData)
        {
            DecodedCertificate c = await CertificateDecoder.DecodeAsync(qrData);
            this._certificate = c;
            this.QrData = qrData;

            // these are visible from other views as well, do not change the language
            this.Name = $"{c.Certificate.Name.FamilyName} {c.Certificate.Name.GivenName}";
            this.PersonID = $"{c.Certificate.Name.FamilyName};{c.Certificate.Name.GivenName};{c.Certificate.DateOfBirth.ToUniversalTime().Ticks}"; // hopefully, this should be unique for everybody
            this.Birthday = c.Certificate.DateOfBirth;
            this.BirthdayAsString = c.Certificate.DateOfBirth.Date.ToString(CultureInfo.CurrentUICulture.DateTimeFormat.ShortDatePattern);

            if (c.Certificate.Vaccination != null && c.Certificate.Vaccination.Length > 0)
            {
                this.Type = StringsHelper.GetString("TypeVaccination");
                this.TypeInfo =
                    string.Join(", ", c.Certificate.Vaccination.Select(x => DataResolver.GetValues(ValueSetName.VaccineMedicinalProduct, x.VaccineMedicinalProduct)).ToArray());
            }
            else if (c.Certificate.Test != null && c.Certificate.Test.Length > 0)
            {
                this.Type = StringsHelper.GetString("TypeTest");
                this.TypeInfo =
                    string.Join(", ", c.Certificate.Test.Select(x => DataResolver.GetValues(ValueSetName.TestType, x.TypeOfTest)).ToArray());
            }
            else
            {
                throw new NotSupportedException();
            }

            ChangeLanguage(System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
        }

        public void ChangeLanguage(string culture)
        {
            CultureInfo cultureInfo = new CultureInfo(culture);
            DecodedCertificate c = this._certificate;

            var details = new ObservableCollection<CertificateDetailItemViewModel>();

            // build the details view
            details.Add(new CertificateDetailItemViewModel(
                $"{c.Certificate.Name.FamilyName} {c.Certificate.Name.GivenName}\r\n{c.Certificate.Name.FamilyNameTransliterated} {c.Certificate.Name.GivenNameTransliterated}",
                c.Certificate.DateOfBirth.Date.ToString(cultureInfo.DateTimeFormat.ShortDatePattern)));

            details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("DetailCertificateValidSince", culture), c.IssuedAt.ToLocalTime().ToString(cultureInfo.DateTimeFormat.ShortDatePattern)));
            details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("DetailCertificateValidUntil", culture), c.Expiration.ToLocalTime().ToString(cultureInfo.DateTimeFormat.ShortDatePattern)));

            if (c.Certificate.Vaccination != null && c.Certificate.Vaccination.Length > 0)
            {
                foreach (var vaccine in c.Certificate.Vaccination)
                {
                    details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("TypeTest", culture), null));
                    details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("DetailCertificateVaccineProphylaxis", culture), DataResolver.GetValues(ValueSetName.VaccineProphylaxis, vaccine.VaccineOrProphylaxis)));
                    details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("DetailCertificateVaccine", culture), DataResolver.GetValues(ValueSetName.VaccineMedicinalProduct, vaccine.VaccineMedicinalProduct)));
                    details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("DetailCertificateManufacturer", culture), DataResolver.GetValues(ValueSetName.VaccineMahNamf, vaccine.MarketingAuthorizationHolder)));
                    details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("DetailCertificateDoseNumber", culture), vaccine.DoseNumber.ToString()));
                    details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("DetailCertificateTotalDoses", culture), vaccine.TotalSeriesOfDoses.ToString()));
                    details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("DetailCertificateVaccinationDate", culture), vaccine.DateOfVaccination.ToString(cultureInfo.DateTimeFormat.ShortDatePattern)));
                    details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("DetailCertificateMemberState", culture), vaccine.Country));
                    details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("DetailCertificateCertificateIssuer", culture), vaccine.CertificateIssuer));
                    details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("DetailCertificateUniqueCertificateIdentifier", culture), vaccine.CertificateID));
                }
            }
            else if (c.Certificate.Test != null && c.Certificate.Test.Length > 0)
            {
                foreach (var test in c.Certificate.Test)
                {
                    details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("TypeTest", culture), null));
                    details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("DetailCertificateTestType", culture), DataResolver.GetValues(ValueSetName.TestType, test.TypeOfTest)));
                    details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("DetailCertificateSampleCollectionDate", culture), test.SampleCollectionDate.ToString(cultureInfo.DateTimeFormat.FullDateTimePattern)));
                    details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("DetailCertificateTestResult", culture), DataResolver.GetValues(ValueSetName.TestResult, test.TestResult)));
                    details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("DetailCertificateTestFacility", culture), test.TestingCentre));
                    details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("DetailCertificateMemberState", culture), test.Country));
                    details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("DetailCertificateCertificateIssuer", culture), test.CertificateIssuer));
                    details.Add(new CertificateDetailItemViewModel(StringsHelper.GetString("DetailCertificateUniqueCertificateIdentifier", culture), test.CertificateID));
                }
            }

            this.Details = details;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
