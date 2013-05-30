using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using MetroLepraLib;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace PhoneApp1
{
    public partial class Post : PhoneApplicationPage
    {
        private Lepra _lepra;

        public Post()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            _lepra = new Lepra();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var newComment = _lepra.AddComment(App.CurrentPost, App.ReplyToComment, txtPostText.Text);
        }
    }
}