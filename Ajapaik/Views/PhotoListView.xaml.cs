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
using System.Collections.ObjectModel;
using Ajapaik.Models;

namespace Ajapaik.Views
{
    public partial class PhotoListView : PhoneApplicationPage
    {

        private void PhotoSelected(object sender, SelectionChangedEventArgs e)
        {
            // If selected index is -1 (no selection) do nothing
            if (PhotoList.SelectedIndex == -1)
                return;

            App.Settings.SelectedPhoto = PhotoList.SelectedItem as Photo;

            // Navigate to the new page
            NavigationService.Navigate(new Uri("/Views/PhotoInfoView.xaml", UriKind.Relative));

            // Reset selected index to -1 (no selection)
            PhotoList.SelectedIndex = -1;
        }

        private void OnMapClick(object sender, EventArgs e)
        {
            NavigationService.GoBack();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            PhotoList.ItemsSource = App.Settings.Photos;
        }
        
        public ObservableCollection<Photo> Photos
        {
            get { return App.Settings.Photos; }
        }
        
        public PhotoListView()
        {
            InitializeComponent();
        }
    }
}