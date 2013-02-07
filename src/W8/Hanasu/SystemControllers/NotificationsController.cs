using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace Hanasu.SystemControllers
{
    public static class NotificationsController
    {
        static NotificationsController()
        {
            toaster = ToastNotificationManager.CreateToastNotifier();
            tileUpdater = TileUpdateManager.CreateTileUpdaterForApplication();
        }
        private static ToastNotifier toaster = null;
        private static TileUpdater tileUpdater = null;
        public static void SendToast(string txt, string imgurl = "")
        {
            if (toaster.Setting != NotificationSetting.Enabled) return;

            // It is possible to start from an existing template and modify what is needed.
            // Alternatively you can construct the XML from scratch.
            var toastXml = new Windows.Data.Xml.Dom.XmlDocument();
            var title = toastXml.CreateElement("toast");
            var visual = toastXml.CreateElement("visual");
            visual.SetAttribute("version", "1");
            visual.SetAttribute("lang", "en-US");

            // The template is set to be a ToastImageAndText01. This tells the toast notification manager what to expect next.
            var binding = toastXml.CreateElement("binding");
            binding.SetAttribute("template", "ToastImageAndText01");

            // An image element is then created under the ToastImageAndText01 XML node. The path to the image is specified
            var image = toastXml.CreateElement("image");
            image.SetAttribute("id", "1");
            image.SetAttribute("src", imgurl);

            // A text element is created under the ToastImageAndText01 XML node.
            var text = toastXml.CreateElement("text");
            text.SetAttribute("id", "1");
            text.InnerText = txt;

            // All the XML elements are chained up together.
            title.AppendChild(visual);
            visual.AppendChild(binding);
            binding.AppendChild(image);
            binding.AppendChild(text);

            toastXml.AppendChild(title);

            // Create a ToastNotification from our XML, and send it to the Toast Notification Manager
            var toast = new ToastNotification(toastXml);
            toaster.Show(toast);
        }

        public static void SendTileUpdate(string text, string imgurl = "")
        {
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWideImageAndText01);

            XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            tileTextAttributes[0].InnerText = text;

            XmlNodeList tileImageAttributes = tileXml.GetElementsByTagName("image");
            ((XmlElement)tileImageAttributes[0]).SetAttribute("src", imgurl);
            ((XmlElement)tileImageAttributes[0]).SetAttribute("alt", "red graphic");

            XmlDocument squareTileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquareText04);
            XmlNodeList squareTileTextAttributes = squareTileXml.GetElementsByTagName("text");
            squareTileTextAttributes[0].AppendChild(squareTileXml.CreateTextNode(text));
            IXmlNode node = tileXml.ImportNode(squareTileXml.GetElementsByTagName("binding").Item(0), true);
            tileXml.GetElementsByTagName("visual").Item(0).AppendChild(node);

            TileNotification tileNotification = new TileNotification(tileXml);

            tileNotification.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(10);

            tileUpdater.Update(tileNotification);
        }

        public static void ClearTile()
        {
            tileUpdater.Clear();
        }
    }
}
