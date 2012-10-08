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
using System.Device.Location;
using Ajapaik.Models;

namespace Ajapaik.Views
{
    public partial class PhotoInfoView : PhoneApplicationPage
    {
        private Photo photo;

        private void OnRephotoClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/CameraWithOverlay.xaml", UriKind.Relative));
        }

        public PhotoInfoView()
        {
            InitializeComponent();
            photo = App.Settings.SelectedPhoto;
            DataContext = photo;
            GeoCoordinate center = new GeoCoordinate(photo.Latitude, photo.Longitude);
            LocationMap.Center = center;
            OldPhotoPushpin.Location = center;
        }
    }
}