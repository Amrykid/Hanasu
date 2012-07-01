using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MahApps.Metro.Controls;

namespace Hanasu.Services.Settings
{
    public static class SettingsThemeHelper
    {
        public static void ApplyThemeAccordingToSettings(MetroWindow win)
        {
            MahApps.Metro.ThemeManager.ChangeTheme(win,
                new MahApps.Metro.Accent(
                    (string)Enum.GetName(typeof(SettingsThemes),
                    Settings.SettingsService.Instance.Theme),
                    new Uri("pack://application:,,,/MahApps.Metro;component/Styles/Accents/" + Enum.GetName(typeof(SettingsThemes), Settings.SettingsService.Instance.Theme) + ".xaml", UriKind.Absolute))
                , MahApps.Metro.Theme.Light);
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
