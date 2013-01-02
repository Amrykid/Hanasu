using Crystal.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage;

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

        private ImageSource _image = null;
        public ImageSource Image
        {
            get
            {
                if (ImageUrl != null)
                {
                    if (_image == null)
                    {
                        GetCachedImage();
                    }

                    return _image;
                }
                else return null;
            }
        }

        private async Task GetCachedImage()
        {
            var idealName = Title + ImageUrl.Substring(ImageUrl.LastIndexOf("."));

            StorageFile file = null;
            try
            {
                file = await App.AppFolder.GetFileAsync(idealName);

                var prop = await file.GetBasicPropertiesAsync();
                if (prop.Size == 0)
                {
                    await file.DeleteAsync();
                    file = null; //Size = 0, so its not completed.
                }
            }
            catch (Exception)
            {

            }

            if (file == null)
            {
                file = await App.AppFolder.CreateFileAsync(idealName);

                var str = await file.OpenAsync(FileAccessMode.ReadWrite);

                var http = new HttpClient();
                var data = await http.GetByteArrayAsync(ImageUrl);

                await str.WriteAsync(data.AsBuffer());
                await str.FlushAsync();

                str.Dispose();

                http.Dispose();
            }
            //_image = new BitmapImage(new Uri("ms-appdata:///local/Hanasu/" + file.DisplayName, UriKind.RelativeOrAbsolute));

            var thumb = await file.GetThumbnailAsync(Windows.Storage.FileProperties.ThumbnailMode.PicturesView);
            _image = new BitmapImage();
            await ((BitmapImage)_image).SetSourceAsync(thumb);

            if (StationDisplay == StationDisplayType.Main)
                ((BitmapImage)_image).DecodePixelHeight = 155;
            else if (StationDisplay == StationDisplayType.Small)
            {
                ((BitmapImage)_image).DecodePixelHeight = 60;
                ((BitmapImage)_image).DecodePixelWidth = 60;
            }

            await Task.Yield();

            RaisePropertyChanged(z => this.Image);
        }
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

                if (_image != null)
                    GetCachedImage(); //Updates the image with a re-decoded image.
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

    public class StationGroup : Crystal.Dynamic.AutoIPNotifyingBaseModel //, ISupportIncrementalLoading
    {
        public string Name { get; set; }
        public string UnlocalizedName { get; set; }
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
