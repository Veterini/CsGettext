using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Oodrive.GetText.Core.Po
{
    public abstract class PoEntryWriterBase : IPoEntryWriter
    {
        protected readonly StreamWriter Writer;

        protected PoEntryWriterBase(StreamWriter writer)
        {
            Writer = writer;
        }

        public abstract void Flush();

        public static int ChunkMaxSize { private get; set; } = 40;

        private static IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }

        protected static string CleanValue(string value)
        {
            var temp = value;
            temp = temp.Replace("\n", "\\n").Replace("\"", "\\\"");

            var result = new StringBuilder();
            foreach (var part in Split(temp, ChunkMaxSize))
            {
                result.Append('"');
                result.Append(part);
                result.Append('"');
                result.AppendLine();
            }

            return result.ToString();
        }
    }

    public class PoHeaderWriter : PoEntryWriterBase
    {
        public CultureInfo Language { get; set; }

        public PoHeaderWriter(StreamWriter writer) : base(writer)
        {
        }

        public override void Flush()
        {
            const string header = @"msgid """"
msgstr """"
""POT-Creation-Date: {0}\n""
""MIME-Version: 1.0\n""
""Content-Type: text/plain; charset=UTF-8\n""
""Content-Transfer-Encoding: 8bit\n""
""Language: {1}\n""
""Plural-Forms: nplurals={2}; plural={3};\n""
""X -Generator: Oodrive.GetText.Converter""";

            Writer.WriteLine(header,DateTime.UtcNow, Language.Name, NPlurals, PluralRule);
            Writer.WriteLine();
        }

        public int NPlurals { get; set; } = 1;

        public string PluralRule { get; set; } = "(n != 1)";
    }

    public class PoEntryWriter : PoEntryWriterBase
    {
        private readonly string _id;
        private readonly string _value;

        public PoEntryWriter(StreamWriter writer, string id, string value) : base(writer)
        {
            _id = id;
            _value = value;
        }

        public override void Flush()
        {
            Writer.Write("msgid ");
            Writer.WriteLine(CleanValue(_id));
            Writer.Write("msgstr ");
            Writer.WriteLine(CleanValue(_value));
            Writer.WriteLine();
        }
    }

    public class PoContextualEntryWriter : PoEntryWriter
    {
        private readonly string _context;

        public PoContextualEntryWriter(StreamWriter writer, string id, string context, string value) : base(writer, id, value)
        {
            _context = context;
        }

        public override void Flush()
        {
            Writer.Write("msgctxt ");
            Writer.WriteLine(CleanValue(_context));
            base.Flush();
        }
    }
}
