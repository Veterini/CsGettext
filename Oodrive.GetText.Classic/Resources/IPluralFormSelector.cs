using System.Collections.Generic;
using System.Globalization;

namespace Oodrive.GetText.Classic.Resources
{
    interface IPluralFormSelector
    {
        IReadOnlyList<CultureInfo> ApplicableCultures { get; }

        int GetPluralForm(int count);
    }
}
