using System;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Oodrive.GetText.Core.Po;

namespace LocalizationFilesConverter
{
    internal class Wxl2PoConverter : AbstractConverter
    {
        public Wxl2PoConverter()
        {
        }

        public override bool Execute()
        {
            using (var s = new XmlTextReader(Input))
            using (var stream = new StreamWriter(Output))
            using (var writer = new PoWriter(stream))
            {
                var ser = new XmlSerializer(typeof (WixLocalizationNode));
                try
                {
                    var root = (WixLocalizationNode) ser.Deserialize(s);
                    writer.Header.Language = CultureInfo.GetCultureInfo(root.Culture);
                    writer.Header.PluralRule = "n > 1"; // TODO from culture get the rule
                    writer.Header.NPlurals = 2; // TODO from culture get the rule

                    foreach (var item in root.Items)
                    {
                        writer.AddNormalEntry(item.Id,item.Value);
                    }
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
