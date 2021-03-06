﻿using System.Collections.Generic;
using System.Linq;

namespace Oodrive.GetText.Core.Po
{
    public class PluralPoEntry : PoEntryBase
    {
        public PluralPoEntry(string id, string pluralId, int nPlurals, IDictionary<int,string> traductions, IEnumerable<string> comments, IEnumerable<string> translatorComments, bool isFuzzy = false, bool isObselete = false) : base(id, isFuzzy, isObselete, comments, translatorComments)
        {
            _traductions = traductions;
            _nPlurals = nPlurals;
            PluralId = pluralId;
        }

        public PluralPoEntry(string id, string pluralId, int nPlurals, IDictionary<int, string> traductions, bool isFuzzy = false, bool isObselete = false) : this(id, pluralId, nPlurals, traductions, Enumerable.Empty<string>(), Enumerable.Empty<string>(), isFuzzy, isObselete)
        {
        }

        private readonly int _nPlurals;
        private readonly IDictionary<int, string> _traductions;

        public string PluralId { get; }

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

        public override void Accept(Dictionary<string, string> dic)
        {
            foreach (var plural in _traductions)
            {
                dic[GetTextKeyGenerator.GetPluralKey(Id, plural.Key)] = plural.Value;
            }
        }
    }
}
