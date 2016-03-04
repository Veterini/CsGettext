using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Reflection;
using NString;
using Oodrive.GetText.Core;
using Oodrive.GetText.Core.Resources;

namespace Oodrive.GetText.Mono.Resources
{
    public class MonoPoResourceManager : PoBasedResourceManager
    {
        #region Defaults

        private const string DefaultFileFormat = "{{culture}}.{{resource}}.po";
        private const string DefaultPath = "Resources";

        #endregion

        #region Constructos

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="name">Name of the resource</param>
        /// <param name="path">Path to retrieve the files from</param>
        /// <param name="fileformat">Format of the file name using {{resource}} and {{culture}} placeholders.</param>
        /// <param name="localizationAssembly">Assembly used for localization</param>
        public MonoPoResourceManager(string name, string path, string fileformat, Assembly localizationAssembly)
            : base(name, path, fileformat, localizationAssembly)
        {
        }

        /// <summary>
        /// Creates a new instance using local path and "{{culture}}\{{resource}}.po" file format.
        /// </summary>
        /// <param name="name">Name of the resource</param>
        /// <param name="localizationAssembly">Assembly used for localization</param>
        public MonoPoResourceManager(string name, Assembly localizationAssembly)
            : base(name, DefaultPath, DefaultFileFormat, localizationAssembly)
        {
        }

        #endregion

        #region Configuration

        /// <summary>
        /// Creates a new instance retrieving path and fileformat from the specified configuration section.
        /// </summary>
        /// <param name="name">Name of the resource</param>
        /// <param name="section">Name of the configuration section with fileformat and path settings</param>
        /// <param name="fallbackFileFormat">File format to be used if configuration could not be retrieved</param>
        /// <param name="fallbackPath">Path to be used if configuration could not be retrieved</param>
        /// <param name="localizationAssembly">Assembly used for localization</param>
        /// <returns>New instance of ResourceManager</returns>
        public static MonoPoResourceManager CreateFromConfiguration(string name, string section, Assembly localizationAssembly, string fallbackFileFormat = DefaultFileFormat, string fallbackPath = DefaultPath)
        {
            var config = ConfigurationManager.GetSection(section) as NameValueCollection;

            string fileformat;
            string path;

            if (config == null)
            {
                fileformat = fallbackFileFormat;
                path = fallbackPath;
            }
            else
            {
                fileformat = config["fileformat"] ?? fallbackFileFormat;
                path = config["path"] ?? fallbackPath;
            }


            return new MonoPoResourceManager(name, path, fileformat, localizationAssembly);
        }

        #endregion

        public string GetStringPlur(string key, int value, object parameters = null)
        {
            var form = GetPluralForm(value);
            var pluralKey = GetTextKeyGenerator.GetPluralKey(key, form);

            var result = GetString(pluralKey);
            var values = parameters.AddProperty("Occurence", value);
            return StringTemplate.Format(!result.IsNullOrEmpty() ? result :  $"[{key} : Occurence {value}]", values);
        }

        public string GetStringCtxt(string key, string context, object parameters = null)
        {
            var contextKey = GetTextKeyGenerator.GetContextKey(key, context);
            var result = GetString(contextKey);

            var str = !result.IsNullOrEmpty() ? result : $"[{key} : Context {context}]";
            return parameters != null ? StringTemplate.Format(str, parameters) : str;
        }

        public string GetStringPlurCtxt(string key, int value, string context, object parameters = null)
        {
            var form = GetPluralForm(value);
            var pluralKey = GetTextKeyGenerator.GetPluralKeyAndContext(key, form, context);

            var result = GetString(pluralKey);
            var values = parameters.AddProperty("Occurence", value);

            return StringTemplate.Format(!result.IsNullOrEmpty() ? result : $"[{key} : Occurence {value} : Context {context}]", values);
        }

        public override string GetString(string key)
        {
            var result = base.GetString(key, Language);

            return !result.IsNullOrEmpty() ? result : $"[{key}]";
        }

        public string GetString(string key, object parameters)
        {
            var result = GetString(key) ?? $"[{key}]";

            return parameters == null ? result : StringTemplate.Format(result , parameters);
        }

        protected override Stream FindResourceFileStream(CultureInfo culture)
        {
            var resourceFileName = GetFileResourceName(culture);

            var loc = FindResourceFileStream(resourceFileName);
            if (loc != null)
                return loc;

            resourceFileName = GetInvariantFileResourceName();
            return FindResourceFileStream(resourceFileName);
        }

        private string GetInvariantFileResourceName()
        {
            var baseKey = "{{resource}}.po".Replace("{{resource}}", BaseNameField);

            if (!ResourcesPath.IsNullOrEmpty())
                baseKey = ResourcesPath + "." + baseKey;

            if (LocalizationAssembly != null)
            {
                baseKey = LocalizationAssembly.GetName().Name + "." + baseKey;
            }

            return baseKey;
        }

        private Stream FindResourceFileStream(string resourceFileName)
        {
            Stream loc;
            if (LocalizationAssembly != null)
            {
                loc = LocalizationAssembly.GetManifestResourceStream(resourceFileName);

                if (loc != null) return loc;
            }

            var entryAssembly = Assembly.GetEntryAssembly();
            // Try the entry assembly dir
            loc = entryAssembly?.GetManifestResourceStream(resourceFileName);

            if (loc != null)
                return loc;

            var executingAssembly = Assembly.GetExecutingAssembly();

            // Else try the executing assembly dir
            loc = executingAssembly.GetManifestResourceStream(resourceFileName);

            return loc;
        }
    }
}
