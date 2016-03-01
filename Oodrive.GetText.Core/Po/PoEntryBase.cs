using System.Collections.Generic;
using System.Linq;

namespace Oodrive.GetText.Core.Po
{
    public abstract class PoEntryBase : IPoEntry
    {
        protected PoEntryBase(string id, bool isFuzzy, bool isObselete, IEnumerable<string> comments, IEnumerable<string> translatorComments)
        {
            Id = id;
            IsFuzzy = isFuzzy;
            IsObselete = isObselete;
            TranslatorComments = translatorComments.ToList();
            Comments = comments.ToList();
        }

        public string Id { get; }

        public bool IsObselete { get; }

        public bool IsFuzzy { get; }

        public virtual bool IsHeader => false;

        public virtual bool IsContextual => false;

        public virtual bool IsPlural => false;

        public virtual string Value => string.Empty;

        public IReadOnlyList<string> TranslatorComments { get; }

        public IReadOnlyList<string> Comments { get; }
    }
}
