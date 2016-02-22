using System.Collections.Generic;
using System.Globalization;

namespace Oodrive.GetText.Classic.Resources.PluralFormSelectors
{
    class BinaryPluralFormSelector : IPluralFormSelector
    {
        public IReadOnlyList<CultureInfo> ApplicableCultures { get; } = new[]
        {CultureInfo.GetCultureInfo("fr"), CultureInfo.GetCultureInfo("tr"),};
        public int GetPluralForm(int count)
        {
            return count > 1 ? 1 : 0;
        }
    }
}
