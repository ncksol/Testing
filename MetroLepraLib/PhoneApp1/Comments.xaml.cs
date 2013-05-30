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
    public partial class Comments : PhoneApplicationPage
    {
        private Lepra _lepra;

        public Comments()
        {
            InitializeComponent();

            _lepra = new Lepra();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var comments = await _lepra.GetComments(App.CurrentPost);
            postsList.ItemsSource = comments;
        }

        private void DownVote_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var comment = (LepraComment)button.Tag;

            _lepra.VoteComment(App.CurrentPost, comment, "-1");
        }

        private void UpVote_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var comment = (LepraComment)button.Tag;

            _lepra.VoteComment(App.CurrentPost, comment, "1");
        }

        private void Reply_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var comment = (LepraComment)button.Tag;
            App.ReplyToComment = comment;

            NavigationService.Navigate(new Uri("/Post.xaml", UriKind.Relative));
        }

        private void ReplyPost_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Post.xaml", UriKind.Relative));
        }
    }
}