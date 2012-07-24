using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MahApps.Metro.Controls;
using System.Windows.Media;

namespace Hanasu.Services.Settings
{
    public static class SettingsThemeHelper
    {
        public static void ApplyThemeAccordingToSettings(MetroWindow win)
        {
            MahApps.Metro.Theme style = Hanasu.Services.Settings.SettingsService.Instance.IsLightTheme ? MahApps.Metro.Theme.Light : MahApps.Metro.Theme.Dark;

            MahApps.Metro.ThemeManager.ChangeTheme(win,
                new MahApps.Metro.Accent(
                    (string)Enum.GetName(typeof(SettingsThemes),
                    Settings.SettingsService.Instance.Theme),
                    new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/" + Enum.GetName(typeof(SettingsThemes), Settings.SettingsService.Instance.Theme) + ".xaml", UriKind.Absolute))
                , style);

            if (win is MainWindow)
            {
                var BackgroundImageBrush = ((MainWindow)win).BackgroundImageBrush;
                if (Hanasu.Services.Settings.SettingsService.Instance.DisplayBackgroundTheme)
                {
                    if (Hanasu.Services.Settings.SettingsService.Instance.IsLightTheme)
                    {
                        switch (Hanasu.Services.Settings.SettingsService.Instance.Theme)
                        {
                            case Services.Settings.SettingsThemes.Blue:
                                BackgroundImageBrush.ImageSource = (ImageSource)new Hanasu.Core.UriToBitmapImageConverter().Convert(new Uri("pack://application:,,,/Hanasu;component/Resources/LightBG/hatsune-miku-10639-2560x1600.jpg"), null, null, null);
                                break;
                            case Services.Settings.SettingsThemes.Orange:
                                BackgroundImageBrush.ImageSource = (ImageSource)new Hanasu.Core.UriToBitmapImageConverter().Convert(new Uri("pack://application:,,,/Hanasu;component/Resources/LightBG/lily.jpg"), null, null, null);
                                break;
                            case Services.Settings.SettingsThemes.Purple:
                                BackgroundImageBrush.ImageSource = (ImageSource)new Hanasu.Core.UriToBitmapImageConverter().Convert(new Uri("pack://application:,,,/Hanasu;component/Resources/LightBG/konachan-com-52624-hatsune_miku-kaito-vocaloid.jpg"), null, null, null);
                                break;
                            case Services.Settings.SettingsThemes.Green:
                                BackgroundImageBrush.ImageSource = (ImageSource)new Hanasu.Core.UriToBitmapImageConverter().Convert(new Uri("pack://application:,,,/Hanasu;component/Resources/LightBG/MegpoidGumionVocals.png"), null, null, null);
                                break;
                            case Services.Settings.SettingsThemes.Red:
                            default:
                                BackgroundImageBrush.ImageSource = (ImageSource)new Hanasu.Core.UriToBitmapImageConverter().Convert(new Uri("pack://application:,,,/Hanasu;component/Resources/LightBG/luka.jpg"), null, null, null);
                                break;
                        }
                    }
                    else
                    {
                        //apply bgs for dark themes.
                    }
                }
                else
                    BackgroundImageBrush.ImageSource = null;
            }
        }
    }
    public enum SettingsThemes
    {
        Red = 0,
        Blue = 1,
        Orange = 3,
        Purple = 4,
        Green = 5
    }
}
