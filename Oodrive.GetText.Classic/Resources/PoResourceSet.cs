using System;
using System.IO;
using System.Resources;

namespace Oodrive.GetText.Classic.Resources
{
    public class PoResourceSet : ResourceSet
    {
        public PoResourceSet(string filename)
            : base(new PoResourceReader(File.OpenRead(filename)))
        {
        }

        public PoResourceSet(Stream stream)
            : base(new PoResourceReader(stream))
        {
        }

        public override Type GetDefaultReader()
        {
            return typeof(PoResourceReader);
        }
    }
}
