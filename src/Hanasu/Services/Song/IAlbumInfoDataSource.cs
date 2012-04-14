using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Services.Song
{
    public interface IAlbumInfoDataSource
    {
        bool GetAlbumInfo(ref SongData song);
        string WebsiteName { get; }
    }
}
