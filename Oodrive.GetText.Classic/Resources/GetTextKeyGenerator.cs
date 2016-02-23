namespace Oodrive.GetText.Classic.Resources
{
    internal static class GetTextKeyGenerator
    {
        internal static string GetContextKey(string key, string context)
        {
            return $"{key}_I18nContext_{context}";
        }

        internal static string GetPluralKey(string key, int form)
        {
            return $"{key}_I18nPluralForm_{form}";
        }

        internal static string GetPluralKeyAndContext(string key, int form, string context)
        {
            return $"{key}_I18nPluralForm_{form}_I18nContext_{context}";
        }
    }
}
