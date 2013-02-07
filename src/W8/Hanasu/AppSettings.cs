using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using System.Reflection;

namespace Hanasu
{
    public class AppSettings
    {
        /// <summary>
        /// Windows.UI.Color
        /// </summary>
        public string PreferredChromeBackgroundColor { get; set; }
        /// <summary>
        /// Windows.UI.Xaml.ApplicationTheme
        /// </summary>
        public string PreferredApplicationTheme { get; set; }

        public void IteratePropertiesAndValues(Action<string, object> act)
        {
            if (act == null) throw new ArgumentNullException("act");

            foreach (var property in this.GetType().GetTypeInfo().DeclaredProperties)
            {
                act(property.Name, property.GetValue(this));
            }
        }
        public void SetProperty(string property, object value)
        {
            this.GetType().GetTypeInfo().DeclaredProperties.First(x => x.Name == property).SetValue(this, value);
        }
    }
}
