namespace Oodrive.GetText.Core
{
    public static class GetTextKeyGenerator
    {
        public static string GetContextKey(string key, string context)
        {
            return $"{key}_I18nContext_{context}";
        }

        public static string GetPluralKey(string key, int form)
        {
            return $"{key}_I18nPluralForm_{form}";
        }

        public static string GetPluralKeyAndContext(string key, int form, string context)
        {
            return $"{key}_I18nPluralForm_{form}_I18nContext_{context}";
        }
    }
}
