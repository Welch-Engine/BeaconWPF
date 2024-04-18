using System.Windows;
using static BeaconWPF.Startup;

namespace BeaconWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Init(); 
            System.Threading.Thread.Sleep(3000);
        }

    }
}
