using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Media;
using Microsoft.Devices;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework.Media;
using System.Windows.Media.Imaging;
using Ajapaik.Models;
using ExifLib;
using System.Net;

namespace Ajapaik.Views
{
    public partial class CameraWithOverlay : PhoneApplicationPage
    {
        // Variables
        private int savedCounter = 0;
        PhotoCamera cam;
        MediaLibrary library = new MediaLibrary(); double initialScale = 1d;
        // these two fully define the zoom state:
        private double TotalImageScale = 1d;
        private Point ImagePosition = new Point(0, 0);

        private Point _oldFinger1;
        private Point _oldFinger2;
        private double _oldScaleFactor;
        private double _angle;

        private void OnPinchStarted(object s, PinchStartedGestureEventArgs e)
        {
            _oldFinger1 = e.GetPosition(MainImage, 0);
            _oldFinger2 = e.GetPosition(MainImage, 1);
            _oldScaleFactor = 1;
        }

        private void OnPinchDelta(object s, PinchGestureEventArgs e)
        {
            var scaleFactor = e.DistanceRatio / _oldScaleFactor;

            var currentFinger1 = e.GetPosition(MainImage, 0);
            var currentFinger2 = e.GetPosition(MainImage, 1);

            var translationDelta = GetTranslationDelta(
                currentFinger1,
                currentFinger2,
                _oldFinger1,
                _oldFinger2,
                ImagePosition,
                scaleFactor);

            _oldFinger1 = currentFinger1;
            _oldFinger2 = currentFinger2;
            _oldScaleFactor = e.DistanceRatio;

            UpdateImage(scaleFactor, translationDelta);
        }

        private void UpdateImage(double scaleFactor, Point delta)
        {
    
            TotalImageScale *= scaleFactor;
            if (TotalImageScale < 1)
            {
                TotalImageScale = 1;
                return;
            }

            ImagePosition = new Point(ImagePosition.X + delta.X, ImagePosition.Y + delta.Y);

            var transform = (CompositeTransform)MainImage.RenderTransform;
            transform.ScaleX = TotalImageScale;
            transform.ScaleY = TotalImageScale;
            transform.TranslateX = ImagePosition.X;
            transform.TranslateY = ImagePosition.Y;
        }

        private Point GetTranslationDelta(
            Point currentFinger1, Point currentFinger2,
            Point oldFinger1, Point oldFinger2,
            Point currentPosition, double scaleFactor)
        {
            var newPos1 = new Point(
                currentFinger1.X + (currentPosition.X - oldFinger1.X) * scaleFactor,
                currentFinger1.Y + (currentPosition.Y - oldFinger1.Y) * scaleFactor);

            var newPos2 = new Point(
                currentFinger2.X + (currentPosition.X - oldFinger2.X) * scaleFactor,
                currentFinger2.Y + (currentPosition.Y - oldFinger2.Y) * scaleFactor);

            var newPos = new Point(
                (newPos1.X + newPos2.X) / 2,
                (newPos1.Y + newPos2.Y) / 2);

            return new Point(
                newPos.X - currentPosition.X,
                newPos.Y - currentPosition.Y);
        }
        //region private methods
        
