using System.Globalization;
using NString;
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

        public static string Tp(string key, string plural, int value)
        {
            var resourceManager = GetTextResources.GetResourceManager(Key);

            return resourceManager == null ? StringTemplate.Format(value == 1 ? key : plural, new { Occurence = value }) : resourceManager.GetStringPlur(key, plural, value);
        }

        public static string Tc(string key, string context)
        {
            var resourceManager = GetTextResources.GetResourceManager(Key);

            return resourceManager == null ? key : resourceManager.GetStringCtxt(key, context);
        }

        public static string Tpc(string key, string plural, int value, string context)
        {
            var resourceManager = GetTextResources.GetResourceManager(Key);

            return resourceManager == null ? StringTemplate.Format(value == 1 ? key : plural, new { Occurence = value }) : resourceManager.GetStringPlurCtxt(key, plural, value, context);
        }

        public static string T(string key)
        {
            var resourceManager = GetTextResources.GetResourceManager(Key);

            return resourceManager == null ? key : resourceManager.GetString(key);
        }
    }
}
