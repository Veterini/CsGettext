using System.Collections.Generic;
using System.Globalization;

namespace Oodrive.GetText.Core.PluralFormSelectors
{
    public class BinaryPluralFormSelector : IPluralFormSelector
    {
        public IReadOnlyList<CultureInfo> ApplicableCultures { get; } = new[]
        {CultureInfo.GetCultureInfo("fr"), CultureInfo.GetCultureInfo("tr"),};
        public int GetPluralForm(int count)
        {
            return count > 1 ? 1 : 0;
        }
    }
}
