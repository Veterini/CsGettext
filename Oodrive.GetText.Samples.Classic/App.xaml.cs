using System.Globalization;
using System.Windows;
using Oodrive.GetText.Classic.Extensions;

namespace Oodrive.GetText.Samples.Classic
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            GetTextResources.AddResourceManager("classic", typeof(Localization.Strings));
            GetTextResources.DefaultResourceManagerKey = "classic";
            GetTextResources.Language = CultureInfo.GetCultureInfo("fr");
        }
    }
}
