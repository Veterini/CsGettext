using System.Collections.Generic;
using System.Globalization;

namespace Oodrive.GetText.Core.PluralFormSelectors
{
    public interface IPluralFormSelector
    {
        IReadOnlyList<CultureInfo> ApplicableCultures { get; }

        int GetPluralForm(int count);
    }
}
