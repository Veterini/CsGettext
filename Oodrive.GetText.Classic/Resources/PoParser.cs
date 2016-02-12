using System.Collections.Generic;
using System.IO;
using System.Text;
using NString;

namespace Oodrive.GetText.Classic.Resources
{
    public static class PoParser
    {
        /// <summary>
        /// Parses an input po file.
        /// </summary>
        private static void Parse(TextReader reader, IDictionary<string, string> dic)
        {
            const int stateWaitingKey = 1;
            const int stateConsumingKey = 2;
            const int stateConsumingValues = 3;

            var state = stateWaitingKey;
            var isFuzzy = false;

            StringBuilder currentKey = null;
            StringBuilder currentValue = null;

            while (true)
            {
                var line = reader.ReadLine();
                line = line?.Trim();
                if (line.IsNullOrEmpty())
                {
                    if (state == stateConsumingValues &&
                        currentKey != null &&
                        currentValue != null &&
                        !isFuzzy)
                    {
                        dic[CleanString(currentKey.ToString())] = CleanString(currentValue.ToString());
                        currentKey = null;
                        currentValue = null;
                    }

                    if (line == null)
                        break;

                    state = stateWaitingKey;
                    isFuzzy = false;
                    continue;
                }


                var parsingResult = ParseLine(line);

                if (parsingResult?.Type == LineType.Fuzzy)
                {
                    isFuzzy = true;
                    continue;
                }

                if (parsingResult?.Type == LineType.Comment)
                {
                    continue;
                }

                var isMsgId = line.StartsWith("msgid ");
                var isMsgStr = !isMsgId && line.StartsWith("msgstr ");

                if (isMsgId || isMsgStr)
                {
                    state = isMsgId ? stateConsumingKey : stateConsumingValues;

                    var firstQuote = line.IndexOf('"');
                    if (firstQuote == -1)
                        continue;

                    var secondQuote = line.IndexOf('"', firstQuote + 1);
                    while (secondQuote != -1 && line[secondQuote - 1] == '\\')
                        secondQuote = line.IndexOf('"', secondQuote + 1);
                    if (secondQuote == -1)
                        continue;

                    var piece = line.Substring(firstQuote + 1, secondQuote - firstQuote - 1);

                    if (isMsgId)
                    {
                        currentKey = new StringBuilder();
                        currentKey.Append(piece);
                    }
                    else
                    {
                        currentValue = new StringBuilder();
                        currentValue.Append(piece);
                    }
                }
                else if (line[0] == '"')
                {
                    line = line[line.Length - 1] == '"' ? line.Substring(1, line.Length - 2) : line.Substring(1, line.Length - 1);

                    switch (state)
                    {
                        case stateConsumingKey:
                            currentKey?.Append(line);
                            break;
                        case stateConsumingValues:
                            currentValue?.Append(line);
                            break;
                    }
                }
            }
        }

        private static LineParsingResult ParseLine(string line)
        {
            if (line[0] == '#')
            {
                switch (line[1])
                {
                    case ',':
                        return line.Contains("fuzzy") ? new LineParsingResult(LineType.Fuzzy) : new LineParsingResult(LineType.Comment,line.Substring(2).Trim());
                    case ':':
                        return new LineParsingResult(LineType.Position, line.Substring(2).Trim());
                    default:
                        return new LineParsingResult(LineType.Comment);
                }
            }

            if (line.StartsWith("msgid "))
            {
                var value = ExtractLineValue(line);
                return new LineParsingResult(LineType.Key,value);
            }

            return null;
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

        enum LineType
        {
            Key,
            Value,
            PluralKey,
            Comment,
            Undefined,
            Fuzzy,
            Context,
            Position
        }

        class LineParsingResult
        {
            public LineType Type { get; }
            public string Value { get; }
            public int? Count { get; }

            public LineParsingResult(LineType type, string value = null, int? count = null)
            {
                Type = type;
                Value = value;
                Count = count;
            }
        }
           
    }
}