﻿using Crystal.Core;
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
            }
            _image = new BitmapImage(new Uri("ms-appdata:///local/Hanasu/" + file.DisplayName, UriKind.RelativeOrAbsolute));

            RaisePropertyChanged(z => this.Image);
        }
        public string Subtitle { get; set; }

        public string UnlocalizedFormat { get; set; }

        public string ServerType { get; set; }
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
