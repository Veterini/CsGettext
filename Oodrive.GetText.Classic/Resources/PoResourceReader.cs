using System;
using System.Collections;
using System.IO;
using System.Resources;

namespace Oodrive.GetText.Classic.Resources
{
    internal class PoResourceReader : IResourceReader
    {
        private Stream Stream { get; }

        public PoResourceReader(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }
            Stream = stream;
        }

        public void Close()
        {
            Stream.Close();
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            using (var reader = new StreamReader(Stream))
            {
                return PoDictionary.ParseIntoDictionary(reader).GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            Stream.Dispose();
        }
    }
}
