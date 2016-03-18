namespace LocalizationFilesConverter
{
    static class Program
    {
        private static void Main(string[] args)
        {
            var success = CommandLineExecuter.Execute(args);
            if (!success) System.Environment.Exit(160); // ERROR_BAD_ARGUMENTS
        }
    }
}
