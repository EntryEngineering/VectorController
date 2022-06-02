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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            comboBoxOfMessageId.ItemsSource = canBus.GetListOfMsgId();
            comboBoxOfMessageId.SelectedIndex = 0;
        }

        private void startCan_Click(object sender, RoutedEventArgs e)
        {
            canBus.StartReceive();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            canBus.StopReceive();
        }

        private void stopCan_Click(object sender, RoutedEventArgs e)
        {
            canBus.StopReceive();
        }

        private void btnGetListOfMsgId_Click(object sender, RoutedEventArgs e)
        {
            comboBoxOfMessageId.ItemsSource = canBus.GetListOfMsgId();
        }

        private void btnSetMsgIdFilter_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(comboBoxOfMessageId.SelectedItem.ToString()))
            {
                canBus.SetMessageIdFilter(comboBoxOfMessageId.SelectedItem.ToString());
            }
        }

        internal void ShowCurrentMessage(string text)
        {
            textBoxOutputForMessage.Text = text;
        }
    }
}
