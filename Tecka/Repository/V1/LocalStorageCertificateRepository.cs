using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tecka.Repository.V1
{
    public class LocalStorageCertificateRepository : ICertificateRepository
    {
        private const string VERSION1 = "CertificatesV1";

        private readonly Windows.Storage.ApplicationDataContainer _localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

        public Task AddCertificate(string certificate)
        {
            return AddCertificates(new string[] { certificate });
        }

        public Task AddCertificates(string[] certificates)
        {
            IList<string> finalCertificates = new List<string>();
            if (_localSettings.Values.ContainsKey(VERSION1))
            {
                string[] existingCertificates = JsonConvert.DeserializeObject<string[]>((string)_localSettings.Values[VERSION1]);
                foreach (var existingCertificate in existingCertificates)
                {
                    finalCertificates.Add(existingCertificate);
                }
            }

            foreach (var cert in certificates)
            {
                finalCertificates.Add(cert);
            }

            _localSettings.Values[VERSION1] = JsonConvert.SerializeObject(finalCertificates.Distinct().ToArray());

            return Task.CompletedTask;
        }

        public Task DeleteCertificate(string certificate)
        {
            if (_localSettings.Values.ContainsKey(VERSION1))
            {
                var certificates = JsonConvert.DeserializeObject<string[]>((string)_localSettings.Values[VERSION1]).ToList();
                certificates.Remove(certificate);
                _localSettings.Values[VERSION1] = JsonConvert.SerializeObject(certificates.Distinct().ToArray());
            }

            return Task.CompletedTask;
        }

        public Task<string[]> GetCertificates()
        {
            if (_localSettings.Values.ContainsKey(VERSION1))
            {
                var certificates = JsonConvert.DeserializeObject<string[]>((string)_localSettings.Values[VERSION1]);
                return Task.FromResult(certificates);
            }

            return Task.FromResult(new string[] { });
        }
    }
}
