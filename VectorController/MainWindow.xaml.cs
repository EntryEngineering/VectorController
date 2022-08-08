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
        BaseCanMessage baseCanMessage = new();

        public MainWindow()
        {
            InitializeComponent();
            Processor.CanBus can = new();
            can.test();


            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

            comboBoxOfMessageId.Items.Add("ALL");
            comboBoxOfMessageId.SelectedIndex = 0;
        }

        private void BackgroundWorker_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            PrintMessageToTextBox(baseCanMessage);
        }

        private void BackgroundWorker_ProgressChanged(object? sender, ProgressChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void BackgroundWorker_DoWork(object? sender, DoWorkEventArgs e)
        {
            baseCanMessage = canBus.GettempCanMessage();
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
            

            uint MessageIdConverted = Convert.ToUInt32(msgIdTxt.Text, 16);
            byte MessageDlcConverted = Convert.ToByte(msgDlc.Text, 16);

            byte MessageData0Converted = Convert.ToByte(msgData0Txt.Text, 16);
            byte MessageData1Converted = Convert.ToByte(msgData1Txt.Text, 16);
            byte MessageData2Converted = Convert.ToByte(msgData2Txt.Text, 16);
            byte MessageData3Converted = Convert.ToByte(msgData3Txt.Text, 16);
            byte MessageData4Converted = Convert.ToByte(msgData4Txt.Text, 16);
            byte MessageData5Converted = Convert.ToByte(msgData5Txt.Text, 16);
            byte MessageData6Converted = Convert.ToByte(msgData6Txt.Text, 16);
            byte MessageData7Converted = Convert.ToByte(msgData7Txt.Text, 16);

            canBus.TXProcess(MessageIdConverted, MessageDlcConverted, MessageData0Converted, MessageData1Converted, MessageData2Converted, MessageData3Converted, MessageData4Converted, MessageData5Converted, MessageData6Converted, MessageData7Converted);
        }



        private void PrintMessageToTextBox(BaseCanMessage message)
        {
            string canMessage = $"MsgID:{message.MessageId} - Data:{message.MessageValueHex}";
            rxTextBox.Text = canMessage;
        }

        private void startPrintingRxMsgToTextBox_Click(object sender, RoutedEventArgs e)
        {
            
        }



        private void btnSetSpecialMdgId_Click(object sender, RoutedEventArgs e)
        {
            comboBoxOfMessageId.Items.Clear();
            comboBoxOfMessageId.Items.Add("03C0");
            comboBoxOfMessageId.SelectedIndex = 0;
            if (!String.IsNullOrEmpty(comboBoxOfMessageId.SelectedItem.ToString()))
            {
                canBus.SetMessageIdFilter(comboBoxOfMessageId.SelectedItem.ToString());
            }
        }

    }
}
