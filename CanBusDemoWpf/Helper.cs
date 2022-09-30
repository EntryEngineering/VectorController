using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using VectorBusLibrary.Processors;
using vxlapi_NET;
using Image = System.Windows.Controls.Image;

namespace CanBusDemoWpf
{
    public partial class MainWindow
    {
        public static class Helper
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
