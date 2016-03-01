using System.Collections.Generic;
using System.IO;
using System.Linq;
using Oodrive.GetText.Core.Po;

namespace Oodrive.GetText.Classic.Resources
{
    public static class PoDictionary
    {
        public static Dictionary<string, string> ParseIntoDictionary(TextReader reader)
        {
            var dic = new Dictionary<string, string>();
            var parser = new PoParser(reader);
            parser.Parse();

            foreach (var message in parser.Messages.Where(message => !message.IsFuzzy && !message.IsObselete))
            {
                message.Accept(dic);
            }

            return dic;
        }
    }
}