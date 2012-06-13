using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.WindowsAzure.Samples.Phone.Identity.AccessControl;
using System.IO;
using System.Text;
using System.Collections.ObjectModel;

namespace PhoneApp
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            var simpleWebTokenStore = Application.Current.Resources["swtStore"]
                as SimpleWebTokenStore;

            string url = "http://localhost:1838/api/beer";
            
            var request = (HttpWebRequest)WebRequest.Create(
            new Uri(url));

                request.Accept = "application/json";
                request.Headers["Authorization"] = simpleWebTokenStore.SimpleWebToken.RawToken;

                request.BeginGetResponse(r =>
                {
                    var httpRequest = (HttpWebRequest)r.AsyncState;
                    var httpResponse = (HttpWebResponse)httpRequest.EndGetResponse(r);

                    using (var reader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var json = reader.ReadToEnd();

                        Deployment.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            var beers = SimpleJson.DeserializeObject(json);

                            BeerList.ItemsSource = (List<object>)beers;
                        }));
                    }

                }, request);
        }
    }
}