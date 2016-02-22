using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NString;

namespace Oodrive.GetText.Classic.Resources
{
    public static class PoParser
    {
        private static ParserState _state;

        /// <summary>
        /// Parses an input po file.
        /// </summary>
        private static void Parse(TextReader reader, IDictionary<string, string> dic)
        {
            SetState(ParserState.WaitingKey);
            var isFuzzy = false;

            StringBuilder currentKey = null;
            StringBuilder currentValue = null;
            StringBuilder currentContext = null;
            StringBuilder currentPluralKey = null;
            IDictionary<int,StringBuilder> pluralValues = null;

            while (true)
            {
                var line = reader.ReadLine();
                line = line?.Trim();
                if (line.IsNullOrEmpty())
                {
                    if (_state == ParserState.ConsumingValue && !isFuzzy && currentKey != null)
                    {
                        SetValuesForKeys(dic, CleanString(currentKey.ToString()), currentPluralKey, currentContext,
                            currentValue, pluralValues);

                        currentKey = null;
                        currentValue = null;
                        currentContext = null;
                        currentPluralKey = null;
                        pluralValues = null;
                    }

                    if (line == null)
                        break;

                    SetState(ParserState.WaitingKey);
                    isFuzzy = false;
                    continue;
                }


                var parsingResult = ParseLine(line);
                switch (parsingResult?.Type)
                {
                    case LineType.Fuzzy:
                        isFuzzy = true;
                        continue;
                    case LineType.Comment:
                        continue;
                    case LineType.Key:
                        SetState(ParserState.ConsumingKey);
                        currentKey = new StringBuilder();
                        currentKey.Append(parsingResult.Value);
                        break;
                    case LineType.Value:
                        SetState(ParserState.ConsumingValue);
                        currentValue = new StringBuilder();
                        currentValue.Append(parsingResult.Value);
                        if (pluralValues != null && parsingResult.PluralForm.HasValue)
                            pluralValues[parsingResult.PluralForm.Value] = currentValue;
                        break;
                    case LineType.PluralKey:
                        SetState(ParserState.ConsumingPluralKey);
                        pluralValues = new Dictionary<int, StringBuilder>();
                        currentPluralKey = new StringBuilder();
                        currentPluralKey.Append(parsingResult.Value);
                        break;
                    case LineType.Context:
                        SetState(ParserState.ConsumingContext);
                        currentContext = new StringBuilder();
                        currentContext.Append(parsingResult.Value);
                        break;
                    case LineType.Multiline:
                        switch (_state)
                        {
                            case ParserState.ConsumingKey:
                                currentKey?.Append(parsingResult.Value);
                                break;
                            case ParserState.ConsumingPluralKey:
                                currentPluralKey?.Append(parsingResult.Value);
                                break;
                            case ParserState.ConsumingValue:
                                currentValue?.Append(parsingResult.Value);
                                break;
                            case ParserState.ConsumingContext:
                                currentContext?.Append(parsingResult.Value);
                                break;
                        }
                        break;
                }
            }
        }

        private static void SetInDictionary(IDictionary<string, string> dic, string key, string value)
        {
            string here;
            if(dic.TryGetValue(key, out here))
                throw new ArgumentOutOfRangeException(nameof(key),"This key has already been added, the po file is illformed");

            dic[key] = value;
        }

        private static void SetValuesForKeys(IDictionary<string, string> dic, string key, StringBuilder currentPluralKey,
            StringBuilder currentContext, StringBuilder currentValue, IDictionary<int, StringBuilder> pluralValues)
        {
            var hasContext = currentContext != null;
            var hasPlural = currentPluralKey != null && pluralValues != null;

            var value = CleanString(currentValue?.ToString() ?? string.Empty);
            var ctxValue = CleanString(currentContext?.ToString() ?? string.Empty);

            if (!hasContext && !hasPlural)
            {
                SetInDictionary(dic,key,value);
                return;
            }

            if (hasContext && !hasPlural)
            {
                var ctxKey = $"{key}_I18nContext_{ctxValue}";
                SetInDictionary(dic, ctxKey,value);
                return;
            }

            foreach (var entry in pluralValues)
            {
                var ctxKey = !hasContext
                    ? $"{key}_I18nPluralForm_{entry.Key}"
                    : $"{key}_I18nPluralForm_{entry.Key}_I18nContext_{ctxValue}";
                SetInDictionary(dic, ctxKey, CleanString(entry.Value.ToString()));
            }
        }

