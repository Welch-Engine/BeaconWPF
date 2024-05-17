using System.Windows;
using System.Windows.Interop;
using WpfScreenHelper;
using WpfScreenHelper.Enum;

namespace BeaconWPF
{
    /// <summary>
    /// Interaction logic for DisplayWindow.xaml
    /// </summary>
    public partial class DisplayWindow : Window
    {

        //! Singleton Insatance
        public static DisplayWindow Instance { get; private set; }
        static DisplayWindow() => Instance = new DisplayWindow();

        //! List of Screens
        private List<Screen> _screensList = new List<Screen>();

        public int SelectedScreen = 0;

        public DisplayWindow()
		{
			InitializeComponent();
        }

        //! ====================================================
        //! [+] WINDOW CLOSING: cancel the closing and set visibility to collapsed
        //! ====================================================
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
            e.Cancel = true;
        }

        //! ====================================================
        //! [+] SET WINDOW POSITION: using WPFScreenHelper so I don't have to use WinForms
        //! ====================================================
        private static void SetWindowPosition(Window wnd, WindowPositions pos, Rect bounds)
        {
            var x = 0.0d;
            var y = 0.0d;

            switch (pos)
            {
                case WindowPositions.Center:
                    x = bounds.X + (bounds.Width - wnd.Width) / 2.0;
                    y = bounds.Y + (bounds.Height - wnd.Height) / 2.0;
                    break;
                case WindowPositions.Left:
                    x = bounds.X;
                    y = bounds.Y + (bounds.Height - wnd.Height) / 2.0;
                    break;
                case WindowPositions.Top:
                    x = bounds.X + (bounds.Width - wnd.Width) / 2.0;
                    y = bounds.Y;
                    break;
                case WindowPositions.Right:
                    x = bounds.X + (bounds.Width - wnd.Width);
                    y = bounds.Y + (bounds.Height - wnd.Height) / 2.0;
                    break;
                case WindowPositions.Bottom:
                    x = bounds.X + (bounds.Width - wnd.Width) / 2.0;
                    y = bounds.Y + (bounds.Height - wnd.Height);
                    break;
                case WindowPositions.TopLeft:
                    x = bounds.X - 4;
                    y = bounds.Y - 1;
                    break;
                case WindowPositions.TopRight:
                    x = bounds.X + (bounds.Width - wnd.Width);
                    y = bounds.Y;
                    break;
                case WindowPositions.BottomRight:
                    x = bounds.X + (bounds.Width - wnd.Width);
                    y = bounds.Y + (bounds.Height - wnd.Height);
                    break;
                case WindowPositions.BottomLeft:
                    x = bounds.X;
                    y = bounds.Y + (bounds.Height - wnd.Height);
                    break;
            }

            wnd.Left = x;
            wnd.Top = y;
        }

        //! ====================================================
        //! [+] VISIBLE CHANGED
        //! ====================================================
        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) => SetWindow();

        private void SetWindow()
        {
            Screen.AllScreens.ToList().ForEach(screen => _screensList.Add(screen));

            // Set the window position depending on screenlist size, set the width adn height as well
            SetWindowPosition(this, WindowPositions.TopLeft, _screensList[SelectedScreen].Bounds);
            Width = _screensList[SelectedScreen].Bounds.Width + 8;
            Height = _screensList[SelectedScreen].Bounds.Height + 5;
        }

        //? =============================[LOADED & UNLOADED]==============================

        //private void Window_Loaded(object sender, RoutedEventArgs e) => MainWindow.CloseDisplayEvent += CloseDisplayMethod;

        //private void Window_Unloaded(object sender, RoutedEventArgs e) => MainWindow.CloseDisplayEvent -= CloseDisplayMethod;

        //!? ====================================================
        //!? DISABLES FOCUS ON THIS WINDOW
        //!? ====================================================
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var source = PresentationSource.FromVisual(this) as HwndSource;
            if(source is not null)
                source.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_MOUSEACTIVATE)
            {
                handled = true;
                return new IntPtr(MA_NOACTIVATE);
            }
            else return IntPtr.Zero;
        }
        private const int WM_MOUSEACTIVATE = 0x0021;
        private const int MA_NOACTIVATE = 0x0003;


    }
}