        private void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            if (MainImage == null) return;
            MainImage.Opacity = args.NewValue / 100;
        }
        // Update the UI if initialization succeeds.
        private void cam_Initialized(object sender, Microsoft.Devices.CameraOperationCompletedEventArgs e)
        {
            if (e.Succeeded)
            {
                this.Dispatcher.BeginInvoke(delegate()
                {
                    // Write message.
                    //txtDebug.Text = "Camera initialized.";
                    //isCameraInitialized = true;
                });
            }
        }
        private void cam_CaptureCompleted(object sender, CameraOperationCompletedEventArgs e)
        {
            // Increments the savedCounter variable used for generating JPEG file names.
            savedCounter++;
        }
        private void OnTakePhotoClick(object sender, EventArgs e)
        {
            if (cam != null)
            {
                try
                {
                    // Start image capture.
                    cam.CaptureImage();
                }
                catch (Exception ex)
                {
                    this.Dispatcher.BeginInvoke(delegate()
                    {
                        // Cannot capture an image until the previous capture has completed.
                        //txtDebug.Text = ex.Message;
                    });
                }
            }
        }
        // Informs when full resolution picture has been taken, saves to local media library and isolated storage.
        void cam_CaptureImageAvailable(object sender, Microsoft.Devices.ContentReadyEventArgs e)
        {
            string fileName = String.Format("Ajapaik_{0:yyyy-MM-dd_hh-mm-ss-tt}.jpg",
              DateTime.Now);

            try
            {   
                // Save picture to the library camera roll.
                library.SavePictureToCameraRoll(fileName, e.ImageStream);

                // Set the position of the stream back to start
                e.ImageStream.Seek(0, SeekOrigin.Begin);

                // Save picture as JPEG to isolated storage.
                App.Settings.PhotoByUser = new MemoryStream();
                var targetStream = App.Settings.PhotoByUser;
                // Initialize the buffer for 4KB disk pages.
                byte[] readBuffer = new byte[4096];
                int bytesRead = -1;

                        // Copy the image to isolated storage. 
                while ((bytesRead = e.ImageStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    targetStream.Write(readBuffer, 0, bytesRead);
                }

                this.Dispatcher.BeginInvoke(delegate()
                {
                    // Write message.
                    //txtDebug.Text = "Camera initialized.";
                    //isCameraInitialized = true;
                    NavigationService.Navigate(new Uri("/Views/AcceptPhotoView.xaml", UriKind.Relative));
                });
            }
            finally
            {
                // Close image stream
                e.ImageStream.Close();
            }

        }
        // Informs when thumbnail picture has been taken, saves to isolated storage
        // User will select this image in the pictures application to bring up the full-resolution picture. 
        public void cam_CaptureThumbnailAvailable(object sender, ContentReadyEventArgs e)
        {
            string fileName = savedCounter + "_th.jpg";

            try
            {
                // Write message to UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                   // txtDebug.Text = "Captured image available, saving thumbnail.";
                });

                // Save thumbnail as JPEG to isolated storage.
                using (IsolatedStorageFile isStore = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (IsolatedStorageFileStream targetStream = isStore.OpenFile(fileName, FileMode.Create, FileAccess.Write))
                    {
                        // Initialize the buffer for 4KB disk pages.
                        byte[] readBuffer = new byte[4096];
                        int bytesRead = -1;

                        // Copy the thumbnail to isolated storage. 
                        while ((bytesRead = e.ImageStream.Read(readBuffer, 0, readBuffer.Length)) > 0)
                        {
                            targetStream.Write(readBuffer, 0, bytesRead);
                        }
                    }
                }

                // Write message to UI thread.
                Deployment.Current.Dispatcher.BeginInvoke(delegate()
                {
                    //txtDebug.Text = "Thumbnail has been saved to isolated storage.";

                });
            }
            finally
            {
                // Close image stream
                e.ImageStream.Close();
            }
        }
        private void LoadPhoto()
        {
            Photo photo = App.Settings.SelectedPhoto;
            WebClient wc = new WebClient();
            wc.OpenReadAsync( new Uri(photo.ImageURL));
            wc.OpenReadCompleted += new OpenReadCompletedEventHandler(wc_OpenReadCompleted);
        }

        void wc_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                var stream = e.Result;
                JpegInfo info = ExifReader.ReadJpeg(stream, "victory");
                

                 if (info.Height > info.Width)
                 {
                    stream = RotateStream(stream, 90);
                 }
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        var bitmap = new BitmapImage();
                        bitmap.SetSource(stream);
                        MainImage.Source = bitmap;
                    });
            }
            
        }




        //Code for initialization, capture completed, image availability events; also setting the source for the viewfinder.
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            LoadPhoto();

            // Check to see if the camera is available on the device.
            if ((PhotoCamera.IsCameraTypeSupported(CameraType.Primary) == true) ||
                 (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing) == true))
            {
                    cam = new Microsoft.Devices.PhotoCamera(CameraType.Primary);
             
                // Event is fired when the PhotoCamera object has been initialized.
                cam.Initialized += new EventHandler<Microsoft.Devices.CameraOperationCompletedEventArgs>(cam_Initialized);

                // Event is fired when the capture sequence is complete.
                cam.CaptureCompleted += new EventHandler<CameraOperationCompletedEventArgs>(cam_CaptureCompleted);

                // Event is fired when the capture sequence is complete and an image is available.
                cam.CaptureImageAvailable += new EventHandler<Microsoft.Devices.ContentReadyEventArgs>(cam_CaptureImageAvailable);

                // Event is fired when the capture sequence is complete and a thumbnail image is available.
                //cam.CaptureThumbnailAvailable += new EventHandler<ContentReadyEventArgs>(cam_CaptureThumbnailAvailable);

                //Set the VideoBrush source to the camera.
                viewfinderBrush.SetSource(cam);
            }
            else
            {
                // The camera is not supported on the device.
                this.Dispatcher.BeginInvoke(delegate()
                {
                    // Write message.
                   // txtDebug.Text = "A Camera is not available on this device.";
                });

                // Disable UI.
                //ShutterButton.IsEnabled = false;
            }
        }


        private Stream RotateStream(Stream stream, int angle)
        {
            stream.Position = 0;
            if (angle % 90 != 0 || angle < 0) throw new ArgumentException();
            if (angle % 360 == 0) return stream;

            BitmapImage bitmap = new BitmapImage();
            bitmap.SetSource(stream);
            WriteableBitmap wbSource = new WriteableBitmap(bitmap);

            WriteableBitmap wbTarget = null;
            if (angle % 180 == 0)
            {
                wbTarget = new WriteableBitmap(wbSource.PixelWidth, wbSource.PixelHeight);
            }
            else
            {
                wbTarget = new WriteableBitmap(wbSource.PixelHeight, wbSource.PixelWidth);
            }

            for (int x = 0; x < wbSource.PixelWidth; x++)
            {
                for (int y = 0; y < wbSource.PixelHeight; y++)
                {
                    switch (angle % 360)
                    {
                        case 90:
                            wbTarget.Pixels[(wbSource.PixelHeight - y - 1) + x * wbTarget.PixelWidth] = wbSource.Pixels[x + y * wbSource.PixelWidth];
                            break;
                        case 180:
                            wbTarget.Pixels[(wbSource.PixelWidth - x - 1) + (wbSource.PixelHeight - y - 1) * wbSource.PixelWidth] = wbSource.Pixels[x + y * wbSource.PixelWidth];
                            break;
                        case 270:
                            wbTarget.Pixels[y + (wbSource.PixelWidth - x - 1) * wbTarget.PixelWidth] = wbSource.Pixels[x + y * wbSource.PixelWidth];
                            break;
                    }
                }
            }
            MemoryStream targetStream = new MemoryStream();
            wbTarget.SaveJpeg(targetStream, wbTarget.PixelWidth, wbTarget.PixelHeight, 0, 100);
            return targetStream;
        }

        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            if (cam != null)
            {
                // Dispose camera to minimize power consumption and to expedite shutdown.
                cam.Dispose();

                // Release memory, ensure garbage collection.
                cam.Initialized -= cam_Initialized;
                cam.CaptureCompleted -= cam_CaptureCompleted;
                cam.CaptureImageAvailable -= cam_CaptureImageAvailable;
                //cam.CaptureThumbnailAvailable -= cam_CaptureThumbnailAvailable;
            }
        }

        // Ensure that the viewfinder is upright in LandscapeRight.
        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            if (cam != null)
            {
                // LandscapeRight rotation when camera is on back of device.
                int landscapeRightRotation = 180;

                // Rotate video brush from camera.
                if (e.Orientation == PageOrientation.LandscapeRight)
                {
                    // Rotate for LandscapeRight orientation.
                    viewfinderBrush.RelativeTransform =
                        new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = landscapeRightRotation };
                }
                else
                {
                    // Rotate for standard landscape orientation.
                    viewfinderBrush.RelativeTransform =
                        new CompositeTransform() { CenterX = 0.5, CenterY = 0.5, Rotation = 0 };
                }
            }

            base.OnOrientationChanged(e);
        }

        public CameraWithOverlay()
        {
            InitializeComponent();
        }
    }
}