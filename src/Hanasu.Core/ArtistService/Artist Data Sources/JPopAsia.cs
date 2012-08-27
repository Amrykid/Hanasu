using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core.Utilities;
using System.Text.RegularExpressions;

namespace Hanasu.Core.ArtistService.Artist_Data_Sources
{
    public class JPopAsia : IArtistInfoDataSource
    {
        public string WebsiteName
        {
            get { return "jpopasia"; }
        }

        public Uri WebsiteUrl
        {
            get { return new Uri("http://www.jpopasia.com/"); }
        }

        public ArtistInfo GetArtistFromName(string name)
        {
            try
            {
                ArtistInfo res = new ArtistInfo();

                string url = null;
                string html = null;
                bool issolo = true;

                name = name.ToLower().Replace("-", "").Replace(" ", "_");

                if (name.Where(x => x == '_').Count() > 1)
                    name = name.Replace("_", "");

                name = name.TrimEnd('_');

                try
                {
                    url = WebsiteUrl + "group/" + name; //have to guess whether its a group or a solo person.

                    bool redirected;
                    html = HtmlTextUtility.GetHtmlFromUrl2(
                       HtmlTextUtility.UrlEncode(url), out redirected);

                    issolo = redirected;
                }
                catch (Exception)
                {
                    try
                    {
                        url = WebsiteUrl + "celebrity/" + name;

                        html = HtmlTextUtility.GetHtmlFromUrl2(
                           HtmlTextUtility.UrlEncode(url));

                        issolo = true;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            url = WebsiteUrl + "celebrity/" + string.Join("_", name.Split('_').Reverse());

                            html = HtmlTextUtility.GetHtmlFromUrl2(
                               HtmlTextUtility.UrlEncode(url));

                            issolo = true;
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                }

                

                var biotext = Regex.Match(html, "<div id=\"bio-summary\".+?>.+?</div>", RegexOptions.Compiled | RegexOptions.Singleline);

                res.Bio = Regex.Replace(biotext.Value, "<.+?>", "", RegexOptions.Singleline | RegexOptions.Compiled);

                res.Bio = HtmlTextUtility.Decode(res.Bio.Trim());

                res.Type = issolo ? ArtistType.Solo : ArtistType.Group;

                var infobox = Regex.Match(html,"<table class=\"pretty clear\".+?>.+?</table>",RegexOptions.Compiled | RegexOptions.Singleline);
                var infobox_rows = Regex.Matches(infobox.Value, "<td>.+?</td>",RegexOptions.Singleline | RegexOptions.Compiled);

                switch (issolo)
                {
                    case true:
                        {
                            res.SOLO_Realname = infobox_rows[0].Value;
                            res.SOLO_Gender = ArtistGender.XAMPP;
                        }
                        break;
                    case false:
                        {
                        }
                        break;
                }


                return res;
            }
            catch (Exception ex)
            {
                throw new Exception("Not found!", ex);
            }
        }

        public ArtistInfo GetArtistFromSong(Songs.SongData song)
        {
            if (song.Artist == null) throw new ArgumentNullException("song.Artist");

            return GetArtistFromName(song.Artist);
        }
    }
}
