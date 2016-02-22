using System.Collections.Generic;
using System.Globalization;

namespace Oodrive.GetText.Classic.Resources.PluralFormSelectors
{
    class SingularPluralFormSelector : IPluralFormSelector
    {
        public IReadOnlyList<CultureInfo> ApplicableCultures { get; } = new[]
        {
            CultureInfo.GetCultureInfo("en"),
            CultureInfo.GetCultureInfo("da"),
            CultureInfo.GetCultureInfo("de"),
            CultureInfo.GetCultureInfo("el"),
            CultureInfo.GetCultureInfo("es"),
            CultureInfo.GetCultureInfo("it"),
            CultureInfo.GetCultureInfo("nl"),
            CultureInfo.GetCultureInfo("pt"),
            CultureInfo.GetCultureInfo("sv"),
        };
        public int GetPluralForm(int count)
        {
            return count == 1 ? 0 : 1;
        }
    }
}
