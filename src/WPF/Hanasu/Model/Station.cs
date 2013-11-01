using Crystal.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Hanasu.Model
{
    public class Station : BaseModel
    {
        public string Title { get; set; }
        public string StreamUrl { get; set; }
        public string PreprocessorFormat { get; set; }
        public string Format { get; set; }

        //Things to statisfy the built-in templates
        public string ImageUrl { get; set; }

        public ImageSource Image { get;  set; }
        public string Subtitle { get; set; }

        public string UnlocalizedFormat { get; set; }

        public string ServerType { get; set; }

        public Uri HomepageUrl { get; set; }

        private StationDisplayType _stationDisplay = StationDisplayType.Main;
        public StationDisplayType StationDisplay
        {
            get { return _stationDisplay; }
            set
            {
                _stationDisplay = value;
            }
        }
    }

    public enum StationDisplayType
    {
        /// <summary>
        /// DecodeHeight = 155 according to StandardStyles.xaml's optimized Standard250x250ItemTemplate template
        /// </summary>
        Main = 0,
        /// <summary>
        /// DecodeHeight/Width = 60 according to StandardStyles.xaml's Standard80ItemTemplate template
        /// </summary>
        Small,
    }

    public class StationGroup : BaseModel //, ISupportIncrementalLoading
    {
        public string Name { get; set; }
        public string UnlocalizedName { get; set; }
        public ObservableCollection<Station> Items { get; set; }
        public ObservableCollection<Station> TopItems { get { if (Items != null) return Items; else return null; } }//{ get { if (Items != null) return Items.Take(5); else return null; } } // ????
    }
}
