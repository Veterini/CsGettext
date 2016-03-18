namespace LocalizationFilesConverter
{
    abstract class AbstractConverter
    {
        public string Input { protected get; set; }
        
        public string Output { protected get; set; }

        public abstract bool Execute();
    }
}
