using System;
using System.Resources;

namespace Oodrive.GetText.Core.Resources
{
    /// <summary>
    /// Arguments for events related to the creation, successful or not, of a resource set.
    /// </summary>
    public class ResourceSetCreationEventArgs : EventArgs
    {
        public ResourceSetCreationEventArgs(string fileName, Type type, ResourceSet set = null, Exception exception = null)
        {
            Exception = exception;
            FileName = fileName;
            ResourceSetType = type;
            ResourceSet = set;
            Success = exception == null;
        }


        /// <summary>
        /// Exception in case of error, null on success.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// FileName from where the resource set was loaded.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Type of the resource set being initialized.
        /// </summary>
        public Type ResourceSetType { get; }

        /// <summary>
        /// Instance of the resource set created, may be null on error.
        /// </summary>
        public ResourceSet ResourceSet { get; }

        /// <summary>
        /// Whether the creation was successful.
        /// </summary>
        public bool Success { get; }
    }
}
