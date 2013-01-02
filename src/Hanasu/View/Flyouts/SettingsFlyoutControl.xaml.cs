using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Hanasu.View.Flyouts
{
    public sealed partial class SettingsFlyoutControl : UserControl
    {
        public SettingsFlyoutControl()
        {
            this.InitializeComponent();
            UseLightThemeToggle.IsOn = App.HanasuAppSettings.PreferredApplicationTheme == "Light" ? true : false;



            var color = Hanasu.Tools.ColorHelper.GetColorFromHexString(App.HanasuAppSettings.PreferredChromeBackgroundColor);
            ASlider.Value = color.A;
            RSlider.Value = color.R;
            GSlider.Value = color.G;
            BSlider.Value = color.B;
            showColor.Fill = new SolidColorBrush(color);


            this.Unloaded += SettingsFlyoutControl_Unloaded;
        }

        void SettingsFlyoutControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded -= SettingsFlyoutControl_Unloaded;

            App.SaveSettings();
        }

        private void UseLightThemeToggle_Toggled(object sender, RoutedEventArgs e)
        {
            App.HanasuAppSettings.PreferredApplicationTheme = UseLightThemeToggle.IsOn ? "Light" : "Dark";
        }


        private void Slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            //http://social.msdn.microsoft.com/Forums/en-US/winappswithcsharp/thread/1cb9c5b9-3ef6-4c88-b747-ae222c38c922/
            byte R, G, B, A;

            A = Convert.ToByte(ASlider.Value);
            R = Convert.ToByte(RSlider.Value);
            G = Convert.ToByte(GSlider.Value);
            B = Convert.ToByte(BSlider.Value);

            Color myColor = new Color();
            myColor = Color.FromArgb(A, R, G, B);

            showColor.Fill = new SolidColorBrush(myColor);

            //set the settings
            App.HanasuAppSettings.PreferredChromeBackgroundColor = myColor.ToString();
        }
    }
}
