using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Core.Utilities;
using System.Text.RegularExpressions;
using System.Net;

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

                //name = name.ToLower().Replace("-", "").Replace(" ", "_");

                //if (name.Where(x => x == '_').Count() > 1)
                //    name = name.Replace("_", "");

                //name = name.TrimEnd('_');

                //try
                //{
                //    url = WebsiteUrl + "group/" + name; //have to guess whether its a group or a solo person.

                //    bool redirected;
                //    html = HtmlTextUtility.GetHtmlFromUrl2(
                //       HtmlTextUtility.UrlEncode(url), out redirected);

                //    issolo = redirected;
                //}
                //catch (Exception)
                //{
                //    try
                //    {
                //        url = WebsiteUrl + "celebrity/" + name;

                //        html = HtmlTextUtility.GetHtmlFromUrl2(
                //           HtmlTextUtility.UrlEncode(url));

                //        issolo = true;
                //    }
                //    catch (Exception)
                //    {
                //        try
                //        {
                //            url = WebsiteUrl + "celebrity/" + string.Join("_", name.Split('_').Reverse());

                //            html = HtmlTextUtility.GetHtmlFromUrl2(
                //               HtmlTextUtility.UrlEncode(url));

                //            issolo = true;
                //        }
                //        catch (Exception)
                //        {
                //            throw;
                //        }
                //    }
                //}


                url = WebsiteUrl + "search/?q=" + HtmlTextUtility.UrlEncode(name);

                try
                {
                    html = HtmlTextUtility.GetHtmlFromUrl2(url);
                }
                catch (WebException ex)
                {
                    throw;
                }

                var results = Regex.Matches(html, "<div class=\"content box\">.+?</div>.+?</div>.+?</div>", RegexOptions.Compiled | RegexOptions.Singleline);
                //var firstresult = Regex.Match(results[0].Value, "<li style=\"margin: 5px 20px;\">.+?</li>", RegexOptions.Compiled | RegexOptions.Singleline);
                var firstresulturl = Regex.Match(results[0].Value, "href=\".+?\"", RegexOptions.Singleline | RegexOptions.Compiled).Value;
                firstresulturl = firstresulturl.Substring(6);
                firstresulturl = firstresulturl.Trim('\"');

                url = firstresulturl;

                html = HtmlTextUtility.GetHtmlFromUrl2(url);

                res.Name = Regex.Replace(
                    Regex.Match(html,
                    "<a href=\"" + url + "\">.+?</a>", RegexOptions.Singleline | RegexOptions.Compiled).Value, "<.+?>", "", RegexOptions.Compiled | RegexOptions.Singleline);

                var biotext = Regex.Match(html, "<div id=\"bio-summary\".+?>.+?</div>", RegexOptions.Compiled | RegexOptions.Singleline);

                res.Bio = Regex.Replace(biotext.Value, "<.+?>", "", RegexOptions.Singleline | RegexOptions.Compiled);

                res.Bio = HtmlTextUtility.Decode(res.Bio.Trim());

                issolo = url.StartsWith(WebsiteUrl + "group") == false;

                res.Type = issolo ? ArtistType.Solo : ArtistType.Group;

                try
                {
                    var imgurl = Regex.Match(html, "<img.+?title=\"" + res.Name + "\".+?>").Value;
                    imgurl = Regex.Match(imgurl, "src=\".+?\"").Value;
                    imgurl = imgurl.Substring(5).TrimEnd('\"');

                    res.ArtistImageUrl = new Uri(imgurl);
                }
                catch (Exception)
                {
                    //No image? no big deal.
                }

                var infobox = Regex.Match(html, "<table class=\"pretty clear\".+?>.+?</table>", RegexOptions.Compiled | RegexOptions.Singleline);
                var infobox_rows = Regex.Matches(infobox.Value, "<td(.+?)?>.+?</td>", RegexOptions.Singleline | RegexOptions.Compiled);

                switch (issolo)
                {
                    case true:
                        {
                            res.SOLO_Realname = Regex.Replace(infobox_rows[1].Value, "<.+?>", "", RegexOptions.Compiled | RegexOptions.Singleline).Trim();

                            for (int i = 2; i < infobox_rows.Count; i++)
                            {
                                if (infobox_rows[i].Value.Contains(">Gender</td>"))
                                {
                                    var geninfo = Regex.Replace(Regex.Match(infobox_rows[i + 1].Value, "<td>.+?</td>", RegexOptions.Singleline | RegexOptions.Compiled).Value,
                                        "<.+?>", "", RegexOptions.Compiled | RegexOptions.Singleline);

                                    if (geninfo != "Birthday")
                                        res.SOLO_Gender = (ArtistGender)Enum.Parse(typeof(ArtistGender), geninfo, true);
                                    i++;
                                }

                                if (infobox_rows[i].Value.Contains(">Birthday</td>"))
                                {
                                    var bdayinfo = Regex.Replace(Regex.Match(infobox_rows[i + 1].Value, "<td>.+?</td>", RegexOptions.Singleline | RegexOptions.Compiled).Value,
                                        "<.+?>", "", RegexOptions.Compiled | RegexOptions.Singleline);

                                    if (bdayinfo != "00 00, 0000")
                                        res.SOLO_Birthday = DateTime.Parse(bdayinfo);
                                    i++;
                                }

                                if (infobox_rows[i].Value.Contains(">Debut</td>"))
                                {
                                    var dat = Regex.Replace(Regex.Match(infobox_rows[i + 1].Value, "<td>.+?</td>", RegexOptions.Singleline | RegexOptions.Compiled).Value.Trim(),
                                        "<(/)?span.+?(/)?>", "", RegexOptions.Compiled | RegexOptions.Singleline);

                                    dat = Regex.Replace(dat,"(<|\\().+?(>|\\))","",RegexOptions.Singleline | RegexOptions.Compiled);

                                    try
                                    {
                                        res.Debut = DateTime.Parse(dat);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                    i++;
                                }
                            }
                        }
                        break;
                    case false:
                        {
                            res.GROUP_BandName = Regex.Replace(infobox_rows[1].Value, "<.+?>", "", RegexOptions.Compiled | RegexOptions.Singleline).Trim();

                            for (int i = 2; i < infobox_rows.Count; i++)
                            {

                                if (infobox_rows[i].Value.Contains(">Band type</td>"))
                                {
                                    res.GROUP_BandType = Regex.Replace(Regex.Match(infobox_rows[i + 1].Value, "<td>.+?</td>", RegexOptions.Singleline | RegexOptions.Compiled).Value,
                                        "<.+?>", "", RegexOptions.Compiled | RegexOptions.Singleline);
                                    i++;
                                }

                                if (infobox_rows[i].Value.Contains(">Debut</td>"))
                                {
                                    var dat = Regex.Replace(Regex.Match(infobox_rows[i + 1].Value, "<td>.+?</td>", RegexOptions.Singleline | RegexOptions.Compiled).Value.Trim(),
                                        "<(/)?span.+?(/)?>", "", RegexOptions.Compiled | RegexOptions.Singleline);

                                    dat = Regex.Replace(dat,"(<|\\().+?(>|\\))","",RegexOptions.Singleline | RegexOptions.Compiled);

                                    try
                                    {
                                        res.Debut = DateTime.Parse(dat);
                                    }
                                    catch (Exception)
                                    {
                                    }
                                    i++;
                                }
                            }
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
