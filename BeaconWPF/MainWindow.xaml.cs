using BeaconWPF.Data;
using BeaconWPF.Data.Helpers;
using System.Windows;
using System.Windows.Interop;

namespace BeaconWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            SourceInitialized += MainWindow_SourceInitialized;                                              // Initialize the complicated window maximize problem thingy
            Resources.Add("services", Startup.Services);

            if (!Constants.exists)
                System.IO.Directory.CreateDirectory(Constants.basePath);
            
            InitializeComponent();

            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;


            //!? ====================================================
            //!? INIT: Window Buttons
            //!? ====================================================
            HideButton.Click += (s, e) => WindowState = WindowState.Minimized;                              // EVENT: Hide App
            MinMaxButton.Click += (s, e) => WindowState = WindowState ==                                    // EVENT: Minimize if Max.
                            WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;             // Maximize if Min.
            CloseButton.Click += (s, e) => Close();                                                         // EVENT: Close App
        }

        #region Window Maximize Fix
        //! ====================================================
        //! [+] SOURCE INITIALIZE: should fix the maximizing issue
        //! ====================================================
        private void MainWindow_SourceInitialized(object? sender, EventArgs e)
        {
            IntPtr handle = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(handle)?.AddHook(WindowProc);
        }

        //! ====================================================
        //! [+] INTPTR WINDOWPROC: I have no idea :(
        //! ====================================================
        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x0024:
                    WindowMaximizeHelper.WmGetMinMaxInfo(hwnd, lParam, (int)MinWidth, (int)MinHeight);
                    handled = true;
                    break;
            }

            return (IntPtr)0;
        }
        #endregion
    }
}
