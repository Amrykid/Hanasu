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
                using (var fs = new FileStream(Instance.LikedSongDBFile, FileMode.OpenOrCreate))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    Instance.LikedSongs = (ObservableCollection<SongData>)bf.Deserialize(fs);
                    fs.Close();
                }

                Instance.OnPropertyChanged("LikedSongs");
            }
            else
            {
                Instance.LikedSongs = new ObservableCollection<SongData>();
            }

            Instance.InitializeInternal();

        }
        private void InitializeInternal()
        {
            Hanasu.Services.Events.EventService.AttachHandler(Events.EventType.Song_Liked, HandleSongLiked);
        }

        public static bool IsInitialized { get; private set; }

        static void Current_Exit(object sender, System.Windows.ExitEventArgs e)
        {
            using (var fs = new FileStream(Instance.LikedSongDBFile, FileMode.OpenOrCreate))
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(fs, Instance.LikedSongs);
                fs.Close();
            }
        }
        private static void HandleSongLiked(EventInfo e)
        {
            MainWindow.SongLikedEventInfo sl = (MainWindow.SongLikedEventInfo)e;

            Instance.LikedSongs.Add(sl.CurrentSong);

            Instance.OnPropertyChanged("LikedSongs");
        }
        public static LikedSongService Instance { get; private set; }

        public string LikedSongDBFile { get; private set; }
        public ObservableCollection<SongData> LikedSongs { get; private set; }

        internal bool IsSongLikedFromString(string name)
        {
            var bits = name.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries);

            var res = LikedSongs.Any(i => (i.TrackTitle == bits[0].Trim(' ') && i.Artist == bits[1].Trim(' '))  || (i.TrackTitle == bits[1].Trim(' ') && i.Artist == bits[0].Trim(' ')));
            return res;
        }
    }
}
