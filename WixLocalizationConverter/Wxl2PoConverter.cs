using System;
using System.Xml;
using System.Xml.Serialization;

namespace LocalizationFilesConverter
{
    internal class Wxl2PoConverter : AbstractConverter
    {
        public Wxl2PoConverter()
        {
        }

        public override bool Execute()
        {
            var s = new XmlTextReader(Input);
            var ser = new XmlSerializer(typeof(WixLocalizationNode));
            WixLocalizationNode root;
            try
            {
                root = (WixLocalizationNode)ser.Deserialize(s);
            }
            catch (InvalidOperationException e)
            {
                return false;
            }

            //TODO

            return true;
        }
    }
}
