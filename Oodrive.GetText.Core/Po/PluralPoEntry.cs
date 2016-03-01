using System.Collections.Generic;
using System.Linq;

namespace Oodrive.GetText.Core.Po
{
    class PluralPoEntry : PoEntryBase
    {
        public PluralPoEntry(string id, int nPlurals, IDictionary<int,string> traductions, IEnumerable<string> comments, IEnumerable<string> translatorComments, bool isFuzzy = false, bool isObselete = false) : base(id, isFuzzy, isObselete, comments, translatorComments)
        {
            _traductions = traductions;
            _nPlurals = nPlurals;
        }

        public PluralPoEntry(string id, int nPlurals, IDictionary<int, string> traductions, bool isFuzzy = false, bool isObselete = false) : this(id, nPlurals, traductions, Enumerable.Empty<string>(), Enumerable.Empty<string>(), isFuzzy, isObselete)
        {
        }

        private readonly int _nPlurals;
        private readonly IDictionary<int, string> _traductions;

        public string this[int pluralForm]    // Indexer declaration
        {
            get
            {
                if(pluralForm >= _nPlurals) return string.Empty;

                string trad;
                if(!_traductions.TryGetValue(pluralForm, out trad)) return string.Empty;

                return trad;
            }
        }

        public override bool IsPlural => true;
    }
}
