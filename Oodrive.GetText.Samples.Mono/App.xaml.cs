using System.Globalization;
using System.Windows;
using Oodrive.GetText.Mono.Extensions;

namespace Oodrive.GetText.Samples.Mono
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            TextResources.AddResourceManager("mono", Localization.Strings.MonoPoResourceManager);
            TextResources.DefaultResourceManagerKey = "mono";
            Localization.Strings.Language = CultureInfo.GetCultureInfo("ja");
        }
    }
}
