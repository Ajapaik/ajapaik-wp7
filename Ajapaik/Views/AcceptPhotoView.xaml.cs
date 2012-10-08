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
using Ajapaik.Models;
using System.Windows.Media.Imaging;
using Hammock;
using System.IO;
using Hammock.Web;
using System.Text;
using Coding4Fun.Phone.Controls;
using Ajapaik.Helpers;

namespace Ajapaik.Views
{
    public partial class AcceptPhotoView : PhoneApplicationPage
    {
        private Photo oldPhoto;

        #region private methods
        private void PhotoPostCompleted(RestRequest request, RestResponse response, object userstate)
        {
            // We want to ensure we are running on our thread UI
            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    Deployment.Current.Dispatcher.BeginInvoke(
                        () =>
                        {
                            ToastPrompt toast = new ToastPrompt
                            {
                                Title = "ajapaik",
                                Message = "your photo was uploaded",
                            };
                            toast.Show();
                        });
                }
                else
                {
                    MessageBox.Show("Error while uploading to server. Please try again later. " +
                            "If this error persists please let the program author know.");
                }
            });
        }

        //private methods
        private void OnOkClick(object sender, EventArgs e)
        {
            WebClient wc = new WebClient();

            var picture = App.Settings.PhotoByUser;
            Dictionary<string, object> data = new Dictionary<string, object>()
            { 
                {"user_file[]", picture.ToArray() }
            };
            PostSubmitter post = new PostSubmitter() { url = "http://www.ajapaik.ee/foto/" + App.Settings.SelectedPhoto.ID + "/upload/", parameters = data };
            post.theEvent += new EventHandler(post_theEvent);
            post.Submit();
           
        }

        private void post_theEvent(object sender, EventArgs e)
        {
            Deployment.Current.Dispatcher.BeginInvoke(
                         () =>
                         {
                             ToastPrompt toast = new ToastPrompt
                             {
                                 Title = "ajapaik",
                                 Message = "your photo was uploaded",
                             };
                             toast.Show();
                         }); 
        }

            
        private void OnCancelClick(object sender, EventArgs e)
        {
            App.Settings.PhotoByUser = null;
            NavigationService.GoBack();
        }
        #endregion

        public AcceptPhotoView()
        {
            InitializeComponent();
            oldPhoto = App.Settings.SelectedPhoto;
            OldPhoto.Source = new BitmapImage(new Uri(oldPhoto.ImageURL));
            BitmapImage bmp = new BitmapImage();
            bmp.SetSource(App.Settings.PhotoByUser);
            NewPhoto.Source = bmp;
        }
    }
}
