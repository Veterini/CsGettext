using System.IO;
using System.Text;
using System.Xml;
using Oodrive.GetText.Core.Po;

namespace LocalizationFilesConverter
{
    internal class Po2WxlConverter : AbstractConverter
    {
        public override bool Execute()
        {
            // Delete the file if it exists.
            if (File.Exists(Output))
            {
                File.Delete(Output);
            }

            using (var stream = File.Create(Output))
            using (var xmlWriter = new XmlTextWriter(stream, Encoding.UTF8))
            using (var parser = new PoParser(new StreamReader(Input)).Parse())
            {
                xmlWriter.Formatting = Formatting.Indented;
                xmlWriter.IndentChar = ' ';
                xmlWriter.Indentation = 4;
                // Root.
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("WixLocalization", "http://schemas.microsoft.com/wix/2006/localization");
                foreach (var message in parser.Messages)
                {
                    if(message.IsHeader)
                        xmlWriter.WriteAttributeString("Culture", parser.Header?.Language?.Name ?? "en-us");

                    var entry = message as PoEntry;
                    if (entry != null) AddEntry(xmlWriter, entry); //We only treat simple message for now
                }
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
            }

            return true;
        }

        private static void AddEntry(XmlWriter xmlWriter, IPoEntry entry)
        {
            if (entry.IsHeader || string.IsNullOrWhiteSpace(entry.Id)) return;

            xmlWriter.WriteStartElement("String");
            xmlWriter.WriteAttributeString("Id", entry.Id);
            xmlWriter.WriteString(entry.Value);
            xmlWriter.WriteEndElement();
        }
    }
}
