using System;
using System.Collections;
using System.IO;
using System.Resources;
using Oodrive.GetText.Core.Po;

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
            using (var parser = new PoParser(new StreamReader(Stream)))
            {
                return parser.ToDictionary().GetEnumerator();
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
