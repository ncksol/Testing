﻿using System;
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
            var posts = await _lepra.GetMyPosts(true);

            postsList.ItemsSource = posts;
        }
    }
}