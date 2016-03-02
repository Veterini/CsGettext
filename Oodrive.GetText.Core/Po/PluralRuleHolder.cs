using System;

namespace Oodrive.GetText.Core.Po
{
    public class PluralRuleHolder
    {
        public Func<int, int> PluralRule { get; set; }
    }
}
