using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core.ArtistService.Artist_Data_Sources;
using Hanasu.Core.Songs;

namespace Hanasu.Core.ArtistService
{
    public class ArtistService
    {
        internal ArtistService()
        {
            DataSource = new JPopAsia();
        }

        public IArtistInfoDataSource DataSource { get; set; }
        public IArtistInfoDataSource[] AllDataSources { get { return new IArtistInfoDataSource[] { DataSource }; } }

        public ArtistInfo FindArtist(SongData song)
        {
            return FindArtist(song.Artist);
        }
        public ArtistInfo FindArtist(string name)
        {
            return DataSource.GetArtistFromName(name);
        }
    }
}
