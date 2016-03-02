using System;
using System.IO;
using System.Resources;
using Oodrive.GetText.Core.Po;

namespace Oodrive.GetText.Core.Resources
{
    public class PoResourceSet : ResourceSet
    {
        public PoResourceSet(Stream stream, PluralRuleHolder holder)
            : base(new PoResourceReader(stream, holder))
        {
        }

        public override Type GetDefaultReader()
        {
            return typeof(PoResourceReader);
        }
    }
}
