using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace Tecka.Utility
{
    public static class StringsHelper
    {
        private const string DEFAULT_CULTURE = "en";

        private static Dictionary<string, Dictionary<string, string>> Localization = new Dictionary<string, Dictionary<string, string>>();

        public static string GetString(string key, string culture = null)
        {
            if (culture == null)
                culture = DEFAULT_CULTURE;

            Dictionary<string, string> localizedStrings;
            if (!Localization.TryGetValue(culture, out localizedStrings))
                localizedStrings = Localization[DEFAULT_CULTURE];

            return localizedStrings[key];
        }

        private static async Task<Dictionary<string, string>> LoadValues(string path)
        {
            StorageFile localFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri(path));
            string content = await FileIO.ReadTextAsync(localFile);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
        }

        public static async Task LoadStrings(string[] supportedCultures)
        {
            foreach(var culture in supportedCultures)
            {
                Localization.Add(culture, await LoadValues($"ms-appx:///Strings/Localization.{culture}.json"));
            }
        }
    }
}
