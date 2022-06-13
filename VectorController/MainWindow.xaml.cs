using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using VectorController.Messages;
using VectorController.Processor;
using vxlapi_NET;

namespace VectorController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly CanMessageProcessor canBus = new(XLDefine.XL_HardwareType.XL_HWTYPE_VN1610, "VectorController_v1");
        readonly BackgroundWorker backgroundWorker = new();


        public MainWindow()
        {
            InitializeComponent();

            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

            comboBoxOfMessageId.Items.Add("ALL");
            comboBoxOfMessageId.SelectedIndex = 0;
        }

        private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            canBus.StopReceive();
        }

        private void startCan_Click(object sender, RoutedEventArgs e)
        {
            canBus.StartReceive();
            Trace.WriteLine("CANBUS RX start");

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
            //canBus.TXProcess(0x100,8,1,2,3,4,5,6,7,8);
        }


        
        private void PrintMessageToTextBox()
        {

            BaseCanMessage message = canBus.GettempCanMessage();

            string canMessage = $"MsgID:{message.MessageId} - Data:{message.MessageValueHex}";
            rxTextBox.Text = canMessage;
        }

        private void startPrintingRxMsgToTextBox_Click(object sender, RoutedEventArgs e)
        {
            PrintMessageToTextBox();
        }
    }
}
