using System.Globalization;
using NString;
using Oodrive.GetText.Classic;
using Oodrive.GetText.Classic.Extensions;

namespace Oodrive.GetText.Samples.Localization
{
    public static class Strings
    {
        private static CultureInfo _language;

        public static CultureInfo Language
        {
            get { return _language; }
            set
            {
                if (Equals(_language, value)) return;
                _language = value;
                GetTextResources.Language = _language;
            }
        }

        public static string Key { get; set; }

        public static string Tp(string key, string plural, int value, object parameters = null)
        {
            var resourceManager = GetTextResources.GetResourceManager(Key);
            var values = parameters.AddProperty("Occurence", value);
            return resourceManager != null ? resourceManager.GetStringPlur(key, plural, value, parameters) : StringTemplate.Format(value == 1 ? key : plural, values);
        }

        public static string Tc(string key, string context, object parameters = null)
        {
            var resourceManager = GetTextResources.GetResourceManager(Key);

            return resourceManager == null ? (parameters != null ? StringTemplate.Format(key,parameters) : key) : resourceManager.GetStringCtxt(key, context, parameters);
        }

        public static string Tpc(string key, string plural, int value, string context, object parameters = null)
        {
            var resourceManager = GetTextResources.GetResourceManager(Key);
            var values = parameters.AddProperty("Occurence", value);
            return resourceManager == null ? StringTemplate.Format(value == 1 ? key : plural, values) : resourceManager.GetStringPlurCtxt(key, plural, value, context, parameters);
        }

        public static string T(string key, object parameters = null)
        {
            var resourceManager = GetTextResources.GetResourceManager(Key);
            var str = (resourceManager == null ? key : resourceManager.GetString(key)) ?? string.Empty;

            return parameters != null ? StringTemplate.Format(str, parameters) : str;
        }
    }
}
