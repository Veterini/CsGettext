using System.Collections.Generic;
using System.Linq;

namespace Oodrive.GetText.Core.Po
{
    public class PoEntry : PoEntryBase
    {
        public PoEntry(string id, string value, IEnumerable<string> comments, IEnumerable<string> translatorComments, bool isFuzzy = false, bool isObselete = false) : base(id, isFuzzy, isObselete, comments, translatorComments)
        {
            Value = value;
        }

        public PoEntry(string id, string value, bool isFuzzy = false, bool isObselete = false) : this(id, value, Enumerable.Empty<string>(), Enumerable.Empty<string>(), isFuzzy, isObselete)
        {
        }

        public override string Value { get; }

        public override void Accept(Dictionary<string, string> dic)
        {
            dic[Id] = Value;
        }
    }
}
