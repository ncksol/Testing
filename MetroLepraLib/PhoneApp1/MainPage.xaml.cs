using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Navigation;
using MetroLepraLib;
using Microsoft.Phone.Controls;

namespace PhoneApp1
{
    public partial class MainPage : PhoneApplicationPage
    {
        private Lepra _lepra;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _lepra = new Lepra();
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
        private async void BtnLoad_OnClick(object sender, RoutedEventArgs e)
        {
            await _lepra.LoadLoginPage();
            imgCaptcha.Source = _lepra.GetCaptcha();
        }

        private async void BtnLogin_OnClick(object sender, RoutedEventArgs e)
        {
            //var headers = await _lepra.TryLogin(txtCaptcha.Text, txtLogin.Text, txtPassword.Text);
            var headers = await _lepra.TryLogin(txtCaptcha.Text);
            var sb = new StringBuilder();
            foreach (var header in headers)
            {
                sb.AppendLine(String.Format("{0}:{1}", header.Key, new List<String>(header.Value).First()));
            }

            txtHeaders.Text = sb.ToString();
        }
    }
}