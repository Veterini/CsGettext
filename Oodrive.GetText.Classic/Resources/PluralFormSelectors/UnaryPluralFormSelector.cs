using System.Collections.Generic;
using System.Globalization;

namespace Oodrive.GetText.Classic.Resources.PluralFormSelectors
{
    class UnaryPluralFormSelector : IPluralFormSelector
    {
        public IReadOnlyList<CultureInfo> ApplicableCultures { get; } = new []
        {
            CultureInfo.GetCultureInfo("ja"),
            CultureInfo.GetCultureInfo("fa"),
            CultureInfo.GetCultureInfo("id"),
            CultureInfo.GetCultureInfo("ko"),
            CultureInfo.GetCultureInfo("vi"),
            CultureInfo.GetCultureInfo("zh"),
        };

        public int GetPluralForm(int count)
        {
            return 0;
        }
    }
}
