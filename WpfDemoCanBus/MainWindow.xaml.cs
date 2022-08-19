using System.Diagnostics;
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


        public MainWindow()
        {
            InitializeComponent();
            CanBus bus = new(new XLDriver(), XLDefine.XL_HardwareType.XL_HWTYPE_VN1610);
            Trace.WriteLine(bus.GetDLLVesrion());

        }
    }
}
