using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

namespace Hanasu.Services.Notifications
{
    public class NotificationInfo
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public int Duration { get; set; }
        public bool IsUrgent { get; set; }
        public object ImageDataSource { get; set; }
        public System.Windows.Media.ImageSource Image
        {
            get
            {
                if (ImageDataSource is string || ImageDataSource is Uri)
                    return (System.Windows.Media.ImageSource)new Hanasu.Converters.UriToBitmapImageConverter().Convert(ImageDataSource, null, null, null);
                //else if (ImageDataSource is byte[])
                //    return (System.Windows.Media.ImageSource)new Hanasu.Converters.ByteArrayToImageConverter().Convert(ImageDataSource, null, null, null);
                else
                    return null;
            }
        }
        public NotificationType Type { get; set; }
        public Action<NotificationInfo> OnClickCallback { get; set; }
    }
}
