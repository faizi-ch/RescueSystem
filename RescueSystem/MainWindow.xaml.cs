using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using DevExpress.Xpf.Core;
using GoogleMapsApi;
using GoogleMapsApi.Entities.Common;
using GoogleMapsApi.Entities.Directions.Request;
using GoogleMapsApi.Entities.Directions.Response;
using GoogleMapsApi.Entities.Geocoding.Request;
using GoogleMapsApi.Entities.Geocoding.Response;
using GoogleMapsApi.StaticMaps;
using GoogleMapsApi.StaticMaps.Entities;
using DevExpress.Xpf.WindowsUI;

namespace RescueSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : DXWindow
    {
        //private Location station1Location, station2Location, station3Location;
        private string[] stations = { "Rescue 1122 (44 Station), Rawalpindi, Pakistan", "Rescue 1122, Peshawar Rd, Rawalpindi, Pakistan", "Rescue 1122, Liaquat Rd, Rawalpindi, Pakistan" };
        //private string station2 = ;
        //private string station3 = ;
        TcpListener listener;
        TcpClient client;
        NetworkStream ns;
        private String loc = "";
        public MainWindow()
        {
            InitializeComponent();
            HideScriptErrors(Browser,true);

            WinUIMessageBox.Show("Waiting for stations to connect... Connect a station first.",
                        "Connection", MessageBoxButton.OK, MessageBoxImage.Information);

            listener = new TcpListener(4444);
            listener.Start();
            client = listener.AcceptTcpClient();
            ns = client.GetStream();
        }

        private void Browser_Loaded(object sender, RoutedEventArgs e)
        {

            Browser.Navigate(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName + @"\CalculateDistance.htm");
            

        }

        private void SimpleButton_Click(object sender, RoutedEventArgs e)
        {
            Browser.InvokeScript("initMap");
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


        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            StatusListEdit.Items.Clear();

            string destAddress;
            Double[] distances=new double[stations.Length];
            string[] durations=new string[stations.Length];
            int c, location = 1;
            double minimum;

            

            if (AddressTextEdit.Text=="")
            {
                WinUIMessageBox.Show("Please! Enter a destination address.",
                        "Destination field is empty", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {

                StatusListEdit.Items.Add("Calculating distance...");

                destAddress = AddressTextEdit.Text;

                for (int i = 0; i < stations.Length; i++)
                {
                    DirectionsRequest directionsRequest2 = new DirectionsRequest()
                    {
                        Origin = stations[i],
                        Destination = destAddress,
                    };

                    DirectionsResponse directionsResponse = GoogleMapsApi.GoogleMaps.Directions.Query(directionsRequest2);
                    GoogleMapsApi.Entities.Directions.Response.Leg leg =
                        directionsResponse.Routes.ElementAt(0).Legs.ElementAt(0);

                    StatusListEdit.Items.Add(string.Format("Distance from: {0} to {1} is {2} will take {3}", stations[i],
                        destAddress, leg.Distance.Text, leg.Duration.Text));

                    distances[i] = Convert.ToDouble(leg.Distance.Text.Substring(0, leg.Distance.Text.Length - 3));
                    durations[i] = leg.Duration.Text;
                }

                minimum = distances[0];
                for (c = 0; c < stations.Length; c++)
                {
                    if (distances[c] < minimum)
                    {
                        minimum = distances[c];
                        location = c;
                    }
                }

                StatusListEdit.Items.Add(
                    string.Format("Station-{0} ({1}) has shortest distance from {2} of {3} km will take just {4}",
                        location + 1, stations[location], destAddress, distances[location], durations[location]));

                Browser.Visibility = Visibility.Visible;
                Browser.InvokeScript("GetRouteAddress", new object[] {stations[location], destAddress});

                if (client!=null)
                {
                    
                        StatusListEdit.Items.Add("");
                        


                        
                        //byte[] bytes = new byte[1024];
                        //int bytesRead = ns.Read(bytes, 0, bytes.Length);
                        //string m = Encoding.ASCII.GetString(bytes, 0, bytesRead);
                        //Text = m;
                        
                        loc = stations[location] + "/" + destAddress;
                        SendButton.Visibility = Visibility.Visible;

                    
                }
                

            }

            //Browser.InvokeScript("calculateDistance", new string[] { station1, station2 });




            //Browser.InvokeScript("GetRouteLatLong", new object[] { station1, 33.6141098, 72.9947294 });



        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
        MessageBoxResult msgBoxResult =
                    WinUIMessageBox.Show("Do you want to send the location?",
                        "Location", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (msgBoxResult == MessageBoxResult.Yes)
        {
            try
            {
                    byte[] byteTime = Encoding.ASCII.GetBytes(loc + "===");
                    ns.Write(byteTime, 0, byteTime.Length);
                }
            catch (Exception exception)
            {
                    WinUIMessageBox.Show("Make sure you have made station application is running. Otherwise restart and then reconnect both.",
                            "Sending ERROR", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            
        }
        }

        private void DXWindow_Loaded(object sender, RoutedEventArgs e)//166.62.28.123
        {
            StatusListEdit.Items.Add("Connected!");
            AddressTextEdit.Focus();// 11.62.0.20

            /*try
            {
                MySql.Data.MySqlClient.MySqlConnection mySqlConnection = new MySqlConnection("Server=11.62.0.20;Database=alertappp;Uid=cpses_jas8jIXx71;Pwd=Saibi.022;");
                mySqlConnection.Open();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }

            /*WebRequest req = WebRequest.Create("login.php");

            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            string postData = "username=Jamilk1&password=Saibi.022";
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);

            req.ContentLength = byteArray.Length;


            Stream ds = req.GetRequestStream();
            ds.Write(byteArray, 0, byteArray.Length);
            ds.Close();

            WebResponse wr = req.GetResponse();
            label1.Content = ((HttpWebResponse)wr).StatusDescription;
            ds = wr.GetResponseStream();
            StreamReader reader = new StreamReader(ds);
            XDocument doc = XDocument.Load(reader);*/

        }
    }
}
