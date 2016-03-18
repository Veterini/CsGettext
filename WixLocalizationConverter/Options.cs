using CommandLine;
using CommandLine.Text;

namespace LocalizationFilesConverter
{
    internal class Options
    {
        [VerbOption("wxl2po", HelpText = "Generate from a wxl file a mono-lingual po file")]
        public Wxl2Po Wxl2Po { get; set; }

        [VerbOption("po2wxl", HelpText = "Generate from a mono-lingual po file a wxl file")]
        public Po2Wxl Po2Wxl { get; set; }

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }

        [HelpVerbOption]
        public string GetUsage(string verb)
        {
            return HelpText.AutoBuild(this, verb);
        }
    }

    internal interface IOption
    {
        bool Accept(IOptionVisitor visitor);
    }

    internal interface IOptionVisitor
    {
        bool Visit(Wxl2Po option);
        bool Visit(Po2Wxl option);
    }

    internal abstract class AbstractOption : IOption
    {
        [Option('i', "input", Required = true, HelpText = "Input file")]
        public string Input { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file")]
        public string Output { get; set; }

        public abstract bool Accept(IOptionVisitor visitor);
    }


    internal class Wxl2Po : AbstractOption
    {
        public override bool Accept(IOptionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }


    internal class Po2Wxl : AbstractOption
    {
        public override bool Accept(IOptionVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}