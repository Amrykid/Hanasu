using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace Hanasu.Services.Song.Album_Info_Data_Source
{
    public class MSMetaServices : IAlbumInfoDataSource
    {
        public bool GetAlbumInfo(ref SongData song)
        {
            var url = String.Format(
                "http://fai.music.metaservices.microsoft.com/ZuneAPI/Search.aspx?SearchString={0}&locale=1033&maxNumberOfResults=100&resultTypeString=album&countOnly=false",
                String.Join(" ",
                    song.Artist,
                    song.TrackTitle));

            using (WebClient wc = new WebClient())
            {
                var inital = wc.DownloadString(url);

                var doc = XDocument.Parse(inital);

                var search = (doc.FirstNode as XElement).Element("MDSR-CD");

                var returncode = search.Element("ReturnCode");

                if (returncode.Value == "SUCCESS")
                {
                    var results = search.Element("SearchResult").Elements("Result");

                    var firstresult = results.First();

                    if (firstresult.Element("albumPerformer").Value.ToLower() == song.Artist.ToLower() || song.Artist.ToLower().StartsWith(firstresult.Element("albumPerformer").Value.ToLower()))
                    {
                        song.Album = firstresult.Element("albumFullTitle").Value;

                        var albumID = firstresult.Element("id_album");

                        var moredata = wc.DownloadString("http://fai.music.metaservices.microsoft.com/ZuneAPI/GetAlbumDetailsFromAlbumId.aspx?albumId=" + albumID.Value + "&locale=1033&volume=1");

                        var doc2 = XDocument.Parse(moredata);

                        var cover = doc2.Element("METADATA").Element("MDAR-CD").Element("LargeCoverArtURL");

                        if (cover.Value == "")
                        {
                        }
                        else
                        {

                            song.AlbumCoverUri = new Uri("http://images.metaservices.microsoft.com/cover/" + cover.Value);
                        }
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        public string WebsiteName
        {
            get { return "Microsoft Metaservices / Zune API"; }
        }
    }
}

/*
http://pastebin.com/VjFApeJ5

Getting data the Zune metadata API

There is no API key required. On various places on the internet, developers handle it as an 'unofficial' and unsupported API.

Get artists:
http://fai.music.metaservices.microsoft.com/ZuneAPI/Search.aspx?SearchString=The%20Fame%20Monster%20%28Deluxe%20Edition%29%20Lady%20Gaga&locale=1033&maxNumberOfResults=100&resultTypeString=artist&countOnly=false

get albums:
http://fai.music.metaservices.microsoft.com/ZuneAPI/Search.aspx?SearchString=The%20Fame%20Monster%20%28Deluxe%20Edition%29%20Lady%20Gaga&locale=1033&maxNumberOfResults=100&resultTypeString=album&countOnly=false

read the <id_album> tag to get details:
http://fai.music.metaservices.microsoft.com/ZuneAPI/GetAlbumDetailsFromAlbumId.aspx?albumId=225306764&locale=1033&volume=1
                                             ^
This is the album id-------------------------|

There, get the <LargeCoverArtURL> tag to get the cover image:
http://images.metaservices.microsoft.com/cover/200/drN400/N435/N43546C3IRM.jpg
*/