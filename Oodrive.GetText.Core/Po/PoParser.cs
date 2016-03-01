using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NString;

namespace Oodrive.GetText.Core.Po
{
    public class PoParser : IDisposable
    {
        private readonly TextReader _reader;

        private static ParserState _state;

        public PoParser(TextReader reader)
        {
            _reader = reader;
            _messages = new List<IPoEntry>();
        }

        public void Parse()
        {

            SetState(ParserState.WaitingKey);
            var isFuzzy = false;

            StringBuilder currentKey = null;
            StringBuilder currentValue = null;
            StringBuilder currentContext = null;
            StringBuilder currentPluralKey = null;
            IDictionary<int, StringBuilder> pluralValues = null;

            while (true)
            {
                var line = _reader.ReadLine();
                line = line?.Trim();
                if (line.IsNullOrEmpty())
                {
                    if (_state == ParserState.ConsumingValue && currentKey != null)
                    {
                        SetValuesForKeys(CleanString(currentKey.ToString()), isFuzzy, currentPluralKey, currentContext,
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
                    case LineType.PluralFormRule:
                        SetPluralFormRule(isFuzzy, CleanString(parsingResult.Value));
                        break;
                }
            }
        }

        private void SetValuesForKeys(string id, bool fuzzy, StringBuilder currentPluralKey, StringBuilder currentContext, StringBuilder currentValue, IDictionary<int, StringBuilder> pluralValues)
        {
            var hasContext = currentContext != null;
            var hasPlural = currentPluralKey != null && pluralValues != null;
            var pluralKey = currentPluralKey != null ? CleanString(currentPluralKey.ToString()) : string.Empty;

            var value = CleanString(currentValue?.ToString() ?? string.Empty);
            var ctxValue = CleanString(currentContext?.ToString() ?? string.Empty);

            if (!hasContext && !hasPlural)
            {
                var message = new PoEntry(id, value, fuzzy);
                _messages.Add(message);
                return;
            }

            if (hasContext && !hasPlural)
            {
                var message = new ContextualPoEntry(id, value, ctxValue, fuzzy);
                _messages.Add(message);
                return;
            }

            var plurals = new Dictionary<int,string>();
            foreach (var builder in pluralValues)
            {
                plurals[builder.Key] = builder.Value.ToString();
            }

            {
                var message = !hasContext
                    ? (IPoEntry)new PluralPoEntry(id, pluralKey, Header?.NPlurals ?? 2, plurals, fuzzy)
                    : new ContextualPluralPoEntry(id, pluralKey, ctxValue, Header?.NPlurals ?? 2,plurals,fuzzy);
                _messages.Add(message);
            }
        }

        private void SetPluralFormRule(bool isFuzzy, string rule)
        {
            var pluralForm = "n == 1";
            var nplurals = 2;
            if (!rule.IsNullOrEmpty() && rule.StartsWith("nplurals="))
            {
                var firstEqual = rule.IndexOf('=');
                var sep = rule.IndexOf(';');
                var npluralsString = rule.Substring(firstEqual + 1, sep - firstEqual - 1).Trim();
                nplurals = int.Parse(npluralsString);
                var firstIndex = rule.IndexOf('=', sep);
                var secondIndex = rule.IndexOf(';', firstIndex + 1);
                pluralForm = rule.Substring(firstIndex + 1, secondIndex - firstIndex - 1).Trim();
            }

            Header = new PoHeader(isFuzzy,nplurals,pluralForm);
        }

        private static LineParsingResult ParseLine(string line)
        {
            if (line[0] == '#')
            {
                if (line.Length <= 1) return new LineParsingResult(LineType.Comment);

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
                return value.StartsWith("Plural-Forms:") ? new LineParsingResult(LineType.PluralFormRule, value.Substring(13).Trim()) : new LineParsingResult(LineType.Multiline, value);
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

        public Dictionary<string, string> ToDictionary()
        {
            if(_messages.Count == 0)
                Parse();

            var dic = new Dictionary<string, string>();
            foreach (var message in Messages.Where(message => !message.IsFuzzy && !message.IsObselete))
            {
                message.Accept(dic);
            }
            return dic;
        } 

        private readonly List<IPoEntry> _messages;
        public IEnumerable<IPoEntry> Messages => _messages;

        public PoHeader Header { get; private set; }

        public void Dispose()
        {
            _reader?.Dispose();
        }

        private static string CleanString(string value)
        {
            return value.Replace("\\n", "\n").Replace("\\\"", "\"");
        }

        private static void SetState(ParserState state)
        {
            _state = state;
        }

        private enum LineType
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
            PreviousId,
            PluralFormRule
        }

        private enum ParserState
        {
            WaitingKey,
            ConsumingKey,
            ConsumingPluralKey,
            ConsumingValue,
            ConsumingContext,
        }

        private class LineParsingResult
        {
            internal LineType Type { get; }
            internal string Value { get; }
            internal int? PluralForm { get; }

            internal LineParsingResult(LineType type, string value = null, int? pluralForm = null)
            {
                Type = type;
                Value = value;
                PluralForm = pluralForm;
            }
        }
    }
}
