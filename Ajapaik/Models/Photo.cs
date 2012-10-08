using System.Runtime.Serialization;

namespace Ajapaik.Models
{
    [DataContract]
    public class Photo
    {
        [DataMember(Name="id")]
        public int ID { get; set; }

        [DataMember(Name="lat")]
        public double Latitude { get; set; }

        [DataMember(Name="lon")]
        public double Longitude { get; set; }

        [DataMember(Name="city_id")]
        public int CityID { get; set; }

        [DataMember(Name="description")]
        public string Description { get; set; }

        [DataMember(Name="image_url")]
        public string ImageURL { get; set; }

        [DataMember(Name="image_thumb")]
        public string ThumbnailURL { get; set; }
    }
}
