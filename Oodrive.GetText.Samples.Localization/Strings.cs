
using System.Globalization;
using NString;
using Oodrive.GetText.Core;
using Oodrive.GetText.Classic.Extensions;
using Oodrive.GetText.Classic.Resources;

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

        private static PoResourceManager _poResourceManager;
        public static PoResourceManager PoResourceManager
        {
            get
            {
                if (_poResourceManager != null)
                    return _poResourceManager;

                _poResourceManager = PoResourceManager.CreateFromConfiguration("Strings", "getText", typeof(Strings).Assembly);
                return _poResourceManager;
            }
        }

        public static string Tp(string key, string plural, int value, object parameters = null)
        {
            var values = parameters.AddProperty("Occurence", value);
            return PoResourceManager != null ? PoResourceManager.GetStringPlur(key, plural, value, parameters) : StringTemplate.Format(value == 1 ? key : plural, values);
        }

        public static string Tc(string key, string context, object parameters = null)
        {
            return PoResourceManager == null ? (parameters != null ? StringTemplate.Format(key,parameters) : key) : PoResourceManager.GetStringCtxt(key, context, parameters);
        }

        public static string Tpc(string key, string plural, int value, string context, object parameters = null)
        {
            var values = parameters.AddProperty("Occurence", value);
            return PoResourceManager == null ? StringTemplate.Format(value == 1 ? key : plural, values) : PoResourceManager.GetStringPlurCtxt(key, plural, value, context, parameters);
        }

        public static string T(string key, object parameters = null)
        {
            var str = (PoResourceManager == null ? key : PoResourceManager.GetString(key)) ?? string.Empty;

            return parameters != null ? StringTemplate.Format(str, parameters) : str;
        }
    }
}

