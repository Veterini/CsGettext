using System.Collections.Generic;
using System.Linq;

namespace Oodrive.GetText.Core.Po
{
    public class ContextualPoEntry : PoEntryBase
    {
        public ContextualPoEntry(string id, string value, string context, IEnumerable<string> comments, IEnumerable<string> translatorComments, bool isFuzzy = false, bool isObselete = false) : base(id, isFuzzy, isObselete, comments, translatorComments)
        {
            Context = context;
            Value = value;
        }

        public ContextualPoEntry(string id, string value, string context, bool isFuzzy = false, bool isObselete = false) : this(id, value, context, Enumerable.Empty<string>(), Enumerable.Empty<string>(), isFuzzy, isObselete)
        {
        }
        
        public override string Value { get; }

        public override void Accept(Dictionary<string, string> dic)
        {
            dic[GetTextKeyGenerator.GetContextKey(Id, Context)] = Value;
        }

        public string Context { get; }

        public override bool IsContextual => true;
    }
}
