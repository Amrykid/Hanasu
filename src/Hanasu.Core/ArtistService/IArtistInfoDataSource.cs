using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Core.ArtistService
{
    public interface IArtistInfoDataSource
    {
        string WebsiteName { get; }
        Uri WebsiteUrl { get; }
        ArtistInfo GetArtistFromName(string name);
        ArtistInfo GetArtistFromSong(Songs.SongData song);
    }
}
