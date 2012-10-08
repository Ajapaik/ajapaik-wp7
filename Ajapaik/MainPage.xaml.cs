using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Ajapaik.Models;
using Ajapaik.Helpers;
using Microsoft.Phone.Controls.Maps;
using System.Collections.ObjectModel;

namespace Ajapaik
{
    public partial class MainPage : PhoneApplicationPage
    {
        private Pushpin currentPushpin;

        //private methods
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            App.Settings.SelectedPhoto = App.Settings.Photos.First(x => x.ID == (int)button.Tag);
            NavigationService.Navigate(new Uri("/Views/PhotoInfoView.xaml", UriKind.Relative));
        }

        private void PushPin_Tap(object sender, RoutedEventArgs e)
        {
            Pushpin pushpin = sender as Pushpin;
            App.Settings.SelectedPhoto = App.Settings.Photos.First(x => x.ID == (int)pushpin.Tag);

            //NavigationService.Navigate(new Uri("/Views/PhotoInfoView.xaml", UriKind.Relative));
            Border border = null;
            if (currentPushpin != null)
            {
                border = (Border)currentPushpin.Content;
                border.Visibility = Visibility.Collapsed;
            }
            //Pushpin pushpin = sender as Pushpin;
            border = (Border)pushpin.Content;
            border.Visibility = Visibility.Visible;
            currentPushpin = pushpin;
            //PhotoPushpin photoPushpin = Pushpins.First(x => x.ID == (int)pushpin.Tag);
            //photoPushpin.Visibility = true;
            //currentPhotoPushpin = photoPushpin;
        }

        private void LoadPhotos()
        {
            WebClient webClient = new WebClient();
            //ProgressBar.IsIndeterminate = true;
            webClient.DownloadStringCompleted += (s, e) =>
            {
                if (e.Error == null)
                {
                    PhotosResponse response = JsonSerializer.Deserialize<PhotosResponse>(e.Result);
                    List<Photo> oldPhotos = response.Photos;
                    App.Settings.Photos = new ObservableCollection<Photo>(oldPhotos);
                    Pushpins = new ObservableCollection<PhotoPushpin>();
                    foreach (Photo photo in oldPhotos)
                    {
                        Pushpins.Add(new PhotoPushpin(photo.ID,photo.Latitude,photo.Longitude, photo.ThumbnailURL, photo.Description));
                    }
                    CenterMap();
                    MapItems.ItemsSource = Pushpins;
                    //ProgressBar.IsIndeterminate = false;
                }
            };
            webClient.DownloadStringAsync(new Uri(Settings.URI));   
        }

        /// <summary>
        /// Center map code.
        /// </summary>
        private void CenterMap()
        {
            if (Pushpins != null)
            {
                var locations = Pushpins.Select(model => model.Location);
                LocationMap.SetView(LocationRect.CreateLocationRect(locations));
            }
        }

        /// <summary>
        /// List app bar button clicked
        /// </summary>
        private void OnListClick(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/PhotoListView.xaml", UriKind.Relative));
        }

        //protected methoods
        /// <summary>
        /// On navigated action
        /// </summary>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            CenterMap();
        } 

        //public methods
        public ObservableCollection<PhotoPushpin> Pushpins { get; set; }

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            LoadPhotos();
        }

        private void pushPin_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }
    }
}