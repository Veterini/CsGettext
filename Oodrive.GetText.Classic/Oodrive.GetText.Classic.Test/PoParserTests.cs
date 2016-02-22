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

        [Test]
        public void TestSimplePoEntryMultipleLines()
        {
            var po =
@"msgid """"
""No INDI devices(...)""
"" in the devices menu.""
msgstr """"
""Nema INDI uređaja (...)""
"" u meniju uređaja.""";

            TextReader reader = new StringReader(po);
            var dic = PoParser.ParseIntoDictionary(reader);
            dic.Count.Should().Be(1);
            dic.Keys.FirstOrDefault().Should().Be("No INDI devices(...) in the devices menu.");
            dic.Values.FirstOrDefault().Should().Be("Nema INDI uređaja (...) u meniju uređaja.");
        }



        [Test]
        public void TestMultipleSameEntriesWithContext()
        {
            var po =
@"msgctxt ""First letter in 'Scope'""
msgid ""S""
msgstr """"

#: skycomponents/horizoncomponent.cpp:429
msgctxt ""South""
msgid ""S""
msgstr """"";

            TextReader reader = new StringReader(po);
            var dic = PoParser.ParseIntoDictionary(reader);
            dic.Count.Should().Be(2);

            dic.Keys.ShouldBeEquivalentTo(new[] { "S_I18nContext_First letter in 'Scope'", "S_I18nContext_South" });
        }

        [Test]
        public void TestPluralMessages()
        {
            var po =
@"#: mainwindow.cpp:127
#, kde-format
msgid ""Time: % 1 second""
msgid_plural ""Time: %1 seconds""
msgstr[0] ""Czas: %1 sekunda""
msgstr[1] ""Czas: %1 sekundy""
msgstr[2] ""Czas: %1 sekund""";


            TextReader reader = new StringReader(po);
            var dic = PoParser.ParseIntoDictionary(reader);
            dic.Count.Should().Be(3);

            dic.Keys.ShouldBeEquivalentTo(new[] { "Time: % 1 second_I18nPluralForm_0", "Time: % 1 second_I18nPluralForm_1", "Time: % 1 second_I18nPluralForm_2" });
        }
    }
}
