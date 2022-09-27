using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using VectorBusLibrary.Processors;
using vxlapi_NET;
using static vxlapi_NET.XLDefine;
using Image = System.Windows.Controls.Image;

namespace CanBusDemoWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static CanBus? canBus = null;

        public MainWindow()
        {
            InitializeComponent();
            //Helelper.SetLogoToWindow(logoEntry);
            SetDefaultValuesForTransmit();

        }

        private void SetDefaultValuesForTransmit()
        {
            txtBoxMsgId.Text = "0x3c0";
            txtBoxDlc.Text = "4";
            txtBoxData0.Text = "0";
            txtBoxData1.Text = "0";
            txtBoxData2.Text = "0";
            txtBoxData3.Text = "0";
            txtBoxData4.Text = "0";
            txtBoxData5.Text = "0";
            txtBoxData6.Text = "0";
            txtBoxData7.Text = "0";
        }

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
                Helelper.WriteLogToTextBox($"Init driver: OK", txtBoxLogApp);

            }
        }

        private void btnOpenDriver_Click(object sender, RoutedEventArgs e)
        {

            Helelper.WriteLogToTextBox($"Open driver: {canBus.OpenDriver()}", txtBoxLogApp);
        }

        private void btnOpenPort_Click(object sender, RoutedEventArgs e)
        {
            Helelper.WriteLogToTextBox($"Get driver config: {canBus.GetDriverConfig()}", txtBoxLogApp);
            canBus.GetAppConfigAndSetAppConfig();

            Helelper.WriteLogToTextBox($"Get app config and set appConfig: OK", txtBoxLogApp);
            canBus.RequestTheUserToAssignChannels();

            CommonVector.GetAccesMask();
            Helelper.WriteLogToTextBox($"Get acces mask: OK", txtBoxLogApp);

            Helelper.WriteLogToTextBox($"Open port: {canBus.OpenPort()}", txtBoxLogApp);
            Helelper.WriteLogToTextBox($"Activate channel: {canBus.ActivateChannel()}", txtBoxLogApp);
            Helelper.WriteLogToTextBox($"Set notification CanBus: {canBus.SetNotificationCanBus()}", txtBoxLogApp);
            Helelper.WriteLogToTextBox($"Reset clock: {canBus.ResetClock()}", txtBoxLogApp);


        }

        private async void btnStartRx_Click(object sender, RoutedEventArgs e)
        {
            Helelper.WriteLogToTextBox($"Rx start", txtBoxLogApp);

            if (canBus != null)
            {

                while (true)
                {
                    txtBoxReceiveMsg.Text = canBus.RxAsync();

                }



            }
        }


        private void btnTransmitSingle_Click(object sender, RoutedEventArgs e)
        {
            XLClass.xl_event_collection xlEventCollection = new XLClass.xl_event_collection(1);
            xlEventCollection.xlEvent[0].tagData.can_Msg.id = Convert.ToUInt32(txtBoxMsgId.Text, 16);
            xlEventCollection.xlEvent[0].tagData.can_Msg.dlc = ushort.Parse(txtBoxDlc.Text);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[0] = byte.Parse(txtBoxData0.Text);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[1] = byte.Parse(txtBoxData1.Text);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[2] = byte.Parse(txtBoxData2.Text);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[3] = byte.Parse(txtBoxData3.Text);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[4] = byte.Parse(txtBoxData4.Text);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[5] = byte.Parse(txtBoxData5.Text);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[6] = byte.Parse(txtBoxData6.Text);
            xlEventCollection.xlEvent[0].tagData.can_Msg.data[7] = byte.Parse(txtBoxData7.Text);
            xlEventCollection.xlEvent[0].tag = XL_EventTags.XL_TRANSMIT_MSG;

            XL_Status status = canBus.CanTransmit(xlEventCollection);

            Helelper.WriteLogToTextBox($"Message[msgId:{txtBoxMsgId.Text} DLC:{txtBoxDlc.Text} data[0]:{txtBoxData0.Text} data[1]:{txtBoxData1.Text} data[2]:{txtBoxData2.Text} data[3]:{txtBoxData3.Text} data[4]:{txtBoxData4.Text} data[5]:{txtBoxData5.Text} data[6]:{txtBoxData6.Text} data[7]:{txtBoxData7.Text}] - {status}", txtBoxLogApp);

        }

        bool txLoopEnable = true;

        private async void btnTransmitInLoop_Click(object sender, RoutedEventArgs e)
        {
            await TxLoop();
        }

        private async Task TxLoop()
        {
            while (txLoopEnable)
            {
                XLClass.xl_event_collection xlEventCollection = new XLClass.xl_event_collection(1);
                xlEventCollection.xlEvent[0].tagData.can_Msg.id = Convert.ToUInt32(txtBoxMsgId.Text, 16);
                xlEventCollection.xlEvent[0].tagData.can_Msg.dlc = ushort.Parse(txtBoxDlc.Text);
                xlEventCollection.xlEvent[0].tagData.can_Msg.data[0] = byte.Parse(txtBoxData0.Text);
                xlEventCollection.xlEvent[0].tagData.can_Msg.data[1] = byte.Parse(txtBoxData1.Text); ;
                xlEventCollection.xlEvent[0].tagData.can_Msg.data[2] = byte.Parse(txtBoxData2.Text); ;
                xlEventCollection.xlEvent[0].tagData.can_Msg.data[3] = byte.Parse(txtBoxData3.Text); ;
                xlEventCollection.xlEvent[0].tagData.can_Msg.data[4] = byte.Parse(txtBoxData4.Text); ;
                xlEventCollection.xlEvent[0].tagData.can_Msg.data[5] = byte.Parse(txtBoxData5.Text); ;
                xlEventCollection.xlEvent[0].tagData.can_Msg.data[6] = byte.Parse(txtBoxData6.Text); ;
                xlEventCollection.xlEvent[0].tagData.can_Msg.data[7] = byte.Parse(txtBoxData7.Text); ;
                xlEventCollection.xlEvent[0].tag = XL_EventTags.XL_TRANSMIT_MSG;

                XL_Status status = canBus.CanTransmit(xlEventCollection);

                //Helelper.WriteLogToTextBox($"Message[msgId:{txtBoxMsgId.Text} DLC:{txtBoxDlc.Text} data[0]:{txtBoxData0.Text} data[1]:{txtBoxData1.Text} data[2]:{txtBoxData2.Text} data[3]:{txtBoxData3.Text} data[4]:{txtBoxData4.Text} data[5]:{txtBoxData5.Text} data[6]:{txtBoxData6.Text} data[7]:{txtBoxData7.Text}] - {status}", txtBoxLogApp);
            }
        }
        private void checkBoxTrnasmitMessageInLoop_Checked(object sender, RoutedEventArgs e)
        {

            //if (checkBoxTrnasmitMessageInLoop.IsChecked == true)
            //{
            //    txLoopEnable = true;
            //    checkBoxTrnasmitMessageInLoop.IsChecked = true;
            //}
            //else
            //{
            //    txLoopEnable = false;
            //    checkBoxTrnasmitMessageInLoop.IsChecked = false;
            //}
        }


        public static class Helelper
        {
            public static void WriteLogToTextBox(string text, TextBox uiElement)
            {
                string outString = $"{DateTime.Now:G}>>{text}{Environment.NewLine}";

                Trace.WriteLine(outString);
                uiElement.AppendText(outString);
                uiElement.ScrollToEnd();
            }

            internal static void InitCanControloler(CanBus context)
            {
                Trace.WriteLine("****************************");
                Trace.WriteLine("CanBus - Vector");
                Trace.WriteLine("****************************");

                Trace.WriteLine("vxlapi_NET        : " + typeof(XLDriver).Assembly.GetName().Version);
                context.OpenDriver();
                context.GetDriverConfig();
                context.GetAppConfigAndSetAppConfig();
                context.RequestTheUserToAssignChannels();
                CommonVector.GetAccesMask();
                Trace.WriteLine(CommonVector.PrintAccessMask());
                context.OpenPort();
                context.ActivateChannel();
                context.SetNotificationCanBus();
                context.ResetClock();
            }

            public static void SetLogoToWindow(Image image)
            {
                BitmapImage bitmapEntry = new BitmapImage();
                bitmapEntry.BeginInit();
                bitmapEntry.UriSource = new Uri(@"https://media-exp1.licdn.com/dms/image/C4D0BAQEv19f-dd628g/company-logo_200_200/0/1639729805598?e=1672272000&v=beta&t=9EMTnAOx1Ml5KDyZx5pD336FXQtnMwsHZX5wNxu_ADI", UriKind.Absolute);

                bitmapEntry.EndInit();

                image.Source = bitmapEntry;
            }

        }
    }
}
