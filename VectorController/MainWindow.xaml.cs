using System.Windows;
using VectorController.Processor;
using vxlapi_NET;

namespace VectorController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            //InitializeComponent();

            CanBus canBusNew = new CanBus(new XLDriver(), XLDefine.XL_HardwareType.XL_HWTYPE_VN1610);
            canBusNew.TestCanBus();

            //CanFdBus canFdBus = new CanFdBus(new XLDriver(), XLDefine.XL_HardwareType.XL_HWTYPE_VN1610);
            //canFdBus.TestCanFDBus();

        }
    }
}
