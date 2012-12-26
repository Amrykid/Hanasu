using Crystal.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
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
        public ImageSource Image { get { if (ImageUrl != null) return new BitmapImage(new Uri(ImageUrl)) { DecodePixelWidth = 250 }; else return null; } }
        public string Subtitle { get; set; }
    }

    public class StationGroup : Crystal.Dynamic.AutoIPNotifyingBaseModel //, ISupportIncrementalLoading
    {
        public string Name { get; set; }
        public ObservableCollection<Station> Items { get; set; }
        public ObservableCollection<Station> TopItems { get { if (Items != null) return Items; else return null; } }//{ get { if (Items != null) return Items.Take(5); else return null; } } // ????


        //#region ISupportIncrementalLoading from http://www.silverlightshow.net/items/Windows-8-metro-Improve-GridView-and-ListView-with-SemanticZoom-and-Incremental-Loading.aspx
        //public Func<Station, bool> Selector { get; set; }

        //public bool HasMoreItems
        //{
        //    get { return TopItems.Count != Items.Count; }
        //}

        //public Windows.Foundation.IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        //{
        //    CoreDispatcher dispatcher = Window.Current.Dispatcher;

        //    return Task.Run<LoadMoreItemsResult>(
        //        () =>
        //        {
        //            var items = Items.Take((int)count);

        //            dispatcher.RunAsync(CoreDispatcherPriority.Normal,
        //                  () =>
        //                  {
        //                      foreach (var item in items)
        //                          if (this.Selector(item))
        //                              TopItems.Add(item);
        //                  });

        //            return new LoadMoreItemsResult() { Count = 100 };
        //        }).AsAsyncOperation<LoadMoreItemsResult>();
        //}
        //#endregion
    }
}
