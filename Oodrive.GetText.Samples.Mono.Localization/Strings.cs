using System.Globalization;
using Oodrive.GetText.Mono.Extensions;
using Oodrive.GetText.Mono.Resources;

namespace Oodrive.GetText.Samples.Mono.Localization
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
                TextResources.Language = _language;
            }
        }

        private static MonoPoResourceManager _monoPoResourceManager;
        public static MonoPoResourceManager MonoPoResourceManager
        {
            get
            {
                if (_monoPoResourceManager != null)
                    return _monoPoResourceManager;

                _monoPoResourceManager = MonoPoResourceManager.CreateFromConfiguration("Strings", "getText", typeof(Strings).Assembly);
                return _monoPoResourceManager;
            }
        }

        /// <summary>
        /// Returns a string similar to 'Hello {Name}!'
        /// </summary>
        public static string HelloUser => MonoPoResourceManager.GetString("HelloUser");

        /// <summary>
        /// Returns a string similar to 'Hello {Name}!'
        /// </summary>
        public static string HelloUser_(object parameter)
        {
            return MonoPoResourceManager.GetString("HelloUser", parameter);
        }

        /// <summary>
        /// Returns a string similar to '{Occurence} files'
        /// </summary>
        public static string NbrOfFiles(int count, object parameter = null)
        {
            return MonoPoResourceManager.GetStringPlur("NbrOfFiles", count, parameter);
        }

        /// <summary>
        /// Returns a string similar to 'Hello everyone!'
        /// </summary>
        public static string HelloWorld(string context, object parameter = null)
        {
            return MonoPoResourceManager.GetStringCtxt("HelloWorld", context, parameter);
        }

    }
}
