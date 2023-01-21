using NL.MinVWS.Encoding;
using PeterO.Cbor;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Tecka
{
    public class DecodedCertificate
    {
        public string Issuer { get; set; }

        public DateTime IssuedAt { get; set; }

        public DateTime Expiration { get; set; }

        public DecodedPayload Certificate { get; set; }
    }

    public class DecodedPayloadName
    {
        [Newtonsoft.Json.JsonProperty("fn")]
        public string FamilyName { get; set; }

        [Newtonsoft.Json.JsonProperty("gn")]
        public string GivenName { get; set; }

        [Newtonsoft.Json.JsonProperty("fnt")]
        public string FamilyNameTransliterated { get; set; }

        [Newtonsoft.Json.JsonProperty("gnt")]
        public string GivenNameTransliterated { get; set; }
    }

    public class DecodedPayloadBase
    {
        [Newtonsoft.Json.JsonProperty("ci")]
        public string CertificateID { get; set; }

        [Newtonsoft.Json.JsonProperty("is")]
        public string CertificateIssuer { get; set; }

        [Newtonsoft.Json.JsonProperty("co")]
        public string Country { get; set; }

        [Newtonsoft.Json.JsonProperty("tg")]
        public string DiseaseTargeted { get; set; }
    }

    public class DecodedPayloadVaccine : DecodedPayloadBase
    {
        [Newtonsoft.Json.JsonProperty("dn")]
        public int DoseNumber { get; set; }

        [Newtonsoft.Json.JsonProperty("dt")]
        public DateTime DateOfVaccination { get; set; }

        [Newtonsoft.Json.JsonProperty("ma")]
        public string MarketingAuthorizationHolder { get; set; }

        [Newtonsoft.Json.JsonProperty("mp")]
        public string VaccineMedicinalProduct { get; set; }

        [Newtonsoft.Json.JsonProperty("sd")]
        public int TotalSeriesOfDoses { get; set; }

        [Newtonsoft.Json.JsonProperty("vp")]
        public string VaccineOrProphylaxis { get; set; }
    }

    public class DecodedPayloadTest : DecodedPayloadBase
    {
        [Newtonsoft.Json.JsonProperty("tt")]
        public string TypeOfTest { get; set; }

        [Newtonsoft.Json.JsonProperty("sc")]
        public DateTime SampleCollectionDate { get; set; }

        [Newtonsoft.Json.JsonProperty("tr")]
        public string TestResult { get; set; }

        [Newtonsoft.Json.JsonProperty("tc")]
        public string TestingCentre { get; set; }
    }

    public class DecodedPayload
    {
        [Newtonsoft.Json.JsonProperty("dob")]
        public DateTime DateOfBirth { get; set; }

        [Newtonsoft.Json.JsonProperty("nam")]
        public DecodedPayloadName Name { get; set; }

        [Newtonsoft.Json.JsonProperty("ver")]
        public string Version { get; set; }

        [Newtonsoft.Json.JsonProperty("v")]
        public DecodedPayloadVaccine[] Vaccination { get; set; }

        [Newtonsoft.Json.JsonProperty("t")]
        public DecodedPayloadTest[] Test { get; set; }
    }

    public static class CertificateDecoder
    {
        public static async Task<DecodedCertificate> DecodeAsync(string qrData)
        {
            const string PREFIX = "HC1:";

            // strip the prefix
            if (qrData.StartsWith(PREFIX))
            {
                qrData = qrData.Substring(PREFIX.Length);
            }
            else
            {
                throw new Exception("No header!");
            }

            // decode Base45
            byte[] decoded = Base45Encoding.Decode(qrData);

            // decompress Zlib
            byte[] cborToken = null;

            using (MemoryStream ms = new MemoryStream(decoded))
            {
                using(Org.BouncyCastle.Utilities.Zlib.ZInputStream zs = new Org.BouncyCastle.Utilities.Zlib.ZInputStream(ms))
                {
                    using (MemoryStream zms = new MemoryStream())
                    {
                        await zs.CopyToAsync(zms);
                        cborToken = zms.ToArray();
                    }
                }
            }

            // deserialize CBOR - skip validation
            CBORObject cborObject = CBORObject.DecodeFromBytes(cborToken);
            Com.AugustCellars.COSE.Message msg = Com.AugustCellars.COSE.Message.DecodeFromCBOR(cborObject);

            DecodedCertificate cert;
            if(msg is Com.AugustCellars.COSE.Sign1Message smsg)
            {
                var encodedToken = smsg.GetContent();
                var webTokenCbor = CBORObject.DecodeFromBytes(encodedToken);

                Com.AugustCellars.WebToken.CWT cwt = new Com.AugustCellars.WebToken.CWT(webTokenCbor);

                // https://ec.europa.eu/health/sites/default/files/ehealth/docs/digital-green-certificates_v1_en.pdf
                var iss = cwt.GetClaim(CBORObject.FromObject(1)); // iss, issuer (optional)
                var iat = cwt.GetClaim(CBORObject.FromObject(6)); // iat, issued at
                var exp = cwt.GetClaim(CBORObject.FromObject(4)); // exp, expiration time
                var hcert = cwt.GetClaim(CBORObject.FromObject(-260)); // hcert, health certificate claim

                cert = new DecodedCertificate()
                {
                    Issuer = iss?.AsString(),
                    IssuedAt = DateTimeOffset.FromUnixTimeSeconds(iat.AsInt64()).DateTime, // UTC
                    Expiration = DateTimeOffset.FromUnixTimeSeconds(exp.AsInt64()).DateTime
                };

                // deserialize JSON
                string webTokenStr = hcert.Values.FirstOrDefault()?.ToJSONString();
                var payload = Newtonsoft.Json.JsonConvert.DeserializeObject<DecodedPayload>(webTokenStr);
                cert.Certificate = payload;
            }
            else
            {
                throw new NotSupportedException();
            }

            return cert;
        }
    }
}
