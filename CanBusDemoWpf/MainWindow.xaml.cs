using System;
using System.Diagnostics;
using System.Windows;
using VectorBusLibrary.Processors;
using vxlapi_NET;
using static vxlapi_NET.XLDefine;

namespace CanBusDemoWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static CanBus? canBus = null;
        System.Timers.Timer rxTimer;
        System.Timers.Timer txTimer;
        XLClass.xl_event_collection xlEventCollection;
        


        public MainWindow()
        {
            InitializeComponent();
            //Helelper.SetLogoToWindow(logoEntry);
            
            InitVector();
        }

        private void InitVector()
        {
            // Init driver
            string appName = txtBoxAppName.Text;
            if (string.IsNullOrEmpty(appName))
            {
                MessageBox.Show("Please enter app name");
            }
            else
            {
                canBus = new(new XLDriver(), XLDefine.XL_HardwareType.XL_HWTYPE_VN1610, appName);
                Helper.WriteLogToTextBox($"Init driver: OK", txtBoxLogApp);

            }
            // Open driver
            Helper.WriteLogToTextBox($"Open driver: {canBus.OpenDriver()}", txtBoxLogApp);

            // Open port
            Helper.WriteLogToTextBox($"Get driver config: {canBus.GetDriverConfig()}", txtBoxLogApp);
            canBus.GetAppConfigAndSetAppConfig();

            Helper.WriteLogToTextBox($"Get app config and set appConfig: OK", txtBoxLogApp);
            canBus.RequestTheUserToAssignChannels();

            CommonVector.GetAccesMask();
            Helper.WriteLogToTextBox($"Get acces mask: OK", txtBoxLogApp);

            Helper.WriteLogToTextBox($"Open port: {canBus.OpenPort()}", txtBoxLogApp);
            Helper.WriteLogToTextBox($"Activate channel: {canBus.ActivateChannel()}", txtBoxLogApp);
            Helper.WriteLogToTextBox($"Set notification CanBus: {canBus.SetNotificationCanBus()}", txtBoxLogApp);
            Helper.WriteLogToTextBox($"Reset clock: {canBus.ResetClock()}", txtBoxLogApp);
        }

        // Button - Init driver
        private void btnIntitDriver_Click(object sender, RoutedEventArgs e)
        {
            string appName = txtBoxAppName.Text;
            if (string.IsNullOrEmpty(appName))
            {
                MessageBox.Show("Please enter app name");
            }
            else
            {
                canBus = new(new XLDriver(), XLDefine.XL_HardwareType.XL_HWTYPE_VN1610, appName);
                Helper.WriteLogToTextBox($"Init driver: OK", txtBoxLogApp);

            }
        }

        // Button - Open driver
        private void btnOpenDriver_Click(object sender, RoutedEventArgs e)
        {

            Helper.WriteLogToTextBox($"Open driver: {canBus.OpenDriver()}", txtBoxLogApp);
        }

        // Button - Open port
        private void btnOpenPort_Click(object sender, RoutedEventArgs e)
        {
            Helper.WriteLogToTextBox($"Get driver config: {canBus.GetDriverConfig()}", txtBoxLogApp);
            canBus.GetAppConfigAndSetAppConfig();

            Helper.WriteLogToTextBox($"Get app config and set appConfig: OK", txtBoxLogApp);
            canBus.RequestTheUserToAssignChannels();

            CommonVector.GetAccesMask();
            Helper.WriteLogToTextBox($"Get acces mask: OK", txtBoxLogApp);

            Helper.WriteLogToTextBox($"Open port: {canBus.OpenPort()}", txtBoxLogApp);
            Helper.WriteLogToTextBox($"Activate channel: {canBus.ActivateChannel()}", txtBoxLogApp);
            Helper.WriteLogToTextBox($"Set notification CanBus: {canBus.SetNotificationCanBus()}", txtBoxLogApp);
            Helper.WriteLogToTextBox($"Reset clock: {canBus.ResetClock()}", txtBoxLogApp);


        }

        // Button - Rx START
        private async void btnStartRx_Click(object sender, RoutedEventArgs e)
        {
            Helper.WriteLogToTextBox($"Rx start", txtBoxLogApp);

            if (canBus != null)
            {
                rxTimer = new System.Timers.Timer(200);
                rxTimer.Elapsed += RxTimer_Elapsed;
                rxTimer.AutoReset = true;
                rxTimer.Enabled = true;
                rxTimer.Start();
            }
        }

        // Timer event for RX
        private void RxTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            //string temp = canBus.RxAsync();
            //txtBoxReceiveMsg.Text = temp;
        }

        
        // Button - Tx single message
        private void btnTransmitSingle_Click(object sender, RoutedEventArgs e)
        {
            

            //XLClass.xl_event_collection xlEventCollection = new XLClass.xl_event_collection(1);
            //xlEventCollection.xlEvent[0].tagData.can_Msg.id = Convert.ToUInt32(txtBoxMsgId.Text, 16);
            //xlEventCollection.xlEvent[0].tagData.can_Msg.dlc = ushort.Parse(txtBoxDlc.Text);
            //xlEventCollection.xlEvent[0].tagData.can_Msg.data[0] = byte.Parse(txtBoxData0.Text);
            //xlEventCollection.xlEvent[0].tagData.can_Msg.data[1] = byte.Parse(txtBoxData1.Text);
            //xlEventCollection.xlEvent[0].tagData.can_Msg.data[2] = byte.Parse(txtBoxData2.Text);
            //xlEventCollection.xlEvent[0].tagData.can_Msg.data[3] = byte.Parse(txtBoxData3.Text);
            //xlEventCollection.xlEvent[0].tagData.can_Msg.data[4] = byte.Parse(txtBoxData4.Text);
            //xlEventCollection.xlEvent[0].tagData.can_Msg.data[5] = byte.Parse(txtBoxData5.Text);
            //xlEventCollection.xlEvent[0].tagData.can_Msg.data[6] = byte.Parse(txtBoxData6.Text);
            //xlEventCollection.xlEvent[0].tagData.can_Msg.data[7] = byte.Parse(txtBoxData7.Text);
            //xlEventCollection.xlEvent[0].tag = XL_EventTags.XL_TRANSMIT_MSG;

            //XL_Status status = canBus.CanTransmit(xlEventCollection);

            //Helper.WriteLogToTextBox($"Message[msgId:{txtBoxMsgId.Text} DLC:{txtBoxDlc.Text} data[0]:{txtBoxData0.Text} data[1]:{txtBoxData1.Text} data[2]:{txtBoxData2.Text} data[3]:{txtBoxData3.Text} data[4]:{txtBoxData4.Text} data[5]:{txtBoxData5.Text} data[6]:{txtBoxData6.Text} data[7]:{txtBoxData7.Text}] - {status}", txtBoxLogApp);

        }


        // asi odebrat
        private void TxMessageInit(bool enabled = false,long interval = 100)
        {
            xlEventCollection = new XLClass.xl_event_collection(1);


            xlEventCollection.xlEvent[0].tagData.can_Msg.id = Convert.ToUInt32(textBoxMessageId.Text, 16);
            xlEventCollection.xlEvent[0].tagData.can_Msg.dlc = ushort.Parse(textBoxDlc.Text);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[0] = Convert.ToByte(txtBoxByte0Hex.Text,16);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[1] = Convert.ToByte(txtBoxByte1Hex.Text, 16);
            //xlEventCollection.xlEvent[0].tagData.can_Msg.data[2] = byte.Parse(textByte2.Text);
            //xlEventCollection.xlEvent[0].tagData.can_Msg.data[3] = byte.Parse(textByte3.Text);
            //xlEventCollection.xlEvent[0].tagData.can_Msg.data[4] = byte.Parse(textByte4.Text);
            //xlEventCollection.xlEvent[0].tagData.can_Msg.data[5] = byte.Parse(textByte5.Text);
            //xlEventCollection.xlEvent[0].tagData.can_Msg.data[6] = byte.Parse(textByte6.Text);
            //xlEventCollection.xlEvent[0].tagData.can_Msg.data[7] = byte.Parse(textByte7.Text);
            xlEventCollection.xlEvent[0].tag = XL_EventTags.XL_TRANSMIT_MSG;

            Trace.WriteLine(xlEventCollection.xlEvent[0].tagData.can_Msg.data[0]);




            txTimer = new System.Timers.Timer();
            txTimer.Elapsed += TimerForTx_Elapsed;
            txTimer.AutoReset = true;
            txTimer.Interval = interval;
            txTimer.Enabled = enabled;
        }



        private void TimerForTx_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Trace.WriteLine(e.SignalTime.ToString());
            XL_Status status = canBus.CanTransmit(xlEventCollection);
            //Helper.WriteLogToTextBox(status.ToString(), txtBoxLogApp);

        }

        private void checkBoxTrnasmitMessageInLoop_Checked(object sender, RoutedEventArgs e)
        {
            StartTxLoop();
        }

        private void checkBoxTrnasmitMessageInLoop_Unchecked(object sender, RoutedEventArgs e)
        {
            StopTxLoop();
        }

        private void StartTxLoop()
        {
            long cycleTime = long.Parse(txtBoxCycleTime.Text);
            TxMessageInit(true, cycleTime);
            txTimer.Start();
            string ourLogInfo = $"Tx start with Cycle time: {cycleTime}ms";

            Trace.WriteLine(ourLogInfo);
            Helper.WriteLogToTextBox(ourLogInfo, txtBoxLogApp);
        }


        private void StopTxLoop()
        {
            txTimer.Stop();
            Trace.WriteLine($"Tx stop");
        }


        private void RestartTxLoop()
        {
            StopTxLoop();
            StartTxLoop();

        }

        //private void toggleBtn_Checked(object sender, RoutedEventArgs e)
        //{
        //    if (txTimer != null)
        //    {
        //        txtBoxData2.Text = "2";
        //        RestartTxLoop();
        //        checkBoxTrnasmitMessageInLoop.IsEnabled = false;
        //    }
        //    else
        //    {
        //        txtBoxData2.Text = "2";
        //        StartTxLoop();
        //        checkBoxTrnasmitMessageInLoop.IsEnabled = false;  
        //    }

        //}

        //private void toggleBtn_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    txtBoxData2.Text = "0";
        //    RestartTxLoop();
        //    checkBoxTrnasmitMessageInLoop.IsEnabled = true;
        //}
    }


}
