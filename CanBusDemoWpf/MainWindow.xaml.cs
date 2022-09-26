using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VectorBusLibrary.Processors;
using vxlapi_NET;

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

            Helelper.WriteLogToTextBox($"Open driver: {canBus.OpenDriver()}",txtBoxLogApp);
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

        private void btnStartRx_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public static class Helelper
    {
        public static void WriteLogToTextBox(string text, TextBox uiElement)
        {
            string outString = $"{DateTime.Now:G}>>{text}{Environment.NewLine}";

            Trace.WriteLine(outString);
            uiElement.AppendText(outString);
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

    }
}
