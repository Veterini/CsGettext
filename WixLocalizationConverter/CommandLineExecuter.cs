using System;

namespace LocalizationFilesConverter
{
    internal class CommandLineExecuter : IOptionVisitor
    {
        private CommandLineExecuter()
        {
        }

        public static bool Execute(string[] args)
        {
            IOption subOption = null;
            var options = new Options();

            if (!CommandLine.Parser.Default.ParseArguments(args, options, (_, sub) => { subOption = sub as IOption; }))
            {
                return false;
            }

            try
            {
                return Execute(subOption);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool Execute(IOption option)
        {
            return option?.Accept(new CommandLineExecuter()) == true;
        }

        bool IOptionVisitor.Visit(Wxl2Po option)
        {
            return CommonVisit<Wxl2PoConverter>(option);
        }

        bool IOptionVisitor.Visit(Po2Wxl option)
        {
            return CommonVisit<Po2WxlConverter>(option);
        }

        private static bool CommonVisit<T>(AbstractOption option) where T : AbstractConverter, new()
        {
            var generator = new T
            {
                Input = option.Input,
                Output = option.Output
            };

            return generator.Execute();
        }
    }
}
