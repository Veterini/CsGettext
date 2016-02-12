using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using Path = Pri.LongPath.Path; 
// ReSharper disable EventNeverSubscribedTo.Global

namespace Oodrive.GetText.Classic.Resources
{
    /// <summary>
    /// Extendable file based resource manager.
    /// </summary>
    public class FileBasedResourceManager : ResourceManager
    {
        #region Properties

        /// <summary>
        /// FilePath to retrieve the files from.
        /// </summary>
        protected string FilePath { get; set; }

        /// <summary>
        /// Format of the resource set po file based on {{culture}} and {{resource}} placeholders.
        /// </summary>
        protected string FileFormat { get; set; }

        public sealed override bool IgnoreCase { get; set; }

        private IDictionary<CultureInfo,ResourceSet> InternalResourceSets { get; }

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
            FailedResourceSet?.Invoke(this, new ResourceSetCreationEventArgs(filename, ResourceSetType,exception: ex));
        }

        #endregion

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="name">Name of the resource</param>
        /// <param name="filePath">FilePath to retrieve the files from</param>
        /// <param name="fileformat">Format of the file name using {{resource}} and {{culture}} placeholders.</param>
        protected FileBasedResourceManager(string name, string filePath, string fileformat)
        {
            FilePath = filePath;
            FileFormat = fileformat;
            BaseNameField = name;

            IgnoreCase = false;
            InternalResourceSets = new Dictionary<CultureInfo, ResourceSet>();
        }

        protected override string GetResourceFileName(CultureInfo culture)
        {
            return FileFormat.Replace("{{culture}}", culture.Name).Replace("{{resource}}", BaseNameField);
        }

        protected override ResourceSet InternalGetResourceSet(CultureInfo culture, bool createIfNotExists, bool tryParents)
        {
            if (FilePath == null && FileFormat == null) return null;
            if (culture == null || culture.Equals(CultureInfo.InvariantCulture)) return null;

            ResourceSet rs;
            var resourceSets = InternalResourceSets;

            if (TryFetchResourceSet(resourceSets, culture, out rs)) return rs;

            var resourceFileName = FindResourceFile(culture);
            if (resourceFileName == null)
            {
                if (!tryParents) return rs;

                var parent = culture.Parent;
                rs = InternalGetResourceSet(parent, createIfNotExists, true);
                AddResourceSet(resourceSets, culture, ref rs);
                return rs;
            }
            rs = CreateResourceSet(resourceFileName);
            AddResourceSet(resourceSets, culture, ref rs);
            return rs;
        }

        private ResourceSet InternalCreateResourceSet(string resourceFileName)
        {
            object[] args =  { resourceFileName };
            return (ResourceSet)Activator.CreateInstance(ResourceSetType, args);
        }

        private ResourceSet CreateResourceSet(string resourceFileName)
        {
            ResourceSet set = null;

            try
            {
                set = InternalCreateResourceSet(resourceFileName);
                RaiseCreatedResourceSet(resourceFileName, set);
            }
            catch (Exception ex)
            {
                RaiseFailedResourceSet(resourceFileName, ex);
            }

            return set;
        }

        private string FindResourceFile(CultureInfo culture)
        {
            var resourceFileName = GetResourceFileName(culture);
            var path = FilePath ?? string.Empty;

            // Try with simple FilePath + filename combination
            var fullpath = Path.Combine(path, resourceFileName);
            if (File.Exists(fullpath)) return fullpath;

            // If FilePath is relative, attempt different directories
            if (path != string.Empty && Path.IsPathRooted(path)) return null;

            // Try the entry assembly dir
            string dir;
            if (Assembly.GetEntryAssembly() != null)
            {
                dir = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                    path);
                fullpath = Path.Combine(dir, resourceFileName);
                if (File.Exists(fullpath)) return fullpath;
            }

            var executingAssembly = Assembly.GetExecutingAssembly();
            var entryAssembly = Assembly.GetEntryAssembly();
            // Else try the executing assembly dir
            if (entryAssembly != null &&
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) ==
                Path.GetDirectoryName(executingAssembly.Location)) return null;

            dir = Path.Combine(Path.GetDirectoryName(executingAssembly.Location), path);
            fullpath = Path.Combine(dir, resourceFileName);
            return File.Exists(fullpath) ? fullpath : null;
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

    }
}
