using System;
using System.Collections.Specialized;
using System.Configuration;
using NString;

namespace Oodrive.GetText.Classic.Resources
{
    public class PoResourceManager : FileBasedResourceManager
    {
        #region Defaults

        private const string DefaultFileFormat = "{{culture}}\\{{resource}}.po";
        private const string DefaultPath = "";

        #endregion

        #region Properties

        /// <summary>
        /// Returns the Gettext resource set type used.
        /// </summary>
        public override Type ResourceSetType => typeof(PoResourceSet);

        #endregion

        #region Constructos

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="name">Name of the resource</param>
        /// <param name="path">Path to retrieve the files from</param>
        /// <param name="fileformat">Format of the file name using {{resource}} and {{culture}} placeholders.</param>
        public PoResourceManager(string name, string path, string fileformat)
            : base(name, path, fileformat)
        {
        }

        /// <summary>
        /// Creates a new instance using local path and "{{culture}}\{{resource}}.po" file format.
        /// </summary>
        /// <param name="name">Name of the resource</param>
        public PoResourceManager(string name)
            : base(name, DefaultPath, DefaultFileFormat)
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

            FileFormat = config["fileformat"] ?? FileFormat;
            FilePath = config["path"] ?? FilePath;

            return true;
        }

        internal object GetString(string resourceKey, int? occurence, string context)
        {
            if (occurence == null && context.IsNullOrEmpty())
                return GetString(resourceKey);


            //TODO WIP
            return string.Empty;
        }

        /// <summary>
        /// Creates a new instance retrieving path and fileformat from the specified configuration section.
        /// </summary>
        /// <param name="name">Name of the resource</param>
        /// <param name="section">Name of the configuration section with fileformat and path settings</param>
        /// <param name="fallbackFileFormat">File format to be used if configuration could not be retrieved</param>
        /// <param name="fallbackPath">Path to be used if configuration could not be retrieved</param>
        /// <returns>New instance of ResourceManager</returns>
        public static PoResourceManager CreateFromConfiguration(string name, string section, string fallbackFileFormat = DefaultFileFormat, string fallbackPath = DefaultPath)
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

            return new PoResourceManager(name, path, fileformat);
        }

        #endregion

    }


}