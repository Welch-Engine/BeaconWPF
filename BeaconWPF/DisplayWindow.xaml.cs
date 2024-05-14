using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

        //!? ====================================================
        //!? DISABLES FOCUS ON THIS WINDOW
        //!? ====================================================
        //protected override void OnSourceInitialized(EventArgs e)
        //{
        //    base.OnSourceInitialized(e);
        //    var source = PresentationSource.FromVisual(this) as HwndSource;
        //    if(source is not null)
        //        source.AddHook(WndProc);
        //}

        //private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        //{
        //    if (msg == WM_MOUSEACTIVATE)
        //    {
        //        handled = true;
        //        return new IntPtr(MA_NOACTIVATE);
        //    }
        //    else return IntPtr.Zero;
        //}
        //private const int WM_MOUSEACTIVATE = 0x0021;
        //private const int MA_NOACTIVATE = 0x0003;

    }
}
