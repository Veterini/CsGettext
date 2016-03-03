using System.Collections.Generic;
using System.Linq;

namespace Oodrive.GetText.Core.Po
{
    public class ContextualPluralPoEntry : PoEntryBase
    {
        public ContextualPluralPoEntry(string id, string pluralKey, string context, int nPlurals, IDictionary<int, string> traductions, IEnumerable<string> comments, IEnumerable<string> translatorComments, bool isFuzzy = false, bool isObselete = false) : base(id, isFuzzy, isObselete, comments, translatorComments)
        {
            _traductions = traductions;
            _nPlurals = nPlurals;
            Context = context;
            PluralKey = pluralKey;
        }

        public ContextualPluralPoEntry(string id, string pluralKey, string context, int nPlurals, IDictionary<int, string> traductions, bool isFuzzy = false, bool isObselete = false) : this(id, pluralKey, context, nPlurals, traductions, Enumerable.Empty<string>(), Enumerable.Empty<string>(), isFuzzy, isObselete)
        {
            Context = context;
        }

        private readonly int _nPlurals;
        private readonly IDictionary<int, string> _traductions;

        public string this[int pluralForm]    // Indexer declaration
        {
            get
            {
                if (pluralForm >= _nPlurals) return string.Empty;

                string trad;
                if (!_traductions.TryGetValue(pluralForm, out trad)) return string.Empty;

                return trad;
            }
        }

        public string Context { get; }

        public string PluralKey { get; }

        public override bool IsContextual => true;

        public override bool IsPlural => true;

        public override void Accept(Dictionary<string, string> dic)
        {
            foreach (var plural in _traductions)
            {
                dic[GetTextKeyGenerator.GetPluralKeyAndContext(Id, plural.Key, Context)] = plural.Value;
            }
        }
    }
}
