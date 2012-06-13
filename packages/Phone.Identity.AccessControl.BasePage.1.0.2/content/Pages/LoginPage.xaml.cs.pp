namespace $rootnamespace$.Pages
{
    using System;
    using System.Windows;
    using System.Windows.Navigation;
    using Microsoft.Phone.Controls;
    using Microsoft.WindowsAzure.Samples.Phone.Identity.AccessControl;

    public partial class LoginPage : PhoneApplicationPage
    {
        private readonly SimpleWebTokenStore swtStore = Application.Current.Resources["swtStore"] as SimpleWebTokenStore;

        public LoginPage()
        {
            this.InitializeComponent();

            this.PageTransitionReset.Begin();
            this.SignInControl.RequestSimpleWebTokenResponseCompleted +=
                (s, e) => 
                {
                    // The ACS token was successfully received and stored in the "swtStore" application resource.
                    // TODO: Navigate to your main page i.e.:
                    // this.NavigationService.Navigate(new Uri("/Pages/NotificationsPage.xaml", UriKind.Relative));
                };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if ((e.NavigationMode == NavigationMode.New) && this.swtStore.IsValid())
            {
                // There is a valid ACS token already in the "swtStore" application resource.
                // TODO: Navigate to your main page i.e.:
				// this.NavigationService.Navigate(new Uri("/Pages/NotificationsPage.xaml", UriKind.Relative));
            }
            else
            {
                // There is not a valid ACS token in the "swtStore" application resource.
                // The token may be expired or it is the first time the user logs in.
                this.PageTransitionIn.Begin();
                this.SignInControl.GetSimpleWebToken();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.PageTransitionReset.Begin();
        }
    }
}