        private static LineParsingResult ParseLine(string line)
        {
            if (line[0] == '#')
            {
                switch (line[1])
                {
                    case ',':
                        return line.Contains("fuzzy") ? new LineParsingResult(LineType.Fuzzy) : new LineParsingResult(LineType.Flag, line.Substring(2).Trim());
                    case ':':
                        return new LineParsingResult(LineType.Position, line.Substring(2).Trim());
                    case '.':
                        return new LineParsingResult(LineType.Comment, line.Substring(2).Trim());
                    case '|':
                        return new LineParsingResult(LineType.PreviousId, line.Substring(2).Trim());
                    case '~':
                        return new LineParsingResult(LineType.Obsolete);
                    default:
                        return new LineParsingResult(LineType.Comment);
                }
            }

            string value;
            if (line[0] == '"')
            {
                value = line[line.Length - 1] == '"' ? line.Substring(1, line.Length - 2) : line.Substring(1, line.Length - 1);
                return new LineParsingResult(LineType.Multiline, value);
            }

            value = ExtractLineValue(line);

            if (line.StartsWith("msgid "))
                return new LineParsingResult(LineType.Key, value);

            if (line.StartsWith("msgstr "))
                return new LineParsingResult(LineType.Value, value);

            if (line.StartsWith("msgctxt "))
                return new LineParsingResult(LineType.Context, value);

            if (line.StartsWith("msgid_plural "))
                return new LineParsingResult(LineType.PluralKey, value);

            if (line.StartsWith("msgstr["))
                return new LineParsingResult(LineType.Value, value, ExtractPluralCount(line));

            return new LineParsingResult(LineType.Undefined);
        }

        private static int ExtractPluralCount(string line)
        {
            var open = line.IndexOf('[');
            var close = line.IndexOf(']');
            if (close == -1) throw new ArgumentException(nameof(line));
            return int.Parse(line.Substring(open + 1, close - open - 1));
        }

        private static string ExtractLineValue(string line)
        {
            var firstQuote = line.IndexOf('"');
            if (firstQuote == -1)
                return string.Empty;

            var secondQuote = line.IndexOf('"', firstQuote + 1);
            while (secondQuote != -1 && line[secondQuote - 1] == '\\')
                secondQuote = line.IndexOf('"', secondQuote + 1);

            return secondQuote == -1 ? string.Empty : line.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
        }

        /// <summary>
        /// Parses an input po file into a dictionary.
        /// </summary>
        public static Dictionary<string, string> ParseIntoDictionary(TextReader reader)
        {
            var dic = new Dictionary<string, string>();
            Parse(reader, dic);
            return dic;
        }

        private static string CleanString(string value)
        {
            return value.Replace("\\n", "\n").Replace("\\\"", "\"");
        }

        private static void SetState(ParserState state)
        {
            _state = state;
        }

        enum LineType
        {
            Key,
            Value,
            PluralKey,
            Comment,
            Undefined,
            Fuzzy,
            Context,
            Position,
            Multiline,
            Flag,
            Obsolete,
            PreviousId
        }

        enum ParserState
        {
            WaitingKey,
            ConsumingKey,
            ConsumingPluralKey,
            ConsumingValue,
            ConsumingContext,
        }

        class LineParsingResult
        {
            public LineType Type { get; }
            public string Value { get; }
            public int? PluralForm { get; }

            public LineParsingResult(LineType type, string value = null, int? pluralForm = null)
            {
                Type = type;
                Value = value;
                PluralForm = pluralForm;
            }
        }
    }
}