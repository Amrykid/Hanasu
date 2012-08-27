using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hanasu.Core.Songs
{
    public interface IAlbumInfoDataSource
    {
        bool GetAlbumInfo(ref SongData song);
        string WebsiteName { get; }
    }
}
