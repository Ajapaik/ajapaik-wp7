using System.Runtime.Serialization;
using System.Collections.Generic;

namespace Ajapaik.Models
{
    [DataContract]
    public class PhotosResponse
    {
        [DataMember(Name="result")]
        public List<Photo> Photos { get; set; }
    }
}