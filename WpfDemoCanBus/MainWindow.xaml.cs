using System.Threading;
using System.Windows;
using VectorBusLibrary.Processors;
using vxlapi_NET;

namespace WpfDemoCanBus
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CanBus bus;
        internal Thread testThread;

        public MainWindow()
        {
            InitializeComponent();
            bus = new(new XLDriver(), XLDefine.XL_HardwareType.XL_HWTYPE_VN1610);
            bus.TestCanBus();



        }

        private void toggleBtn_Checked(object sender, RoutedEventArgs e)
        {

            ZASKI15_stateTxt.Text = "on";
            testThread = new Thread(new ThreadStart(TxOn));
            testThread.Start();
        }

        private void toggleBtn_Unchecked(object sender, RoutedEventArgs e)
        {
            if (testThread.ThreadState == ThreadState.Running)
            {
                testThread.Abort();
            }
            testThread = new Thread(new ThreadStart(TxOff));
            ZASKI15_stateTxt.Text = "off";
            testThread.Start();
        }


        internal static void TxOn()
        {
            CanBus bus = new(new XLDriver(), XLDefine.XL_HardwareType.XL_HWTYPE_VN1610);
            for (int i = 0; i < 20000; i++)
            {
                bus.CanTransmit(2);
            }
        }

        internal static void TxOff()
        {
            CanBus bus = new(new XLDriver(), XLDefine.XL_HardwareType.XL_HWTYPE_VN1610);
            for (int i = 0; i < 20000; i++)
            {
                bus.CanTransmit(0);
            }
        }




    }
}
