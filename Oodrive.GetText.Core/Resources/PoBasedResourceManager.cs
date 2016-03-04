using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using NString;
using Oodrive.GetText.Core.PluralFormSelectors;
using Oodrive.GetText.Core.Po;

namespace Oodrive.GetText.Core.Resources
{
    /// <summary>
    /// Extendable file based resource manager.
    /// </summary>
    public abstract class PoBasedResourceManager : ResourceManager
    {
        #region Properties

        private static readonly IPluralFormSelector[] Selectors =
        {
           new UnaryPluralFormSelector(),
           new SingularPluralFormSelector(),
           new PolishPluralFormSelector(),
           new BinaryPluralFormSelector(),
        };
        
        public CultureInfo Language { get; set; }

        private readonly PluralRuleHolder _pluralRuleHolder = new PluralRuleHolder();

        /// <summary>
        /// Returns the Gettext resource set type used.
        /// </summary>
        public override Type ResourceSetType => typeof(PoResourceSet);

        /// <summary>
        /// ResourcesPath to retrieve the files from.
        /// </summary>
        protected string ResourcesPath { get; set; }

        /// <summary>
        /// Format of the resource set po file based on {{culture}} and {{resource}} placeholders.
        /// </summary>
        protected string ResourceFormat { get; set; }

        public sealed override bool IgnoreCase { get; set; }

        protected Assembly LocalizationAssembly { get; }

        private IDictionary<CultureInfo, ResourceSet> InternalResourceSets { get; }

        #endregion

        #region Notification Events

        /// <summary>
        /// Event that notifies the successful creation of a resource set.
        /// </summary>
        public event EventHandler<ResourceSetCreationEventArgs> CreatedResourceSet;

        /// <summary>
        /// Event that notifies an error creating a resource set.
        /// </summary>
        public event EventHandler<ResourceSetCreationEventArgs> FailedResourceSet;

        private void RaiseCreatedResourceSet(string filename, ResourceSet set)
        {
            CreatedResourceSet?.Invoke(this, new ResourceSetCreationEventArgs(filename, ResourceSetType, set));
        }

        private void RaiseFailedResourceSet(string filename, Exception ex)
        {
            FailedResourceSet?.Invoke(this, new ResourceSetCreationEventArgs(filename, ResourceSetType, exception: ex));
        }

        #endregion

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
        /// Creates a new instance.
        /// </summary>
        /// <param name="name">Name of the resource</param>
        /// <param name="resourcesPath">ResourcesPath to retrieve the files from</param>
        /// <param name="fileformat">Format of the file name using {{resource}} and {{culture}} placeholders.</param>
        /// <param name="localizationAssembly"></param>
        protected PoBasedResourceManager(string name, string resourcesPath, string fileformat, Assembly localizationAssembly)
        {
            ResourcesPath = resourcesPath;
            ResourceFormat = fileformat;
            BaseNameField = name;
            LocalizationAssembly = localizationAssembly;

            IgnoreCase = false;
            InternalResourceSets = new Dictionary<CultureInfo, ResourceSet>();
        }

        protected string GetFileResourceName(CultureInfo culture)
        {
            var baseKey = ResourceFormat.Replace("{{culture}}", culture.Name).Replace("{{resource}}", BaseNameField);

            if (!ResourcesPath.IsNullOrEmpty())
                baseKey = ResourcesPath + "." + baseKey;

            if (LocalizationAssembly != null)
            {
                baseKey = LocalizationAssembly.GetName().Name + "." + baseKey;
            }

            return baseKey;
        }


        protected override ResourceSet InternalGetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents)
        {
            if (ResourcesPath == null && ResourceFormat == null) return null;
            if (culture == null || culture.Equals(CultureInfo.InvariantCulture)) return null;

            ResourceSet rs;
            var resourceSets = InternalResourceSets;

            if (TryFetchResourceSet(resourceSets, culture, out rs)) return rs;

            var resourceFileStream = FindResourceFileStream(culture);
            if (resourceFileStream == null)
            {
                if (!tryParents) return rs;

                var parent = culture.Parent;
                rs = InternalGetResourceSet(parent, createIfNotExists, true);
                AddResourceSet(resourceSets, culture, ref rs);
                return rs;
            }
            rs = CreateResourceSet(resourceFileStream);
            AddResourceSet(resourceSets, culture, ref rs);
            return rs;
        }

        private ResourceSet CreateResourceSet(Stream resourceFileStream)
        {
            ResourceSet set = null;

            try
            {
                set = InternalCreateResourceSet(resourceFileStream);
                RaiseCreatedResourceSet(ResourcesPath, set);
            }
            catch (Exception ex)
            {
                RaiseFailedResourceSet(ResourcesPath, ex);
            }

            return set;
        }

        protected virtual Stream FindResourceFileStream(CultureInfo culture)
        {
            var resourceFileName = GetFileResourceName(culture);

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

        private static void AddResourceSet(IDictionary<CultureInfo, ResourceSet> localResourceSets, CultureInfo culture, ref ResourceSet rs)
        {
            lock (localResourceSets)
            {
                if (localResourceSets.ContainsKey(culture))
                {
                    var existing = localResourceSets[culture];

                    if (existing == null || Equals(existing, rs)) return;
                    rs.Dispose();
                    rs = existing;
                }
                else
                {
                    localResourceSets.Add(culture, rs);
                }
            }
        }

        private static bool TryFetchResourceSet(IDictionary<CultureInfo, ResourceSet> localResourceSets, CultureInfo culture, out ResourceSet set)
        {
            lock (localResourceSets)
            {
                if (localResourceSets.ContainsKey(culture))
                {
                    set = localResourceSets[culture];
                    return true;
                }

                set = null;
                return false;
            }
        }

        protected int GetPluralForm(int value)
        {
            var rule = _pluralRuleHolder.PluralRule;
            return rule?.Invoke(value) ?? GetPluralFormFromSelectors(value);
        }

        protected int GetPluralFormFromSelectors(int count)
        {
            var selector = Selectors.FirstOrDefault(_ => _.ApplicableCultures.Contains(Language));

            return selector?.GetPluralForm(count) ?? count % 2;
        }

        protected ResourceSet InternalCreateResourceSet(Stream resourceFileStream)
        {
            object[] args = { resourceFileStream, _pluralRuleHolder };
            return (ResourceSet)Activator.CreateInstance(ResourceSetType, args);
        }

    }
}
