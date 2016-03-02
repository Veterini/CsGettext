using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using NString;
using Oodrive.GetText.Core;
using Oodrive.GetText.Core.PluralFormSelectors;
using Oodrive.GetText.Core.Po;
using Oodrive.GetText.Core.Resources;

namespace oodrive.GetText.Mono.Resources
{
    public class MonoPoResourceManager : LocalizationAssemblyBasedResourceManager
    {
        private static readonly IPluralFormSelector[] Selectors =
        {
           new UnaryPluralFormSelector(),
           new SingularPluralFormSelector(),
           new PolishPluralFormSelector(),
           new BinaryPluralFormSelector(),
        };

        private readonly PluralRuleHolder _pluralRuleHolder = new PluralRuleHolder();

        #region Defaults

        private const string DefaultFileFormat = "{{resource}}.{{culture}}.po";
        private const string DefaultPath = "Resources";

        #endregion

        #region Properties

        /// <summary>
        /// Returns the Gettext resource set type used.
        /// </summary>
        public override Type ResourceSetType => typeof(PoResourceSet);

        public CultureInfo Language { get; set; }

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
        /// Loads the named configuration section and retrieves file format and path from "fileformat" and "path" settings.
        /// </summary>
        /// <param name="section">Name of the section to retrieve.</param>
        /// <returns>True if the configuration section was loaded.</returns>
        public bool LoadConfiguration(string section)
        {
            var config = ConfigurationManager.GetSection(section) as NameValueCollection;

            if (config == null) return false;

            ResourceFormat = config["fileformat"] ?? ResourceFormat;
            ResourcesPath = config["path"] ?? ResourcesPath;

            return true;
        }

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

        private int GetPluralForm(int value)
        {
            var rule = _pluralRuleHolder.PluralRule;
            return rule?.Invoke(value) ?? GetPluralFormFromSelectors(value);
        }

        private int GetPluralFormFromSelectors(int count)
        {
            var selector = Selectors.FirstOrDefault(_ => _.ApplicableCultures.Contains(Language));

            return selector?.GetPluralForm(count) ?? (count == 1 ? 0 : 1);
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

        protected override ResourceSet InternalCreateResourceSet(Stream resourceFileStream)
        {
            object[] args = { resourceFileStream, _pluralRuleHolder };
            var rs = (PoResourceSet)Activator.CreateInstance(ResourceSetType, args);
            return rs;
        }
    }
}
