using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.WindowsUI;
using Hardcodet.Wpf.TaskbarNotification;

namespace Station
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DXWindow
    {
        private string origin = "Rescue 1122 (44 Station), Rawalpindi, Pakistan";
        private string destination = "";

        TcpClient client;
        NetworkStream ns;
        Thread t = null;
        private const string hostName = "localhost";
        private const int portNum = 4444;
        public MainWindow()
        {
            

            while (true)
            {
                try
                {
                    client = new TcpClient(hostName, portNum);
                }
                catch (SocketException e)
                {

                    continue;
                }


                if (!client.Connected)
                {
                    client.Close();
                }
                else
                {
                    break;
                }

            }
            ns = client.GetStream();


            InitializeComponent();
            HideScriptErrors(Browser, true);

            t = new Thread(DoWork);
            t.Start();
        }

        public void DoWork()
        {
            byte[] bytes = new byte[1024];
            string line;

            try
            {
                while (true)
                {

                    int bytesRead = ns.Read(bytes, 0, bytes.Length);
                    line = Encoding.ASCII.GetString(bytes, 0, bytesRead);

                    if (line.EndsWith("==="))
                    {
                        TaskbarIcon.ShowBalloonTip("Alert!", "There is an emergency...", BalloonIcon.Error);
                        //Label.Visibility = Visibility.Visible;
                        string[] s = line.Substring(0, line.Length - 3).Split('/');

                        origin = s[0];
                        destination = s[1];

                        //Visibility=Visibility.Visible;


                    }
                }
            }
            catch (Exception e)
            {
                Environment.Exit(0);
            }
            
        }

        public void HideScriptErrors(WebBrowser wb, bool hide)
        {
            var fiComWebBrowser = typeof(WebBrowser).GetField("_axIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic);
            if (fiComWebBrowser == null) return;
            var objComWebBrowser = fiComWebBrowser.GetValue(wb);
            if (objComWebBrowser == null)
            {
                wb.Loaded += (o, s) => HideScriptErrors(wb, hide); //In case we are to early
                return;
            }
            objComWebBrowser.GetType().InvokeMember("Silent", BindingFlags.SetProperty, null, objComWebBrowser, new object[] { hide });
        }

        private void DXWindow_Loaded(object sender, RoutedEventArgs e)
        {
            HideScriptErrors(Browser, true);
        }

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {
            Browser.Navigate(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + @"\ShowMap.htm");

            
        }

        private void ShowButton_Click(object sender, RoutedEventArgs e)
        {
            if (destination=="")
            {
                WinUIMessageBox.Show("No location is received yet.",
                        "Route", MessageBoxButton.OK, MessageBoxImage.Information);
               
            }
            else
            {
                Browser.InvokeScript("GetRouteAddress", new string[] { origin, destination });
                //Label.Visibility = Visibility.Hidden;
            }
            
        }
        private void TaskbarIcon_OnTrayMouseDoubleClick(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Visible;
            this.Show();
            this.ShowActivated = true;
            this.WindowState = WindowState.Normal;
        }

        private void DXWindow_StateChanged(object sender, EventArgs e)
        {


            if (this.WindowState == WindowState.Minimized)
            {

                TaskbarIcon.IsEnabled = true;
                TaskbarIcon.Visibility = Visibility.Visible;
                this.Visibility = Visibility.Hidden;
                //Icon myIcon = new Icon("Resources/NotifyIcon.ico");
                TaskbarIcon.ShowBalloonTip("Rescue Station", "Application is running...", BalloonIcon.None);
            }
            if (this.WindowState == WindowState.Normal || this.WindowState == WindowState.Maximized)
            {

                this.Visibility = Visibility.Visible;
            }
        }

        private void DXWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ns.Close();
            client.Close();
            Environment.Exit(0);
        }
    }
}
