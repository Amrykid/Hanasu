using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Crystal.Core;
using Hanasu.Model;

namespace Hanasu.ViewModel
{
    public class HomeViewModel : BaseViewModel
    {
        private Regex NewsItemNewsPageImageRegex = new Regex("<img.+?style=\".+?border:1px solid #555;\".+?src=\"(?<src>.+?)\".+?>", RegexOptions.Compiled | RegexOptions.Singleline);

        public HomeViewModel()
        {
            Initialize();
        }

        private async void Initialize()
        {
            //load news
            await LoadNewsFeed();

            //load charts?
        }

        public async Task LoadNewsFeed()
        {
            if (NewsItems == null)
                NewsItems = new ObservableCollection<NewsItem>();

            //load and parse the rss xml.
            var feed = "http://www.jpopasia.com/rss/jpopasia.xml";
            XDocument doc = await Task.Run(() => XDocument.Load(feed)); //do this operation on another thread because it /might/ take a while.

            //convert each item-node into an NewsItem using LINQ-to-XML.
            var items = from x in doc.Element("rss").Element("channel").Elements("item")
                        select new NewsItem()
                        {
                            Title = x.Element("title").Value,
                            Description = x.Element("description").Value,
                            Url = x.Element("link").Value,
                            PublishDate = DateTime.Parse(x.Element("pubdate").Value)
                        };
            using (HttpClient http = new HttpClient())
            {
                foreach (var item in items.Take(5))
                {
                    if (!NewsItems.Any(x => x.Title == item.Title))
                    {
                        var html = await http.GetStringAsync(item.Url);
                        var srcMatch = NewsItemNewsPageImageRegex.Match(html);
                        var src = srcMatch.Groups["src"].Value;
                        item.ImageUrl = src;

                        NewsItems.Add(item);
                    }
                }
            }
        }
        public ObservableCollection<NewsItem> NewsItems
        {
            get { return GetPropertyOrDefaultType<ObservableCollection<NewsItem>>(x => this.NewsItems); }
            set { SetProperty(x => this.NewsItems, value); }
        }
    }
}
