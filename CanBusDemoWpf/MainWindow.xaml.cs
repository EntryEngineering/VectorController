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
                Helelper.WriteLogToTextBox("Driver init - Ok", txtBoxLogApp);

            }
        }

        private void btnOpenDriver_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnOpenPort_Click(object sender, RoutedEventArgs e)
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
    }
}
