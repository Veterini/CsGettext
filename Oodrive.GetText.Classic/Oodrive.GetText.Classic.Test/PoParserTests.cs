using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Oodrive.GetText.Classic.Resources;

namespace Oodrive.GetText.Classic.Test
{
    [TestFixture]
    public class PoParserTests
    {
        [Test]
        public void TestSimplePoEntry()
        {
            var po =
@"#: kstarsinit.cpp:163
msgid ""Set Focus &Manually...""
msgstr ""フォーカスを手動でセット(&M)...""";

            TextReader reader = new StringReader(po);
            var dic = PoParser.ParseIntoDictionary(reader);
            dic.Count.Should().Be(1);
            dic.Keys.FirstOrDefault().Should().Be("Set Focus &Manually...");
            dic.Values.FirstOrDefault().Should().Be("フォーカスを手動でセット(&M)...");
        }

        [Test]
        public void TestMultipleEntries()
        {
            var po =
@"#: finddialog.cpp:38
msgid ""Globular Clusters""
msgstr """"

#: finddialog.cpp:39
msgid ""Gaseous Nebulae""
msgstr """"

#: finddialog.cpp:40
msgid ""Planetary Nebulae""
msgstr """"";

            TextReader reader = new StringReader(po);
            var dic = PoParser.ParseIntoDictionary(reader);
            dic.Count.Should().Be(3);

            dic.Keys.ShouldBeEquivalentTo(new [] { "Globular Clusters" , "Gaseous Nebulae" , "Planetary Nebulae" });
        }
    }
}
