using Crystal.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Hanasu.Model
{
    public class Station : Crystal.Dynamic.AutoIPNotifyingBaseModel
    {
        public string Title { get; set; }
        public string StreamUrl { get; set; }
        public string PreprocessorFormat { get; set; }
        public string Format { get; set; }

        //Things to statisfy the built-in templates
        public string ImageUrl { get; set; }
        public ImageSource Image { get { if (ImageUrl != null) return new BitmapImage(new Uri(ImageUrl)); else return null; } }
        public string Subtitle { get; set; }
    }

    public class StationGroup : Crystal.Dynamic.AutoIPNotifyingBaseModel
    {
        public string Name { get; set; }
        public ObservableCollection<Station> Items { get; set; }
        public IEnumerable<Station> TopItems { get { if (Items != null) return Items.Take(5); else return null; } } // ????
    }
}
