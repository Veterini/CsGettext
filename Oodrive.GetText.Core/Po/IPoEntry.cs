using System.Collections.Generic;

namespace Oodrive.GetText.Core.Po
{
    public interface IPoEntry
    {
        string Id { get; }

        string Value { get; }

        bool IsObselete { get; }

        bool IsFuzzy { get; }

        bool IsHeader { get; }

        bool IsContextual { get; }

        bool IsPlural { get; }

        IReadOnlyList<string> TranslatorComments { get; }

        IReadOnlyList<string> Comments { get; }
    }
}
