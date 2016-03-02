using System;
using System.Collections;
using System.IO;
using System.Resources;
using Oodrive.GetText.Core.Po;

namespace Oodrive.GetText.Core.Resources
{
    public class PoResourceReader : IResourceReader
    {
        private Stream Stream { get; }

        private readonly PluralRuleHolder _holder;

        public PoResourceReader(Stream stream, PluralRuleHolder holder)
        {
            _holder = holder;
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
                var dic = parser.ToDictionary();
                if(_holder != null) _holder.PluralRule = parser.Header?.PluralFormSelector;
                return dic.GetEnumerator();
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
