using System.Device.Location;
using System.ComponentModel;

namespace Ajapaik.Models
{
    public class PhotoPushpin : INotifyPropertyChanged
    {
        private bool visibility;

        #region protected methods
        /// <summary>
        /// Looks that property was changed.
        /// </summary>
        protected virtual void OnPropertyChanged (PropertyChangedEventArgs e)
        {
            var tempEvent = PropertyChanged;
            if (tempEvent != null)
            {
                tempEvent(this, e);
            }
        }
        #endregion
        
        /// <summary>
        /// Property change event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public int ID { get; set; }

        public string Title { get; set; }

        public GeoCoordinate Location { get; set; }

        public string Thumbnail { get; set; }

        public bool Visibility 
        { 
            get { return visibility; }
            set
            {
                if(visibility != value)
                {   
                    visibility = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("Visibility"));
                }
            }
        }

        public PhotoPushpin(int id, double latitude, double longitude, string thumbnail, string title)
        {
            this.ID = id;
            this.Title = title;
            this.Location = new GeoCoordinate(latitude, longitude);
            this.Visibility = false;
            this.Thumbnail = thumbnail; 
        }           
    }
}