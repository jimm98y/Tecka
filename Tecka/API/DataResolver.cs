using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace Tecka.API
{
    public class DataValueSetValues
    {
        [Newtonsoft.Json.JsonProperty("valueSetId")]
        public string ValueSetId { get; set; }

        [Newtonsoft.Json.JsonProperty("valueSetDate")]
        public DateTime ValueSetDate { get; set; }

        [Newtonsoft.Json.JsonProperty("valueSetValues")]
        public Dictionary<string, ValueSetValue> ValueSetValues { get; set; }
    }

    public class ValueSetValue
    {
        [Newtonsoft.Json.JsonProperty("display")]
        public string Display { get; set; }

        [Newtonsoft.Json.JsonProperty("lang")]
        public string Lang { get; set; }

        [Newtonsoft.Json.JsonProperty("active")]
        public bool IsActive { get; set; }

        [Newtonsoft.Json.JsonProperty("system")]
        public string System { get; set; }

        [Newtonsoft.Json.JsonProperty("version")]
        public string Version { get; set; }
    }

    public enum ValueSetName
    {
        CountryCodes,
        DiseaseAgentTargeted,
        TestManf,
        TestResult,
        TestType,
        VaccineMahNamf,
        VaccineMedicinalProduct,
        VaccineProphylaxis
    }

    public class DataResolver
    {
        public static Dictionary<ValueSetName, DataValueSetValues> LoadedValues = new Dictionary<ValueSetName, DataValueSetValues>();
                
        public static async Task LoadConfiguration()
        {
            LoadedValues.Add(ValueSetName.CountryCodes, await LoadValues("ms-appx:///Data/country_codes.json"));
            LoadedValues.Add(ValueSetName.DiseaseAgentTargeted, await LoadValues("ms-appx:///Data/disease_agent_targeted.json"));
            LoadedValues.Add(ValueSetName.TestManf, await LoadValues("ms-appx:///Data/test_manf.json"));
            LoadedValues.Add(ValueSetName.TestResult, await LoadValues("ms-appx:///Data/test_result.json"));
            LoadedValues.Add(ValueSetName.TestType, await LoadValues("ms-appx:///Data/test_type.json"));
            LoadedValues.Add(ValueSetName.VaccineMahNamf, await LoadValues("ms-appx:///Data/vaccine_mah_manf.json"));
            LoadedValues.Add(ValueSetName.VaccineMedicinalProduct, await LoadValues("ms-appx:///Data/vaccine_medicinal_product.json"));
            LoadedValues.Add(ValueSetName.VaccineProphylaxis, await LoadValues("ms-appx:///Data/vaccine_prophylaxis.json"));
        }

        public static async Task<DataValueSetValues> LoadValues(string path)
        {
            StorageFile localFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(path));
            string content = await FileIO.ReadTextAsync(localFile);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<DataValueSetValues>(content);
        }

        public static string GetValues(ValueSetName name, string value)
        {
            return LoadedValues[name].ValueSetValues[value].Display;
        }
    }
}
