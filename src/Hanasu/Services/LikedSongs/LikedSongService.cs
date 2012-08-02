using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Services.Song;
using System.Collections.ObjectModel;
using Hanasu.Core;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Hanasu.Services.Events;
using System.Runtime.Serialization;
using System.Windows.Data;
using System.Net;
using Hanasu.Windows;

namespace Hanasu.Services.LikedSongs
{
    public class LikedSongService : BaseINPC, IService
    {
        static LikedSongService()
        {

            if (!IsInitialized && Instance == null)
                Initialize();
        }
        public static void Initialize()
        {
            if (IsInitialized && Instance != null) return;

            IsInitialized = true;

            Instance = new LikedSongService();

            System.Windows.Application.Current.Exit += Current_Exit;

            var dir = Hanasu.Services.Stations.StationsService.StationsCacheDir;

            Instance.LikedSongDBFile = dir.Replace("\\Cache", "") + "LikedSongs.bin";

            if (System.IO.File.Exists(Instance.LikedSongDBFile))
            {
                try
                {
                    using (var fs = new FileStream(Instance.LikedSongDBFile, FileMode.OpenOrCreate))
                    {
                        IFormatter bf = new BinaryFormatter();
                        Instance.LikedSongs = (ObservableCollection<SongData>)bf.Deserialize(fs);
                        CheckIfAlbumArtIsCached();
                        fs.Close();
                    }


                    Instance.OnPropertyChanged("LikedSongs");



                }
                catch (Exception)
                {
                    Instance.LikedSongs = new ObservableCollection<SongData>();
                }
            }
            else
            {
                Instance.LikedSongs = new ObservableCollection<SongData>();
            }

            Instance.InitializeInternal();

        }

        private static void CheckIfAlbumArtIsCached()
        {
            var items = Instance.LikedSongs.Where(s => s.AlbumCoverData == null && s.AlbumCoverUri != null).ToArray();
            if (items != null && items.Length != 0)
            {
                var dialog = AsyncTaskDialog.GetNewTaskDialog(null,
                    "Updating Liked Songs Library.",
                    "Caching Album Art...");

                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(t =>
                    {
                        try
                        {
                            foreach (var s in items)
                                if (s.AlbumCoverUri != null)
                                {
                                    SongData moo = s;
                                    using (WebClient wc = new WebClient())
                                    {
                                        moo.AlbumCoverData = wc.DownloadData(s.AlbumCoverUri);
                                    }

                                    var ind = Instance.LikedSongs.FastIndexOf(s);

                                    App.Current.Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                                    {
                                        Instance.LikedSongs[ind] = moo;
                                    }));


                                }
                        }
                        catch (Exception)
                        {
                        }

                        SaveLikedSongsDB();
                        App.Current.Dispatcher.Invoke(new Hanasu.Services.Notifications.NotificationsService.EmptyDelegate(() =>
                            {
                                dialog.Close();
                            }));
                    }));
                //dialog.Owner = App.Current.MainWindow;
                dialog.Show();
            }
        }
        private void InitializeInternal()
        {
            Hanasu.Services.Events.EventService.AttachHandler(Events.EventType.Song_Liked, HandleSongLiked);
        }

        public static bool IsInitialized { get; private set; }

        static void Current_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            if (Instance.LikedSongs.Count > 0)
            {
                //nulls out every song's album data
                //var x = Instance.LikedSongs.ToArray();
                //Instance.LikedSongs.Clear();
                //foreach (var y in x)
                //{
                //    var item = y;
                //    item.AlbumCoverData = null;
                //    Instance.LikedSongs.Add(item);
                //}

                SaveLikedSongsDB();
            }
        }

        internal static void SaveLikedSongsDB()
        {
            using (var fs = new FileStream(Instance.LikedSongDBFile, FileMode.OpenOrCreate))
            {
                IFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, Instance.LikedSongs);
                fs.Close();
            }
        }
        private static void HandleSongLiked(EventInfo e)
        {
            MainWindow.SongLikedEventInfo sl = (MainWindow.SongLikedEventInfo)e;

            if (sl.CurrentSong.TrackTitle == null || sl.CurrentSong.Artist == null) return;

            Instance.LikedSongs.Add(sl.CurrentSong);

            Instance.OnPropertyChanged("LikedSongs");

            // MainWindow mw = (MainWindow)App.Current.MainWindow;

            // var be = BindingOperations.GetBindingExpression(mw.LikedSongsListView, System.Windows.Controls.ListView.ItemsSourceProperty);

            //be.UpdateTarget();
        }
        public static LikedSongService Instance { get; private set; }

        public string LikedSongDBFile { get; private set; }
        public ObservableCollection<SongData> LikedSongs { get; private set; }

        internal bool IsSongLikedFromString(string name)
        {
            if (name.Contains(" - "))
            {
                var bits = name.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);

                var res = LikedSongs.Any(i => (i.TrackTitle == bits[0].Trim(' ') && i.Artist == bits[1].Trim(' ')) || (i.TrackTitle == bits[1].Trim(' ') && i.Artist == bits[0].Trim(' ')) || name == i.OriginallyBroadcastSongData);
                return res;
            }
            else
                return false;
        }
        internal SongData GetSongFromString(string name)
        {
            if (name.Contains(" - "))
            {
                var bits = name.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);

                var res = LikedSongs.First(i => (i.TrackTitle == bits[0].Trim(' ') && i.Artist == bits[1].Trim(' ')) || (i.TrackTitle == bits[1].Trim(' ') && i.Artist == bits[0].Trim(' ')) || i.OriginallyBroadcastSongData == name);

                if (res.TrackTitle == null)
                    throw new Exception("Not found!");
                else
                    return res;
            }
            else
            {
                var res = LikedSongs.First(i => name == i.OriginallyBroadcastSongData);

                if (res.TrackTitle == null)
                    throw new Exception("Not found!");
                else
                    return res;
            }
        }

        internal bool IsLiked(SongData currentSong)
        {
            return Instance.LikedSongs.Any(it => it.TrackTitle == currentSong.TrackTitle && it.Artist == currentSong.Artist);
        }
    }
}
