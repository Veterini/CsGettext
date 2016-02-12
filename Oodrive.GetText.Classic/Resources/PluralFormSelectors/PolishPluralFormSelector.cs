using System.Collections.Generic;
using System.Globalization;

namespace Oodrive.GetText.Classic.Resources.PluralFormSelectors
{
    class PolishPluralFormSelector : IPluralFormSelector
    {
        public IReadOnlyList<CultureInfo> ApplicableCultures { get; } = new[] {CultureInfo.GetCultureInfo(1045)}; //Poland
        public int GetPluralForm(int n)
        {
            var plural = (n == 1 ? 0 : n % 10 >= 2 && n % 10 <= 4 && (n % 100 < 10 || n % 100 >= 20) ? 1 : 2);
            return plural;
        }
    }
}
