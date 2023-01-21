using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Tecka.API;
using Tecka.Repository.V1;
using Tecka.Utility;

namespace Tecka.ViewModels
{
    public class TeckaViewModel : INotifyPropertyChanged
    {
        private bool _isInitialized = false;
        private readonly ICertificateRepository _repository;

        private ObservableCollection<PersonViewModel> _persons;

        public ObservableCollection<PersonViewModel> Persons
        {
            get { return _persons; }
            set { _persons = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Persons))); }
        }

        public TeckaViewModel(ICertificateRepository repository)
        {
            this._repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<bool> InitializeAsync()
        {
            if (_isInitialized) return false;

            await StringsHelper.LoadStrings(new string[] { "en", "cs" });
            await DataResolver.LoadConfiguration();
            await ReloadCertificates();
            _isInitialized = true;
            return true;
        }

        private async Task ReloadCertificates()
        {
            string[] certificates = await _repository.GetCertificates();
            var persons = await LoadCertificatesAsync(certificates);
            Persons = new ObservableCollection<PersonViewModel>(persons);
        }

        public async Task AddPersonFromUzisAsync()
        {
            try
            {
                string[] certificates = await DownloadCertificatesFromUzisAsync();
                await _repository.AddCertificates(certificates);
                await ReloadCertificates();
            }
            catch(Exception ex)
            {
                // TODO
            }
        }

        public async Task AddCertificateFromQRAsync(string qrCode)
        {
            try
            {
                await _repository.AddCertificate(qrCode);
                await ReloadCertificates();
            }
            catch (Exception ex)
            {
                // TODO
            }
        }

        public async Task DeletePersonAsync(PersonViewModel person)
        {
            foreach(var certificate in person.Certificates)
            {
                await _repository.DeleteCertificate(certificate.QrData);
            }

            await ReloadCertificates();
        }

        public async Task DeleteCertificateAsync(CertificateViewModel certificate)
        {
            await _repository.DeleteCertificate(certificate.QrData);
            await ReloadCertificates();
        }

        public async Task<IList<PersonViewModel>> LoadCertificatesAsync(string[] certificates)
        {
            List<CertificateViewModel> decodedCertificates = new List<CertificateViewModel>();
            Dictionary<string, PersonViewModel> persons = new Dictionary<string, PersonViewModel>();

            for (int i = 0; i < certificates.Length; i++)
            {
                var certificateVM = new CertificateViewModel();
                await certificateVM.LoadAsync(certificates[i]);
                decodedCertificates.Add(certificateVM);
            }

            if (decodedCertificates.Count > 0)
            {
                for (int i = 0; i < decodedCertificates.Count; i++)
                {
                    var certificate = decodedCertificates[i];
                    PersonViewModel pvm;
                    if(!persons.TryGetValue(certificate.PersonID, out pvm))
                    {
                        pvm = new PersonViewModel(certificate.PersonID, certificate.Name, certificate.Birthday);
                        persons.Add(certificate.PersonID, pvm);
                    }

                    pvm.Certificates.Add(certificate);
                }
            }

            return persons.Values.ToList();
        }

        public async Task<string[]> DownloadCertificatesFromUzisAsync()
        {
            List<string> ret = new List<string>();
            using (IHttpClientWrapper httpClient = new NetHttpClientWrapper())
            {
                OckoUzisClient client = new OckoUzisClient(httpClient);
                await client.Login();

                OckoPerson[] persons = await client.GetPersonsAsync();
                foreach (var person in persons)
                {
                    OckoCertificate[] certificates = await client.GetCertificatesAsync(person.Id);
                    foreach (var certificate in certificates)
                    {
                        ret.Add(certificate.qrData);
                    }
                }

                return ret.ToArray();
            }
        }

        public async Task LoadRules()
        {
            using (IHttpClientWrapper httpClient = new NetHttpClientWrapper())
            {
                MzcrClient client = new MzcrClient(httpClient);
                AppInfo appInfo = await client.GetInfoAsync();
                LoadCertificatesResult certificates = await client.LoadCertificatesAsync();
                LoadRevokedCertificatesResult revoked = await client.LoadRevokedCertificatesAsync();
                LoadDgcRulesResult rules = await client.LoadDgcRulesAsync();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
