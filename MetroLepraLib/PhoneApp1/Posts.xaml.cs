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
    public partial class Posts : PhoneApplicationPage
    {
        private Lepra _lepra;

        public Posts()
        {
            InitializeComponent();

            _lepra = new Lepra();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            /*var posts = await _lepra.GetInbox();
            postsList.ItemsSource = posts;*/
            _lepra.AddPost("http://void.leprosorium.ru/asylum/");
        }

        private void DownVote_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button) sender;
            var post = (LepraPost) button.Tag;

            _lepra.VotePost(post, "-1");
        }

        private void UpVote_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var post = (LepraPost)button.Tag;

            _lepra.VotePost(post, "1");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            App.CurrentPost = (LepraPost) button.Tag;

            NavigationService.Navigate(new Uri("/Comments.xaml", UriKind.Relative));
        }
    }
}