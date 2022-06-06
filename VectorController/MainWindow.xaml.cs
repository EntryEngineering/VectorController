using System;
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

        CanMessageProcessor canBus = new(XLDefine.XL_HardwareType.XL_HWTYPE_VN1610, "VectorController_v1");
        public MainWindow()
        {
            InitializeComponent();
            comboBoxOfMessageId.Items.Add("ALL");
            comboBoxOfMessageId.SelectedIndex = 0;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            canBus.StopReceive();
        }

        private void startCan_Click(object sender, RoutedEventArgs e)
        {
            canBus.StartReceive();
        }


        private void stopCan_Click(object sender, RoutedEventArgs e)
        {
            canBus.StopReceive();
        }

        private void btnGetListOfMsgId_Click(object sender, RoutedEventArgs e)
        {
            comboBoxOfMessageId.Items.Clear();
            comboBoxOfMessageId.ItemsSource = canBus.GetListOfMsgId();
            comboBoxOfMessageId.SelectedIndex = 0;
        }

        private void btnSetMsgIdFilter_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(comboBoxOfMessageId.SelectedItem.ToString()))
            {
                canBus.SetMessageIdFilter(comboBoxOfMessageId.SelectedItem.ToString());
            }
        }

        private void sendMessageBtn_Click(object sender, RoutedEventArgs e)
        {
            canBus.TXProcess(0x100,8,1,2,3,4,5,6,7,8);
        }
    }
}
