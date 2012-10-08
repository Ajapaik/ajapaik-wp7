using Ajapaik.Models;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.IO;

namespace Ajapaik
{
    public class Settings
    {
        public const string URI = "http://api.ajapaik.ee/?action=photo&city_id=2";

        public Photo SelectedPhoto { get; set; }

        public MemoryStream PhotoByUser { get; set; }

        public ObservableCollection<Photo> Photos { get; set; }
    }
}