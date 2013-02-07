using Hanasu.Core.Preprocessor;
using Hanasu.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Hanasu.Extensions;

namespace Hanasu.Tools.Preprocessing.Preprocessors.ASX
{
    public class ASXPreprocessor : MultiStreamPreprocessor
    {
        public override async Task<IMultiStreamEntry[]> Parse(Uri url)
        {
            var list = new List<IMultiStreamEntry>();
            string data = null;


            data = await HtmlTextUtility.GetHtmlFromUrl2(url.ToString(), false);

            XDocument doc = XDocument.Parse(data); //Yes, I know I could have used Load but I needed this to be an asynchronous method.

            XElement head = null;

            head = doc.Element("asx");
            if (head == null)
                head = doc.Element("Asx");


            foreach (var item in from x in head.Elements("entry")
                                 select new ASXEntry()
                                 {
                                     File = x.Element("ref").Attribute("href").Value
                                 })
                list.Add(item);

            return list.ToArray();
        }

        public override bool Supports(Uri url)
        {
            return url.Segments.Last().ToLower().EndsWith(Extension);
        }

        public override async Task<Uri> Process(Uri url)
        {
            url = new Uri((await Parse(url))[0].File, UriKind.Absolute);

            return url;
        }

        public override string Extension
        {
            get { return ".asx"; }
        }
    }

    public struct ASXEntry : IMultiStreamEntry
    {
        public string File { get; set; }
        public string Title { get; set; }
        public int Length { get; set; }
        public int ID { get; set; }
    }
}
