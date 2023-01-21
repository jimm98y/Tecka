using System;
using System.Threading.Tasks;

namespace Tecka
{
    public class AppInfo
    {
        [Newtonsoft.Json.JsonProperty("verzeAplikace")]
        public string Version { get; set; }

        [Newtonsoft.Json.JsonProperty("subjektIco")]
        public Int64? IdentificationNumber { get; set; }

        [Newtonsoft.Json.JsonProperty("nazevPracoviste")]
        public string OfficeName { get; set; }
    }

    public class LoadCertificatesResult
    {
        [Newtonsoft.Json.JsonProperty("podpisoveCertifikaty")]
        public CertificateView[] Certificates { get; set; }

        [Newtonsoft.Json.JsonProperty("status")]
        public string Status { get; set; }

        [Newtonsoft.Json.JsonProperty("detail")]
        public string Detail { get; set; }
    }

    public class LoadDgcRulesResult
    {
        [Newtonsoft.Json.JsonProperty("pravidla")]
        public DgcRuleView[] Rules { get; set; }

        [Newtonsoft.Json.JsonProperty("status")]
        public string Status { get; set; }

        [Newtonsoft.Json.JsonProperty("detail")]
        public string Detail { get; set; }
    }

    public class LoadRevokedCertificatesResult
    {
        [Newtonsoft.Json.JsonProperty("revokovaneCertifikaty")]
        public RevokedCertificateView[] Certificates { get; set; }

        [Newtonsoft.Json.JsonProperty("status")]
        public string Status { get; set; }

        [Newtonsoft.Json.JsonProperty("detail")]
        public string Detail { get; set; }
    }

    public class RevokedCertificateView
    {
        [Newtonsoft.Json.JsonProperty("idCertifikatu")]
        public string Id { get; set; }

        [Newtonsoft.Json.JsonProperty("changeId")]
        public Int64? ChangeId { get; set; }
    }

    public class DgcRuleView
    {
        [Newtonsoft.Json.JsonProperty("id")]
        public Int64 Id { get; set; }

        [Newtonsoft.Json.JsonProperty("statKod")]
        public string Country { get; set; }

        [Newtonsoft.Json.JsonProperty("platnostTestu")]
        public TestValidityRule[] ValidityOfTests { get; set; }

        [Newtonsoft.Json.JsonProperty("ochranaLhutaDenOd")]
        public Int64? ProtectivePeriodDaysSinceInfection { get; set; }

        [Newtonsoft.Json.JsonProperty("ochranaLhutaDenDo")]
        public Int64? ProtectivePeriodDaysAfterInfection { get; set; }

        [Newtonsoft.Json.JsonProperty("platnostVakcinace")]
        public VaccinationValidityRule[] ValidityOfVaccination { get; set; }

        [Newtonsoft.Json.JsonProperty("platnostOd")]
        public DateTime? ValiditySince { get; set; }

        [Newtonsoft.Json.JsonProperty("platnostDo")]
        public DateTime? ValidityUntil { get; set; }
    }

    public class VaccinationValidityRule
    {
        [Newtonsoft.Json.JsonProperty("vaccineMedicinalProduct")]
        public string VaccineMedicinalProduct { get; set; }

        [Newtonsoft.Json.JsonProperty("jednodavkovaOdolnostDenOd")]
        public Int64? SingleDoseProtectionSince { get; set; }

        [Newtonsoft.Json.JsonProperty("dvoudavkovaOdolnostDenOd")]
        public Int64? DoubleDoseProtectionSince { get; set; }

        [Newtonsoft.Json.JsonProperty("prvniDavkouOdolnostDenDo")]
        public Int64? FirstDoseProtectionUntil { get; set; }

        [Newtonsoft.Json.JsonProperty("odolnostMesicDo")]
        public Int64? DurationInMonths { get; set; }
    }

    public class TestValidityRule
    {
        [Newtonsoft.Json.JsonProperty("typeOfTest")]
        public string TestType { get; set; }

        [Newtonsoft.Json.JsonProperty("platnostHod")]
        public Int64? ValidityHours { get; set; }
    }

    public class CertificateView
    {
        [Newtonsoft.Json.JsonProperty("id")]
        public Int64 Id { get; set; }

        [Newtonsoft.Json.JsonProperty("country")]
        public string Country { get; set; }

        [Newtonsoft.Json.JsonProperty("certificateType")]
        public string CertificateType { get; set; }

        [Newtonsoft.Json.JsonProperty("rawData")]
        public string RawData { get; set; }

        [Newtonsoft.Json.JsonProperty("changeId")]
        public Int64? ChangeId { get; set; }

        [Newtonsoft.Json.JsonProperty("aktivni")]
        public bool IsActive { get; set; }

        [Newtonsoft.Json.JsonProperty("kid")]
        public string Kid { get; set; }
    }

    // Swagger here: https://dgcverify.mzcr.cz/index.html
    public class MzcrClient
    {
        private const string BASE_MZCR_URL = "https://dgcverify.mzcr.cz/api/v1/verify/";

        private const string LOAD_CERTIFICATES_URL = "NactiPodpisoveCertifikaty";

        private const string LOAD_REVOKED_CERTIFICATES_URL = "NactiRevokovaneCertifikaty";
        
        private const string LOAD_DGC_RULES_URL = "NactiPravidlaPlatnostiDgc";

        private const string LOAD_INFO = "Info";

        private readonly IHttpClientWrapper _client;

        public MzcrClient(IHttpClientWrapper client)
        {
            this._client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public async Task<AppInfo> GetInfoAsync()
        {
            string response = await _client.HttpGetAsync($"{BASE_MZCR_URL}{LOAD_INFO}");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<AppInfo>(response);
        }

        public async Task<LoadRevokedCertificatesResult> LoadRevokedCertificatesAsync()
        {
            string response = await _client.HttpGetAsync($"{BASE_MZCR_URL}{LOAD_REVOKED_CERTIFICATES_URL}");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<LoadRevokedCertificatesResult>(response);
        }

        public async Task<LoadDgcRulesResult> LoadDgcRulesAsync()
        {
            string response = await _client.HttpGetAsync($"{BASE_MZCR_URL}{LOAD_DGC_RULES_URL}");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<LoadDgcRulesResult>(response);
        }

        public async Task<LoadCertificatesResult> LoadCertificatesAsync()
        {
            string response = await _client.HttpGetAsync($"{BASE_MZCR_URL}{LOAD_CERTIFICATES_URL}");
            return Newtonsoft.Json.JsonConvert.DeserializeObject<LoadCertificatesResult>(response);
        }
    }
}
